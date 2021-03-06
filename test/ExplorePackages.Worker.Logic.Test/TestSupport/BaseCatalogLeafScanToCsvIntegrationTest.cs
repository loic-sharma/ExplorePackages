﻿using System;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace Knapcode.ExplorePackages.Worker
{
    public abstract class BaseCatalogLeafScanToCsvIntegrationTest<T> : BaseCatalogScanToCsvIntegrationTest<T> where T : ICsvRecord<T>, new()
    {
        protected BaseCatalogLeafScanToCsvIntegrationTest(ITestOutputHelper output, DefaultWebApplicationFactory<StaticFilesStartup> factory) : base(output, factory)
        {
        }

        protected override Task<CatalogIndexScan> UpdateAsync(DateTimeOffset max)
        {
            return UpdateAsync(DriverType, OnlyLatestLeaves, max);
        }
    }
}
