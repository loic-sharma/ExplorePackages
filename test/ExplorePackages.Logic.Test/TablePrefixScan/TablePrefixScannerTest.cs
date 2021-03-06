﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Xunit;
using Xunit.Abstractions;

namespace Knapcode.ExplorePackages.TablePrefixScan
{
    public class TablePrefixScannerTest : IClassFixture<TablePrefixScannerTest.Fixture>
    {
        [Fact]
        public async Task AllowsProjection()
        {
            (var table, var expected) = await _fixture.SortAndInsertAsync(GenerateTestEntities("PK", 2, 2));
            MinSelectColumns = new[] { nameof(TestEntity.FieldB) };

            var actual = await Target.ListAsync<TestEntity>(table, string.Empty, MinSelectColumns, takeCount: 1);

            Assert.Equal(expected.Count, actual.Count);
            Assert.All(expected.Zip(actual), (pair) =>
            {
                // Built-in properties
                Assert.Equal(pair.First.PartitionKey, pair.Second.PartitionKey);
                Assert.Equal(pair.First.RowKey, pair.Second.RowKey);
                Assert.Equal(pair.First.Timestamp, pair.Second.Timestamp);
                Assert.Equal(pair.First.ETag, pair.Second.ETag);

                // Custom properties
                Assert.Null(pair.Second.FieldA);
                Assert.Equal(pair.First.FieldB, pair.Second.FieldB);
            });
        }

        [Theory]
        [InlineData(1, 1, 1, "[AA] [AB] [AC] [AD] [AE] [AF] [AG] [AH] [BI] [BJ] [BK] [BL] [BM] [BN] [BO] [BP]")]
        [InlineData(1, 2, 1, "[AA, AB] [AC, AD] [AE] [AF] [AG] [AH] [BI] [BJ, BK] [BL] [BM] [BN] [BO] [BP]")]
        [InlineData(1, 2, 3, "[AA, AB] [AC, AD] [AE, AF, AG] [AH] [BI, BJ, BK] [BL, BM] [BN, BO, BP]")]
        [InlineData(1, 3, 2, "[AA, AB, AC] [AD, AE, AF] [AG, AH] [BI, BJ] [BK, BL, BM] [BN, BO] [BP]")]
        [InlineData(2, 1, 3, "[AA, AB] [AC, AD] [AE, AF, AG, AH] [BI, BJ, BK, BL, BM, BN] [BO, BP]")]
        [InlineData(2, 3, 1, "[AA, AB, AC, AD, AE, AF] [AG, AH] [BI, BJ] [BK, BL, BM, BN, BO, BP]")]
        [InlineData(3, 1, 2, "[AA, AB, AC] [AD, AE, AF] [AG, AH] [BI, BJ, BK, BL, BM, BN] [BO, BP]")]
        [InlineData(3, 2, 1, "[AA, AB, AC, AD, AE, AF] [AG, AH] [BI, BJ, BK] [BL, BM, BN, BO, BP]")]
        public async Task AllowsMultipleSegmentsPerPrefix(int takeCount, int segmentsPerFirstPrefix, int segmentsPerSubsequentPrefix, string expected)
        {
            (var table, var all) = await _fixture.SortAndInsertAsync(new[]
            {
                new TestEntity("AA", "1"),
                new TestEntity("AB", "1"),
                new TestEntity("AC", "1"),
                new TestEntity("AD", "1"),
                new TestEntity("AE", "1"),
                new TestEntity("AF", "1"),
                new TestEntity("AG", "1"),
                new TestEntity("AH", "1"),
                new TestEntity("BI", "1"),
                new TestEntity("BJ", "1"),
                new TestEntity("BK", "1"),
                new TestEntity("BL", "1"),
                new TestEntity("BM", "1"),
                new TestEntity("BN", "1"),
                new TestEntity("BO", "1"),
                new TestEntity("BP", "1"),
            });

            var segments = await Target.ListSegmentsAsync<TestEntity>(table, string.Empty, MinSelectColumns, takeCount, segmentsPerFirstPrefix, segmentsPerSubsequentPrefix);

            var actual = string.Join(" ", segments.Select(x => "[" + string.Join(", ", x.Select(x => x.PartitionKey)) + "]"));
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("")]
        [InlineData("P")]
        public async Task EnumeratesEmptyTable(string prefix)
        {
            (var table, var _) = await _fixture.SortAndInsertAsync(Enumerable.Empty<TestEntity>());

            var actual = await Target.ListAsync<TestEntity>(table, prefix, MinSelectColumns, StorageUtility.MaxTakeCount);

            Assert.Empty(actual);
        }

