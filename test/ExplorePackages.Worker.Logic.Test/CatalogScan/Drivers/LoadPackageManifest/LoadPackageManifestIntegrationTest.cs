﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using MessagePack;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Xunit;
using Xunit.Abstractions;

namespace Knapcode.ExplorePackages.Worker.LoadPackageManifest
{
    public class LoadPackageManifestIntegrationTest : BaseCatalogScanIntegrationTest
    {
        public const string LoadPackageManifestDir = nameof(LoadPackageManifest);
        public const string LoadPackageManifest_WithDeleteDir = nameof(LoadPackageManifest_WithDelete);

        public class LoadPackageManifest : LoadPackageManifestIntegrationTest
        {
            public LoadPackageManifest(ITestOutputHelper output, DefaultWebApplicationFactory<StaticFilesStartup> factory)
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
                await SetCursorAsync(min0);

                // Act
                await UpdateAsync(max1);

                // Assert
                await VerifyOutputAsync(LoadPackageManifestDir, Step1);

                // Act
                await UpdateAsync(max2);

                // Assert
                await VerifyOutputAsync(LoadPackageManifestDir, Step2);
                AssertOnlyInfoLogsOrLess();
            }
        }

        public class LoadPackageManifest_WithDelete : LoadPackageManifestIntegrationTest
        {
            public LoadPackageManifest_WithDelete(ITestOutputHelper output, DefaultWebApplicationFactory<StaticFilesStartup> factory)
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
                    if (req.RequestUri.AbsolutePath.EndsWith("/behaviorsample.nuspec"))
                    {
                        var newReq = Clone(req);
                        newReq.RequestUri = new Uri($"http://localhost/{TestData}/behaviorsample.1.0.0.nuspec");
                        return await TestDataHttpClient.SendAsync(newReq);
                    }

                    return null;
                };

                // Set the Last-Modified date for the etag
                var file = new FileInfo(Path.Combine(TestData, "behaviorsample.1.0.0.nuspec"))
                {
                    LastWriteTimeUtc = DateTime.Parse("2021-01-14T18:00:00Z")
                };

                var min0 = DateTimeOffset.Parse("2020-12-20T02:37:31.5269913Z");
                var max1 = DateTimeOffset.Parse("2020-12-20T03:01:57.2082154Z");
                var max2 = DateTimeOffset.Parse("2020-12-20T03:03:53.7885893Z");

                await CatalogScanService.InitializeAsync();
                await SetCursorAsync(min0);

                // Act
                await UpdateAsync(max1);

                // Assert
                await VerifyOutputAsync(LoadPackageManifest_WithDeleteDir, Step1);

                // Act
                await UpdateAsync(max2);

                // Assert
                await VerifyOutputAsync(LoadPackageManifest_WithDeleteDir, Step2);
                AssertOnlyInfoLogsOrLess();
            }
        }

        public override bool OnlyLatestLeaves => true;

        protected override IEnumerable<string> GetExpectedTableNames()
        {
            return base.GetExpectedTableNames().Concat(new[] { Options.Value.PackageManifestTableName });
        }

        public LoadPackageManifestIntegrationTest(ITestOutputHelper output, DefaultWebApplicationFactory<StaticFilesStartup> factory) : base(output, factory)
        {
        }

        protected override CatalogScanDriverType DriverType => CatalogScanDriverType.LoadPackageManifest;

        private async Task VerifyOutputAsync(string testName, string stepName)
        {
            Assert.Empty(HttpMessageHandlerFactory.Requests.Where(x => x.RequestUri.AbsoluteUri.EndsWith(".nupkg")));
            Assert.NotEmpty(HttpMessageHandlerFactory.Requests.Where(x => x.RequestUri.AbsoluteUri.EndsWith(".nuspec")));

            await VerifyWideEntityOutputAsync(
                Options.Value.PackageManifestTableName,
                Path.Combine(testName, stepName),
                stream =>
                {
                    var entity = MessagePackSerializer.Deserialize<PackageManifestService.PackageManifestInfoVersions>(stream, ExplorePackagesMessagePack.Options);

                    string manifestHash = null;
                    SortedDictionary<string, List<string>> httpHeaders = null;

                    if (entity.V1.Available)
                    {
                        using var algorithm = SHA256.Create();
                        manifestHash = algorithm.ComputeHash(entity.V1.ManifestBytes.ToArray()).ToHex();
                        httpHeaders = NormalizeHeaders(entity.V1.HttpHeaders);
                    }

                    return new
                    {
                        entity.V1.Available,
                        entity.V1.CommitTimestamp,
                        HttpHeaders = httpHeaders,
                        ManifestHash = manifestHash,
                    };
                });
        }
    }
}
