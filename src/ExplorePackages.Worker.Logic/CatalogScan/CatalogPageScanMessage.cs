﻿using Newtonsoft.Json;

namespace Knapcode.ExplorePackages.Worker
{
    public class CatalogPageScanMessage
    {
        [JsonProperty("s")]
        public string StorageSuffix { get; set; }

        [JsonProperty("p")]
        public string ScanId { get; set; }

        [JsonProperty("r")]
        public string PageId { get; set; }
    }
}
