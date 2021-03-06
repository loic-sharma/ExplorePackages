﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Xunit;
using Xunit.Abstractions;

namespace Knapcode.ExplorePackages.Worker.PackageSignatureToCsv
{
    public class PackageSignatureToCsvIntegrationTest : BaseCatalogLeafScanToCsvIntegrationTest<PackageSignature>
    {
        private const string PackageSignatureToCsvDir = nameof(PackageSignatureToCsv);
        private const string PackageSignatureToCsv_AuthorSignatureDir = nameof(PackageSignatureToCsv_AuthorSignature);
        private const string PackageSignatureToCsv_BadTimestampDir = nameof(PackageSignatureToCsv_BadTimestamp);
        private const string PackageSignatureToCsv_WithDeleteDir = nameof(PackageSignatureToCsv_WithDelete);

        public PackageSignatureToCsvIntegrationTest(ITestOutputHelper output, DefaultWebApplicationFactory<StaticFilesStartup> factory)
            : base(output, factory)
        {
        }

        protected override string DestinationContainerName => Options.Value.PackageSignatureContainerName;
        protected override CatalogScanDriverType DriverType => CatalogScanDriverType.PackageSignatureToCsv;

        public class PackageSignatureToCsv : PackageSignatureToCsvIntegrationTest
        {
            public PackageSignatureToCsv(ITestOutputHelper output, DefaultWebApplicationFactory<StaticFilesStartup> factory)
                : base(output, factory)
            {
            }

            [Fact]
            public async Task Execute()
            {
                Logger.LogInformation("Settings: " + Environment.NewLine + JsonConvert.SerializeObject(Options.Value, Formatting.Indented));

                // Arrange
                var min0 = DateTimeOffset.Parse("2020-11-27T19:34:24.4257168Z");
                var max1 = DateTimeOffset.Parse("2020-11-27T19:35:06.0046046Z");
                var max2 = DateTimeOffset.Parse("2020-11-27T19:36:50.4909042Z");

                await CatalogScanService.InitializeAsync();
                await SetCursorAsync(CatalogScanDriverType.LoadPackageArchive, max2);
                await SetCursorAsync(min0);

                // Act
                await UpdateAsync(max1);

                // Assert
                await AssertOutputAsync(PackageSignatureToCsvDir, Step1, 0);
                await AssertOutputAsync(PackageSignatureToCsvDir, Step1, 1);
                await AssertOutputAsync(PackageSignatureToCsvDir, Step1, 2);

                // Act
                await UpdateAsync(max2);

                // Assert
                await AssertOutputAsync(PackageSignatureToCsvDir, Step2, 0);
                await AssertOutputAsync(PackageSignatureToCsvDir, Step1, 1); // This file is unchanged.
                await AssertOutputAsync(PackageSignatureToCsvDir, Step2, 2);

                await AssertExpectedStorageAsync();
                AssertOnlyInfoLogsOrLess();
            }
        }

        public class PackageSignatureToCsv_AuthorSignature : PackageSignatureToCsvIntegrationTest
        {
            public PackageSignatureToCsv_AuthorSignature(ITestOutputHelper output, DefaultWebApplicationFactory<StaticFilesStartup> factory)
                : base(output, factory)
            {
            }

            [Fact]
            public async Task Execute()
            {
                ConfigureWorkerSettings = x => x.AppendResultStorageBucketCount = 1;

                Logger.LogInformation("Settings: " + Environment.NewLine + JsonConvert.SerializeObject(Options.Value, Formatting.Indented));

                // Arrange
                var min0 = DateTimeOffset.Parse("2020-03-04T22:55:23.8646211Z");
                var max1 = DateTimeOffset.Parse("2020-03-04T22:56:51.1816512Z");

                await CatalogScanService.InitializeAsync();
                await SetCursorAsync(CatalogScanDriverType.LoadPackageArchive, max1);
                await SetCursorAsync(min0);

                // Act
                await UpdateAsync(max1);

                // Assert
                await AssertOutputAsync(PackageSignatureToCsv_AuthorSignatureDir, Step1, 0);
                await AssertExpectedStorageAsync();
                AssertOnlyInfoLogsOrLess();
            }
        }

