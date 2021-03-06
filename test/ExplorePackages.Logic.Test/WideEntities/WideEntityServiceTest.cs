﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Moq;
using Xunit;
using Xunit.Abstractions;

namespace Knapcode.ExplorePackages.WideEntities
{
    public class WideEntityServiceTest : IClassFixture<WideEntityServiceTest.Fixture>
    {
        public class ExecuteBatchAsync : WideEntityServiceTest
        {
            [Fact]
            public async Task SplitsBatches()
            {
                // Arrange
                var partitionKey = StorageUtility.GenerateDescendingId().ToString();
                var batch = new[]
                {
                    WideEntityOperation.Insert(partitionKey, "rk-1", Bytes.Slice(100, WideEntityService.MaxTotalDataSize)),
                    WideEntityOperation.Insert(partitionKey, "rk-2", Bytes.Slice(200, WideEntityService.MaxTotalDataSize)),
                    WideEntityOperation.Insert(partitionKey, "rk-3", Bytes.Slice(300, WideEntityService.MaxTotalDataSize)),
                    WideEntityOperation.Insert(partitionKey, "rk-4", Bytes.Slice(400, WideEntityService.MaxTotalDataSize)),
                };

                // Act
                var inserted = await Target.ExecuteBatchAsync(TableName, batch, allowBatchSplits: true);

                // Assert
                var retrieved = await Target.RetrieveAsync(TableName, partitionKey);
                Assert.Equal(retrieved.Count, inserted.Count);
                for (var i = 0; i < inserted.Count; i++)
                {
                    Assert.Equal(retrieved[i].PartitionKey, inserted[i].PartitionKey);
                    Assert.Equal(retrieved[i].RowKey, inserted[i].RowKey);
                    Assert.Equal(retrieved[i].ETag, inserted[i].ETag);
                    Assert.Equal(retrieved[i].Timestamp, inserted[i].Timestamp);
                    Assert.Equal(retrieved[i].SegmentCount, inserted[i].SegmentCount);
                    Assert.Equal(retrieved[i].ToByteArray(), inserted[i].ToByteArray());
                }
            }

            public ExecuteBatchAsync(Fixture fixture, ITestOutputHelper output) : base(fixture, output)
            {
            }
        }

        public class InsertOrReplaceAsync : WideEntityServiceTest
        {
            [Fact]
            public async Task InsertsWhenNoEntityExists()
            {
                // Arrange
                var content = Bytes.Slice(16 * 1024, 32 * 1024);
                var partitionKey = StorageUtility.GenerateDescendingId().ToString();
                var rowKey = "rk";

                // Act
                var newEntity = await Target.InsertOrReplaceAsync(TableName, partitionKey, rowKey, content);

                // Assert
                var retrieved = await Target.RetrieveAsync(TableName, partitionKey, rowKey);
                Assert.Equal(retrieved.Timestamp, newEntity.Timestamp);
                Assert.Equal(retrieved.ETag, newEntity.ETag);
                Assert.Equal(retrieved.ToByteArray(), newEntity.ToByteArray());
            }

            [Fact]
            public async Task ReplacesExistingEntity()
            {
                // Arrange
                var existingContent = Bytes.Slice(0, 1024 * 1024);
                var newContent = Bytes.Slice(16 * 1024, 32 * 1024);
                var partitionKey = StorageUtility.GenerateDescendingId().ToString();
                var rowKey = "rk";
                var existingEntity = await Target.InsertAsync(TableName, partitionKey, rowKey, existingContent);

                // Act
                var newEntity = await Target.InsertOrReplaceAsync(TableName, partitionKey, rowKey, newContent);

                // Assert
                Assert.Equal(default, newEntity.Timestamp);

                Assert.NotEqual(existingEntity.ETag, newEntity.ETag);
                Assert.NotEqual(existingEntity.ToByteArray(), newEntity.ToByteArray());

                var retrieved = await Target.RetrieveAsync(TableName, partitionKey, rowKey);
                Assert.Equal(retrieved.ETag, newEntity.ETag);
                Assert.Equal(retrieved.ToByteArray(), newEntity.ToByteArray());
            }

