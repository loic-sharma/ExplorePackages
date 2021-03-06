﻿using System;
using Microsoft.WindowsAzure.Storage.Table;
using NuGet.Versioning;

namespace Knapcode.ExplorePackages.Worker.FindLatestPackageLeaf
{
    public class LatestPackageLeaf : TableEntity, ILatestPackageLeaf
    {
        public LatestPackageLeaf(string prefix, CatalogLeafItem item, int leafRank, int pageRank, string pageUrl)
        {
            PartitionKey = GetPartitionKey(prefix, item.PackageId);
            RowKey = GetRowKey(item.PackageVersion);
            Prefix = prefix;
            Url = item.Url;
            ParsedLeafType = item.Type;
            CommitId = item.CommitId;
            CommitTimestamp = item.CommitTimestamp;
            PackageId = item.PackageId;
            PackageVersion = item.PackageVersion;
            LeafRank = leafRank;
            PageRank = pageRank;
            PageUrl = pageUrl;
        }

        public LatestPackageLeaf()
        {
        }

        [IgnoreProperty]
        public string LowerVersion => RowKey;

        [IgnoreProperty]
        public CatalogLeafType ParsedLeafType
        {
            get => Enum.Parse<CatalogLeafType>(LeafType);
            set => LeafType = value.ToString();
        }

        public string Prefix { get; set; }
        public string Url { get; set; }
        public string LeafType { get; set; }
        public string CommitId { get; set; }
        public DateTimeOffset CommitTimestamp { get; set; }
        public string PackageId { get; set; }
        public string PackageVersion { get; set; }
        public int LeafRank { get; set; }
        public int PageRank { get; set; }
        public string PageUrl { get; set; }

        public static string GetPartitionKey(string prefix, string id)
        {
            return $"{prefix}${id.ToLowerInvariant()}";
        }

        public static string GetRowKey(string version)
        {
            return NuGetVersion.Parse(version).ToNormalizedString().ToLowerInvariant();
        }
    }
}