        public class PackageSignatureToCsv_BadTimestamp : PackageSignatureToCsvIntegrationTest
        {
            public PackageSignatureToCsv_BadTimestamp(ITestOutputHelper output, DefaultWebApplicationFactory<StaticFilesStartup> factory)
                : base(output, factory)
            {
            }

            [Fact]
            public async Task Execute()
            {
                ConfigureWorkerSettings = x => x.AppendResultStorageBucketCount = 1;

                Logger.LogInformation("Settings: " + Environment.NewLine + JsonConvert.SerializeObject(Options.Value, Formatting.Indented));

                // Arrange
                var min0 = DateTimeOffset.Parse("2020-11-04T15:12:14Z");
                var max1 = DateTimeOffset.Parse("2020-11-04T15:12:15.7221964Z");

                await CatalogScanService.InitializeAsync();
                await SetCursorAsync(CatalogScanDriverType.LoadPackageArchive, max1);
                await SetCursorAsync(min0);

                // Act
                await UpdateAsync(max1);

                // Assert
                await AssertOutputAsync(PackageSignatureToCsv_BadTimestampDir, Step1, 0);
                await AssertExpectedStorageAsync();
                AssertOnlyInfoLogsOrLess();
            }
        }

        public class PackageSignatureToCsv_WithDelete : PackageSignatureToCsvIntegrationTest
        {
            public PackageSignatureToCsv_WithDelete(ITestOutputHelper output, DefaultWebApplicationFactory<StaticFilesStartup> factory)
                : base(output, factory)
            {
            }

            [Fact]
            public async Task Execute()
            {
                Logger.LogInformation("Settings: " + Environment.NewLine + JsonConvert.SerializeObject(Options.Value, Formatting.Indented));

                // Arrange
                HttpMessageHandlerFactory.OnSendAsync = async req =>
                {
                    if (req.RequestUri.AbsolutePath.EndsWith("/behaviorsample.1.0.0.nupkg"))
                    {
                        var newReq = Clone(req);
                        newReq.RequestUri = new Uri($"http://localhost/{TestData}/behaviorsample.1.0.0.nupkg");
                        return await TestDataHttpClient.SendAsync(newReq);
                    }

                    return null;
                };
                var min0 = DateTimeOffset.Parse("2020-12-20T02:37:31.5269913Z");
                var max1 = DateTimeOffset.Parse("2020-12-20T03:01:57.2082154Z");
                var max2 = DateTimeOffset.Parse("2020-12-20T03:03:53.7885893Z");

                await CatalogScanService.InitializeAsync();
                await SetCursorAsync(CatalogScanDriverType.LoadPackageArchive, max2);
                await SetCursorAsync(min0);

                // Act
                await UpdateAsync(max1);

                // Assert
                await AssertOutputAsync(PackageSignatureToCsv_WithDeleteDir, Step1, 0);
                await AssertOutputAsync(PackageSignatureToCsv_WithDeleteDir, Step1, 1);
                await AssertOutputAsync(PackageSignatureToCsv_WithDeleteDir, Step1, 2);

                // Act
                await UpdateAsync(max2);

                // Assert
                await AssertOutputAsync(PackageSignatureToCsv_WithDeleteDir, Step1, 0); // This file is unchanged.
                await AssertOutputAsync(PackageSignatureToCsv_WithDeleteDir, Step1, 1); // This file is unchanged.
                await AssertOutputAsync(PackageSignatureToCsv_WithDeleteDir, Step2, 2);

                await AssertExpectedStorageAsync();
                AssertOnlyInfoLogsOrLess();
            }
        }

        protected override IEnumerable<string> GetExpectedCursorNames()
        {
            return base.GetExpectedCursorNames().Concat(new[] { "CatalogScan-" + CatalogScanDriverType.LoadPackageArchive });
        }

        protected override IEnumerable<string> GetExpectedTableNames()
        {
            return base.GetExpectedTableNames().Concat(new[] { Options.Value.PackageArchiveTableName });
        }
    }
}