        [Theory]
        [InlineData("")]
        [InlineData("P")]
        public async Task HandlesSurrogatePairs(string prefix)
        {
            // For some reason the Azure Storage Emulator sorts the '𐐷' character before 'A'. Real Azure Table Storage
            // does not do this.
            var isStorageEmulator = _fixture.StorageAccount == CloudStorageAccount.DevelopmentStorageAccount;
            var hackPrefix = isStorageEmulator ? "A" : string.Empty;

            (var table, var expected) = await _fixture.SortAndInsertAsync(new[]
            {
                new TestEntity(prefix + "B", "R1"),
                new TestEntity(prefix + "C", "R1"),
                new TestEntity(prefix + hackPrefix + "𐐷", "R1"),
                new TestEntity(prefix + hackPrefix + "𐐷", "R2"),
                new TestEntity(prefix + hackPrefix + "𐐷", "R3"),
                new TestEntity(prefix + "𤭢a", "R1"),
                new TestEntity(prefix + "𤭢b", "R2"),
                new TestEntity(prefix + "𤭢c", "R3"),
                new TestEntity(prefix + "𤭣", "R1"),
                new TestEntity(prefix + "𤭣", "R2"),
                new TestEntity(prefix + "𤭣", "R3"),
            });

            var actual = await Target.ListAsync<TestEntity>(table, prefix, MinSelectColumns, takeCount: 1);

            Assert.Equal(expected, actual);
        }

        [Theory]
        [MemberData(nameof(EnumeratesEmptyPrefixTestData))]
        public async Task EnumeratesEmptyPrefix(string prefix, int takeCount)
        {
            (var table, var _) = await _fixture.SortAndInsertAsync(new[]
            {
                new TestEntity("B", "R1"),
                new TestEntity("D", "R1"),
            });

            var actual = await Target.ListAsync<TestEntity>(table, prefix, MinSelectColumns, takeCount);

            Assert.Empty(actual);
        }

        public static IEnumerable<object[]> EnumeratesEmptyPrefixTestData
        {
            get
            {
                foreach (var prefix in new[] { "A", "C", "E" })
                {
                    foreach (var takeCount in new[] { 1, 2, 1000 })
                    {
                        yield return new object[] { prefix, takeCount };
                    }
                }
            }
        }