            public InsertOrReplaceAsync(Fixture fixture, ITestOutputHelper output) : base(fixture, output)
            {
            }
        }

        public class InsertAsync : WideEntityServiceTest
        {
            [Fact]
            public async Task FailsWhenEntityIsTooLarge()
            {
                // Arrange
                var content = Bytes.Slice(0, WideEntityService.MaxTotalDataSize + 1);
                var partitionKey = StorageUtility.GenerateDescendingId().ToString();
                var rowKey = "rk";

                // Act & Assert
                var ex = await Assert.ThrowsAsync<ArgumentException>(() => Target.InsertAsync(TableName, partitionKey, rowKey, content));
                Assert.Equal("The content is too large. (Parameter 'content')", ex.Message);
            }

            [Fact]
            public async Task FailsWhenEntityExists()
            {
                // Arrange
                var existingContent = Bytes.Slice(0, 1024 * 1024);
                var newContent = Bytes.Slice(16 * 1024, 32 * 1024);
                var partitionKey = StorageUtility.GenerateDescendingId().ToString();
                var rowKey = "rk";
                var existingEntity = await Target.InsertAsync(TableName, partitionKey, rowKey, existingContent);

                // Act & Assert
                var ex = await Assert.ThrowsAsync<StorageException>(() => Target.InsertAsync(TableName, partitionKey, rowKey, newContent));
                Assert.Equal((int)HttpStatusCode.Conflict, ex.RequestInformation.HttpStatusCode);
                var retrieved = await Target.RetrieveAsync(TableName, partitionKey, rowKey);
                Assert.Equal(existingEntity.ETag, retrieved.ETag);
                Assert.Equal(existingEntity.ToByteArray(), retrieved.ToByteArray());
            }

            public InsertAsync(Fixture fixture, ITestOutputHelper output) : base(fixture, output)
            {
            }
        }

        public class ReplaceAsync : WideEntityServiceTest
        {
            [Fact]
            public async Task ReplacesExistingEntity()
            {
                // Arrange
                var existingContent = Bytes.Slice(0, 1024 * 1024);
                var newContent = Bytes.Slice(16 * 1024, 32 * 1024);
                var partitionKey = StorageUtility.GenerateDescendingId().ToString();
                var rowKey = "rk";
                var existingEntity = await Target.InsertAsync(TableName, partitionKey, rowKey, existingContent);

                // Act
                var newEntity = await Target.ReplaceAsync(TableName, existingEntity, newContent);

                // Assert
                Assert.Equal(default, newEntity.Timestamp);

                Assert.NotEqual(existingEntity.ETag, newEntity.ETag);
                Assert.NotEqual(existingEntity.ToByteArray(), newEntity.ToByteArray());

                var retrieved = await Target.RetrieveAsync(TableName, partitionKey, rowKey);
                Assert.Equal(retrieved.ETag, newEntity.ETag);
                Assert.Equal(retrieved.ToByteArray(), newEntity.ToByteArray());
            }

            [Fact]
            public async Task FailsWhenExistingEntityHasBeenDeleted()
            {
                // Arrange
                var content = Bytes.Slice(0, 16);
                var partitionKey = StorageUtility.GenerateDescendingId().ToString();
                var rowKey = "rk";
                var existingEntity = await Target.InsertAsync(TableName, partitionKey, rowKey, content);
                await Target.DeleteAsync(TableName, existingEntity);

                // Act & Assert
                var ex = await Assert.ThrowsAsync<StorageException>(() => Target.ReplaceAsync(TableName, existingEntity, content));
                Assert.Equal((int)HttpStatusCode.NotFound, ex.RequestInformation.HttpStatusCode);
                Assert.Null(await Target.RetrieveAsync(TableName, partitionKey, rowKey));
            }

