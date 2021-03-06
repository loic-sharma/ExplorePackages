﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Knapcode.ExplorePackages.Worker.FindLatestPackageLeaf;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Xunit;
using Xunit.Abstractions;

namespace Knapcode.ExplorePackages.Worker.TableCopy
{
    public class TableCopyDriverIntegrationTest : BaseWorkerLogicIntegrationTest
    {
        public TableCopyDriverIntegrationTest(ITestOutputHelper output, DefaultWebApplicationFactory<StaticFilesStartup> factory)
            : base(output, factory)
        {
        }

        public class CopyAsync_Serial : TableCopyDriverIntegrationTest
        {
            public CopyAsync_Serial(ITestOutputHelper output, DefaultWebApplicationFactory<StaticFilesStartup> factory) : base(output, factory)
            {
            }

            [Fact]
            public Task ExecuteAsync()
            {
                return CopyAsync(TableScanStrategy.Serial);
            }
        }

        public class CopyAsync_PrefixScan : TableCopyDriverIntegrationTest
        {
            public CopyAsync_PrefixScan(ITestOutputHelper output, DefaultWebApplicationFactory<StaticFilesStartup> factory) : base(output, factory)
            {
            }

            [Fact]
            public Task ExecuteAsync()
            {
                return CopyAsync(TableScanStrategy.PrefixScan);
            }
        }

        private async Task CopyAsync(TableScanStrategy strategy)
        {
            // Arrange
            var min0 = DateTimeOffset.Parse("2020-11-27T20:58:24.1558179Z");
            var max1 = DateTimeOffset.Parse("2020-11-27T23:41:30.2461308Z");

            await CatalogScanService.InitializeAsync();
            await SetCursorAsync(CatalogScanDriverType.FindLatestPackageLeaf, min0);
            await UpdateAsync(CatalogScanDriverType.FindLatestPackageLeaf, onlyLatestLeaves: null, max1);

            var serviceClientFactory = Host.Services.GetRequiredService<ServiceClientFactory>();
            var destTableName = StoragePrefix + "1d1";
            var tableClient = serviceClientFactory.GetStorageAccount().CreateCloudTableClient();
            var sourceTable = tableClient.GetTableReference(Options.Value.LatestPackageLeafTableName);
            var destinationTable = tableClient.GetTableReference(destTableName);

            var tableScanService = Host.Services.GetRequiredService<TableScanService<LatestPackageLeaf>>();

            var taskStateStorageSuffix = "copy";
            await TaskStateStorageService.InitializeAsync(taskStateStorageSuffix);
            var taskState = await TaskStateStorageService.GetOrAddAsync(new TaskStateKey(taskStateStorageSuffix, "copy", "copy"));

            // Act
            await tableScanService.StartTableCopyAsync(
                taskState.Key,
                sourceTable.Name,
                destinationTable.Name,
                partitionKeyPrefix: string.Empty,
                strategy,
                takeCount: 10,
                segmentsPerFirstPrefix: 1,
                segmentsPerSubsequentPrefix: 1);
            await UpdateAsync(taskState.Key);

            // Assert
            var sourceEntities = await sourceTable.GetEntitiesAsync<LatestPackageLeaf>(TelemetryClient.StartQueryLoopMetrics());
            var destinationEntities = await destinationTable.GetEntitiesAsync<LatestPackageLeaf>(TelemetryClient.StartQueryLoopMetrics());

            Assert.All(sourceEntities.Zip(destinationEntities), pair =>
            {
                pair.First.Timestamp = DateTimeOffset.MinValue;
                pair.First.ETag = string.Empty;
                pair.Second.Timestamp = DateTimeOffset.MinValue;
                pair.Second.ETag = string.Empty;
                Assert.Equal(JsonConvert.SerializeObject(pair.First), JsonConvert.SerializeObject(pair.Second));
            });

            var countLowerBound = await TaskStateStorageService.GetCountLowerBoundAsync(
                taskState.Key.StorageSuffix,
                taskState.Key.PartitionKey);
            Assert.Equal(0, countLowerBound);

            AssertOnlyInfoLogsOrLess();
        }
    }
}