        [Fact]
        public async Task EnumeratesAllEntitiesInSinglePage()
        {
            (var table, var expected) = await _fixture.SortAndInsertAsync(GenerateTestEntities("PK", 3, 2));

            var actual = await Target.ListAsync<TestEntity>(table, string.Empty, MinSelectColumns, StorageUtility.MaxTakeCount);

            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        public async Task EnumeratesAllEntitiesWithNoPrefix(int takeCount)
        {
            (var table, var expected) = await _fixture.SortAndInsertAsync(GenerateTestEntities("PK", 3, 3));

            var actual = await Target.ListAsync<TestEntity>(table, string.Empty, MinSelectColumns, takeCount);

            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        public async Task EnumeratesAllEntitiesWithPrefixMatchingEverything(int takeCount)
        {
            (var table, var expected) = await _fixture.SortAndInsertAsync(GenerateTestEntities("PK", 3, 3));

            var actual = await Target.ListAsync<TestEntity>(table, "P", MinSelectColumns, takeCount);

            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        public async Task EnumeratesAllEntitiesWithMatchingPrefix(int takeCount)
        {
            (var table, var all) = await _fixture.SortAndInsertAsync(Enumerable
                .Empty<TestEntity>()
                .Concat(GenerateTestEntities("AK", 3, 3))
                .Concat(GenerateTestEntities("PK", 3, 3))
                .Concat(GenerateTestEntities("ZK", 3, 3)));
            var expected = all.Where(x => x.PartitionKey.StartsWith("PK")).ToList();

            var actual = await Target.ListAsync<TestEntity>(table, "P", MinSelectColumns, takeCount);

            Assert.Equal(expected, actual);
        }

        [Theory]
        [MemberData(nameof(IncludesPartitionKeyMatchingPrefixTestData))]
        public async Task IncludesPartitionKeyMatchingPrefix(int prefixLength, int expectedCount, int takeCount)
        {
            (var table, var all) = await _fixture.SortAndInsertAsync(Enumerable
                .Empty<TestEntity>()
                .Concat(GenerateTestEntities("AK", 1, 1))
                .Concat(new[]
                {
                    new TestEntity("P", "R1"),
                    new TestEntity("PA", "R1"),
                    new TestEntity("PP", "R1"),
                    new TestEntity("PZ", "R1"),
                    new TestEntity("PPA", "R1"),
                    new TestEntity("PPP", "R1"),
                    new TestEntity("PPP", "R2"),
                    new TestEntity("PPP", "R3"),
                    new TestEntity("PPZ", "R1"),
                    new TestEntity("PPPA", "R1"),
                    new TestEntity("PPPP", "R1"),
                    new TestEntity("PPPZ", "R1"),
                    new TestEntity("PPPPP", "R1"),
                    new TestEntity("PPPPPP", "R1"),
                    new TestEntity("PPPPPPP", "R1"),
                })
                .Concat(GenerateTestEntities("ZK", 1, 1)));
            var prefix = new string('P', prefixLength);
            var expected = all.Where(x => x.PartitionKey.StartsWith(prefix)).ToList();

            var actual = await Target.ListAsync<TestEntity>(table, prefix, MinSelectColumns, takeCount);

            Assert.Equal(expected, actual);
            Assert.Equal(expectedCount, actual.Count);
        }

        public static IEnumerable<object[]> IncludesPartitionKeyMatchingPrefixTestData
        {
            get
            {
                var testData = new List<(int prefixLength, int expectedCount)>
                {
                    (0, 17),
                    (1, 15),
                    (2, 12),
                    (3, 9),
                    (4, 4),
                    (5, 3),
                    (6, 2),
                    (7, 1),
                    (8, 0),
                    (9, 0),
                };
                foreach (var testCase in testData)
                {
                    foreach (var takeCount in new[] { 1, 2, 1000 })
                    {
                        yield return new object[] { testCase.prefixLength, testCase.expectedCount, takeCount };
                    }
                }
            }
        }

        private readonly Fixture _fixture;

        public TablePrefixScannerTest(Fixture fixture, ITestOutputHelper output)
        {
            _fixture = fixture;
            MinSelectColumns = null;
            Target = new TablePrefixScanner(
                output.GetTelemetryClient(),
                output.GetLogger<TablePrefixScanner>());
        }

        private static IEnumerable<TestEntity> GenerateTestEntities(string prefix, int partitionKeyCount, int rowsPerPartitionKey)
        {
            for (var pk = 1; pk <= partitionKeyCount; pk++)
            {
                for (var rk = 1; rk <= rowsPerPartitionKey; rk++)
                {
                    yield return new TestEntity(prefix + pk, "R" + rk);
                }
            }
        }

        public IList<string> MinSelectColumns { get; set; }
        public TablePrefixScanner Target { get; }

        public class Fixture : IAsyncLifetime
        {
            private static readonly List<(IReadOnlyList<TestEntity> sortedEntities, CloudTable table)> _candidates = new List<(IReadOnlyList<TestEntity> sortedEntities, CloudTable table)>();

            public Fixture()
            {
                var connectionString = "UseDevelopmentStorage=true";
                StorageAccount = CloudStorageAccount.Parse(connectionString);
            }

            public CloudStorageAccount StorageAccount { get; }

            public async Task<(CloudTable table, IReadOnlyList<TestEntity> sortedEntities)> SortAndInsertAsync(IEnumerable<TestEntity> entities)
            {
                IReadOnlyList<TestEntity> sortedEntities = entities.OrderBy(x => x).ToList();

                CloudTable table = null;
                foreach (var candidate in _candidates)
                {
                    if (candidate.sortedEntities.SequenceEqual(sortedEntities, new PartitionKeyRowKeyComparer<TestEntity>()))
                    {
                        table = candidate.table;
                        sortedEntities = candidate.sortedEntities;
                        break;
                    }
                }

                if (table == null)
                {
                    table = StorageAccount.CreateCloudTableClient().GetTableReference("t" + Guid.NewGuid().ToByteArray().ToTrimmedBase32());
                    await table.CreateIfNotExistsAsync();
                    await Task.WhenAll(sortedEntities
                        .GroupBy(x => x.PartitionKey)
                        .Select(x => table.InsertEntitiesAsync(x.ToList()))
                        .ToList());
                    _candidates.Add((sortedEntities, table));
                }

                return (table, sortedEntities);
            }

            public Task InitializeAsync()
            {
                return Task.CompletedTask;
            }

            public Task DisposeAsync()
            {
                return Task.WhenAll(_candidates.Select(x => x.table.DeleteIfExistsAsync()));
            }
        }

        public class PartitionKeyRowKeyComparer<T> : IEqualityComparer<T> where T : ITableEntity
        {
            public bool Equals([AllowNull] T x, [AllowNull] T y)
            {
                if (ReferenceEquals(x, y))
                {
                    return true;
                }

                if (x is null || y is null)
                {
                    return false;
                }

                return x.PartitionKey == y.PartitionKey && x.RowKey == y.RowKey;
            }

            public int GetHashCode([DisallowNull] T obj)
            {
                return HashCode.Combine(obj.PartitionKey, obj.RowKey);
            }
        }

        public class TestEntity : TableEntity, IEquatable<TestEntity>, IComparable<TestEntity>
        {
            public TestEntity()
            {
            }

            public TestEntity(string partitionKey, string rowKey) : base(partitionKey, rowKey)
            {
                FieldA = partitionKey + "/" + rowKey;
                FieldB = rowKey + "/" + partitionKey;
            }

            public string FieldA { get; set; }
            public string FieldB { get; set; }

            public int CompareTo([AllowNull] TestEntity other)
            {
                if (other == null)
                {
                    return 1;
                }

                var partitionKeyCompare = PartitionKey.CompareTo(other.PartitionKey);
                if (partitionKeyCompare != 0)
                {
                    return partitionKeyCompare;
                }

                return RowKey.CompareTo(other.RowKey);
            }

            public override bool Equals(object obj)
            {
                return Equals(obj as TestEntity);
            }

            public bool Equals(TestEntity other)
            {
                return other != null &&
                       PartitionKey == other.PartitionKey &&
                       RowKey == other.RowKey &&
                       Timestamp.Equals(other.Timestamp) &&
                       ETag == other.ETag &&
                       FieldA == other.FieldA &&
                       FieldB == other.FieldB;
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(PartitionKey, RowKey, Timestamp, ETag, FieldA, FieldB);
            }
        }
    }
}