            [Fact]
            public async Task FailsWhenExistingEntityHasBeenChanged()
            {
                // Arrange
                var existingContent = Bytes.Slice(0, 1024 * 1024);
                var newContent = Bytes.Slice(16 * 1024, 32 * 1024);
                var partitionKey = StorageUtility.GenerateDescendingId().ToString();
                var rowKey = "rk";
                var existingEntity = await Target.InsertAsync(TableName, partitionKey, rowKey, existingContent);
                var changedEntity = await Target.ReplaceAsync(TableName, existingEntity, existingContent);

                // Act & Assert
                var ex = await Assert.ThrowsAsync<StorageException>(() => Target.ReplaceAsync(TableName, existingEntity, newContent));
                Assert.Equal((int)HttpStatusCode.PreconditionFailed, ex.RequestInformation.HttpStatusCode);
                var retrieved = await Target.RetrieveAsync(TableName, partitionKey, rowKey);
                Assert.Equal(changedEntity.ETag, retrieved.ETag);
                Assert.Equal(changedEntity.ToByteArray(), retrieved.ToByteArray());
            }

            public ReplaceAsync(Fixture fixture, ITestOutputHelper output) : base(fixture, output)
            {
            }
        }

        public class RetrieveRangeAsync : WideEntityServiceTest
        {
            [Theory]
            [InlineData(false)]
            [InlineData(true)]
            public async Task ReturnsEmptyForNonExistentEntity(bool includeData)
            {
                // Arrange
                var partitionKey = StorageUtility.GenerateDescendingId().ToString();
                var minRowKey = "rk-1";
                var maxRowKey = "rk-3";

                // Act
                var actual = await Target.RetrieveAsync(TableName, partitionKey, minRowKey, maxRowKey, includeData);

                // Assert
                Assert.Empty(actual);
            }

            [Theory]
            [InlineData("a", "rk-1", 0, 1)]
            [InlineData("rk-0", "rk-1", 0, 1)]
            [InlineData("rk-1", "rk-1", 0, 1)]
            [InlineData("rk-1", "rk-2", 0, 2)]
            [InlineData("rk-2", "rk-3", 1, 2)]
            [InlineData("rk-2", "rk-4", 1, 2)]
            [InlineData("rk-2", "z", 1, 3)]
            [InlineData("rk-4", "rk-5", 3, 1)]
            [InlineData("rk-1", "rk-5", 0, 4)]
            [InlineData("a", "z", 0, 4)]
            public async Task FetchesRangeOfEntities(string minRowKey, string maxRowKey, int skip, int take)
            {
                // Arrange
                var partitionKey = StorageUtility.GenerateDescendingId().ToString();
                var a = await Target.InsertAsync(TableName, partitionKey, "rk-1", Bytes.Slice(0, 1024));
                var b = await Target.InsertAsync(TableName, partitionKey, "rk-2", Bytes.Slice(1024, 1024));
                var c = await Target.InsertAsync(TableName, partitionKey, "rk-3", Bytes.Slice(2048, 1024));
                var d = await Target.InsertAsync(TableName, partitionKey, "rk-5", Bytes.Slice(3072, 1024));
                var all = new[] { a, b, c, d };

                // Act
                var entities = await Target.RetrieveAsync(TableName, partitionKey, minRowKey, maxRowKey);

                // Assert
                Assert.Equal(take, entities.Count);
                for (var i = skip; i < skip + take; i++)
                {
                    var expected = all[i];
                    var actual = entities[i - skip];
                    Assert.Equal(expected.PartitionKey, actual.PartitionKey);
                    Assert.Equal(expected.RowKey, actual.RowKey);
                    Assert.Equal(expected.ETag, actual.ETag);
                    Assert.Equal(expected.ToByteArray(), actual.ToByteArray());
                }
            }

