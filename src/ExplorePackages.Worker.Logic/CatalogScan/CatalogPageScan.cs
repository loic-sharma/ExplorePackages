﻿using System;
using Microsoft.WindowsAzure.Storage.Table;

namespace Knapcode.ExplorePackages.Worker
{
    public class CatalogPageScan : TableEntity
    {
        public CatalogPageScan(string storageSuffix, string scanId, string pageId)
        {
            StorageSuffix = storageSuffix;
            PartitionKey = scanId;
            RowKey = pageId;
            Created = DateTimeOffset.UtcNow;
        }

        public CatalogPageScan()
        {
        }

        [IgnoreProperty]
        public string ScanId => PartitionKey;

        [IgnoreProperty]
        public string PageId => RowKey;

        [IgnoreProperty]
        public CatalogPageScanState ParsedState
        {
            get => Enum.Parse<CatalogPageScanState>(State);
            set => State = value.ToString();
        }

        [IgnoreProperty]
        public CatalogScanDriverType ParsedDriverType
        {
            get => Enum.Parse<CatalogScanDriverType>(DriverType);
            set => DriverType = value.ToString();
        }

        public string StorageSuffix { get; set; }
        public DateTimeOffset Created { get; set; }
        public string State { get; set; }
        public string DriverType { get; set; }
        public string DriverParameters { get; set; }
        public DateTimeOffset Min { get; set; }
        public DateTimeOffset Max { get; set; }
        public string Url { get; set; }
        public int Rank { get; set; }
    }
}