            [Fact]
            public async Task AllowsNotFetchingData()
            {
                // Arrange
                var partitionKey = StorageUtility.GenerateDescendingId().ToString();
                var rowKeys = new[] { "rk-1", "rk-2", "rk-3" };
                var before = DateTimeOffset.UtcNow;
                await Target.InsertAsync(TableName, partitionKey, rowKeys[0], Bytes.Slice(0, 1024));
                await Target.InsertAsync(TableName, partitionKey, rowKeys[1], Bytes.Slice(1024, 1024));
                await Target.InsertAsync(TableName, partitionKey, rowKeys[2], Bytes.Slice(2048, 1024));
                var after = DateTimeOffset.UtcNow;

                // Act
                var actual = await Target.RetrieveAsync(TableName, partitionKey, rowKeys[0], rowKeys[2], includeData: false);

                // Assert
                Assert.Equal(3, actual.Count);
                Assert.All(actual, x => Assert.Equal(partitionKey, x.PartitionKey));
                Assert.Equal(rowKeys[0], actual[0].RowKey);
                Assert.Equal(rowKeys[1], actual[1].RowKey);
                Assert.Equal(rowKeys[2], actual[2].RowKey);
                var error = TimeSpan.FromMinutes(5);
                Assert.All(actual, x => Assert.InRange(x.Timestamp, before.Subtract(error), after.Add(error)));
                Assert.All(actual, x => Assert.NotEqual(default, x.Timestamp));
                Assert.All(actual, x => Assert.NotNull(x.ETag));
                Assert.All(actual, x => Assert.Equal(1, x.SegmentCount));
                Assert.All(actual, x =>
                {
                    var ex = Assert.Throws<InvalidOperationException>(() => x.GetStream());
                    Assert.Equal("The data was not included when retrieving this entity.", ex.Message);
                });
            }

            public RetrieveRangeAsync(Fixture fixture, ITestOutputHelper output) : base(fixture, output)
            {
            }
        }

        public class RetrieveAsync : WideEntityServiceTest
        {
            [Theory]
            [InlineData(false)]
            [InlineData(true)]
            public async Task ReturnsNullForNonExistentEntity(bool includeData)
            {
                // Arrange
                var partitionKey = StorageUtility.GenerateDescendingId().ToString();
                var rowKey = "rk";

                // Act
                var wideEntity = await Target.RetrieveAsync(TableName, partitionKey, rowKey, includeData);

                // Assert
                Assert.Null(wideEntity);
            }

            [Fact]
            public async Task AllowsNotFetchingData()
            {
                // Arrange
                var src = Bytes.Slice(0, 1024);
                var partitionKey = StorageUtility.GenerateDescendingId().ToString();
                var rowKey = "rk";
                var before = DateTimeOffset.UtcNow;
                await Target.InsertAsync(TableName, partitionKey, rowKey, src);
                var after = DateTimeOffset.UtcNow;

                // Act
                var wideEntity = await Target.RetrieveAsync(TableName, partitionKey, rowKey, includeData: false);

                // Assert
                Assert.Equal(partitionKey, wideEntity.PartitionKey);
                Assert.Equal(rowKey, wideEntity.RowKey);
                var error = TimeSpan.FromMinutes(5);
                Assert.InRange(wideEntity.Timestamp, before.Subtract(error), after.Add(error));
                Assert.NotEqual(default, wideEntity.Timestamp);
                Assert.NotNull(wideEntity.ETag);
                Assert.Equal(1, wideEntity.SegmentCount);
                var ex = Assert.Throws<InvalidOperationException>(() => wideEntity.GetStream());
                Assert.Equal("The data was not included when retrieving this entity.", ex.Message);
            }

            public RetrieveAsync(Fixture fixture, ITestOutputHelper output) : base(fixture, output)
            {
            }
        }

        public class IntegrationTest : WideEntityServiceTest
        {
            [Theory]
            [MemberData(nameof(RoundTripsBytesTestData))]
            public async Task RoundTripsBytes(int length)
            {
                // Arrange
                var src = Bytes.Slice(0, length);
                var partitionKey = StorageUtility.GenerateDescendingId().ToString();
                var rowKey = "rk";

                // Act
                await Target.InsertAsync(TableName, partitionKey, rowKey, src);
                var wideEntity = await Target.RetrieveAsync(TableName, partitionKey, rowKey);

                // Assert
                Assert.Equal(src.ToArray(), wideEntity.ToByteArray());
            }

            [Fact]
            public async Task PopulatesWideEntityProperties()
            {
                // Arrange
                var src = Bytes.Slice(0, WideEntityService.MaxTotalDataSize);
                var partitionKey = StorageUtility.GenerateDescendingId().ToString();
                var rowKey = "rk";

                // Act
                var before = DateTimeOffset.UtcNow;
                await Target.InsertAsync(TableName, partitionKey, rowKey, src);
                var after = DateTimeOffset.UtcNow;
                var wideEntity = await Target.RetrieveAsync(TableName, partitionKey, rowKey);

                // Assert
                Assert.Equal(partitionKey, wideEntity.PartitionKey);
                Assert.Equal(rowKey, wideEntity.RowKey);
                var error = TimeSpan.FromMinutes(5);
                Assert.InRange(wideEntity.Timestamp, before.Subtract(error), after.Add(error));
                Assert.NotEqual(default, wideEntity.Timestamp);
                Assert.NotNull(wideEntity.ETag);
                Assert.Equal(_fixture.IsLoopback ? 8 : 3, wideEntity.SegmentCount);
            }

            public static IEnumerable<object[]> RoundTripsBytesTestData => ByteArrayLengths
                .Distinct()
                .OrderBy(x => x)
                .Select(x => new object[] { x });

            private static IEnumerable<int> ByteArrayLengths
            {
                get
                {
                    yield return 0;
                    var current = 1;
                    do
                    {
                        yield return current;
                        current *= 2;
                    }
                    while (current <= WideEntityService.MaxTotalDataSize);

                    for (var i = 16; i >= 0; i--)
                    {
                        yield return WideEntityService.MaxTotalDataSize - i;
                    }

                    var random = new Random(0);
                    for (var i = 0; i < 10; i++)
                    {
                        yield return random.Next(0, WideEntityService.MaxTotalDataSize);
                    }
                }
            }

            public IntegrationTest(Fixture fixture, ITestOutputHelper output) : base(fixture, output)
            {
            }
        }

        private readonly Fixture _fixture;

        public WideEntityServiceTest(Fixture fixture, ITestOutputHelper output)
        {
            _fixture = fixture;
            Target = new WideEntityService(
                _fixture.ServiceClientFactory,
                output.GetTelemetryClient());
        }

        public string TableName => _fixture.TableName;
        public ReadOnlyMemory<byte> Bytes => _fixture.Bytes.AsMemory();
        public WideEntityService Target { get; }

        public class Fixture : IAsyncLifetime
        {
            public Fixture()
            {
                Options = new Mock<IOptions<ExplorePackagesSettings>>();
                Settings = new ExplorePackagesSettings();
                Options.Setup(x => x.Value).Returns(() => Settings);
                ServiceClientFactory = new ServiceClientFactory(Options.Object);
                TableName = "t" + StorageUtility.GenerateUniqueId().ToLowerInvariant();
                IsLoopback = GetTable().Uri.IsLoopback;

                Bytes = new byte[4 * 1024 * 1024];
                var random = new Random(0);
                random.NextBytes(Bytes);
            }

            public Mock<IOptions<ExplorePackagesSettings>> Options { get; }
            public ExplorePackagesSettings Settings { get; }
            public ServiceClientFactory ServiceClientFactory { get; }
            public string TableName { get; }
            public bool IsLoopback { get; }
            public byte[] Bytes { get; }

            public Task InitializeAsync()
            {
                return GetTable().CreateIfNotExistsAsync();
            }

            public Task DisposeAsync()
            {
                return GetTable().DeleteIfExistsAsync();
            }

            private CloudTable GetTable()
            {
                return ServiceClientFactory.GetStorageAccount().CreateCloudTableClient().GetTableReference(TableName);
            }
        }
    }
}
