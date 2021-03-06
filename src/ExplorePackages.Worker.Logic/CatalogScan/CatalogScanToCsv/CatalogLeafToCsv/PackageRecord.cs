﻿using System;
using System.Collections.Generic;
using System.Linq;
using NuGet.Versioning;

namespace Knapcode.ExplorePackages.Worker
{
    public record PackageRecord
    {
        public PackageRecord()
        {
        }

        public PackageRecord(Guid? scanId, DateTimeOffset? scanTimestamp, PackageDeleteCatalogLeaf leaf)
            : this(scanId, scanTimestamp, leaf.PackageId, leaf.PackageVersion, leaf.CommitTimestamp, created: null)
        {
        }

        public PackageRecord(Guid? scanId, DateTimeOffset? scanTimestamp, PackageDetailsCatalogLeaf leaf)
            : this(scanId, scanTimestamp, leaf.PackageId, leaf.PackageVersion, leaf.CommitTimestamp, leaf.Created)
        {
        }

        public PackageRecord(Guid? scanId, DateTimeOffset? scanTimestamp, string id, string version, DateTimeOffset catalogCommitTimestamp, DateTimeOffset? created)
        {
            ScanId = scanId;
            ScanTimestamp = scanTimestamp;
            Id = id;
            Version = NuGetVersion.Parse(version).ToNormalizedString();
            LowerId = id.ToLowerInvariant();
            Identity = $"{LowerId}/{Version.ToLowerInvariant()}";
            CatalogCommitTimestamp = catalogCommitTimestamp;
            Created = created;
        }

        public Guid? ScanId { get; set; }
        public DateTimeOffset? ScanTimestamp { get; set; }
        public string LowerId { get; set; }

        [KustoPartitionKey]
        public string Identity { get; set; }

        public string Id { get; set; }
        public string Version { get; set; }
        public DateTimeOffset CatalogCommitTimestamp { get; set; }
        public DateTimeOffset? Created { get; set; }

        public static List<T> Prune<T>(List<T> records) where T : PackageRecord, IEquatable<T>
        {
            return records
                .GroupBy(x => x, PackageRecordIdVersionComparer.Instance) // Group by unique package version
                .Select(g => g
                    .GroupBy(x => new { x.ScanId, x.CatalogCommitTimestamp }) // Group package version records by scan and catalog commit timestamp
                    .OrderByDescending(x => x.Key.CatalogCommitTimestamp)
                    .ThenByDescending(x => x.First().ScanTimestamp)
                    .First())
                .SelectMany(g => g)
                .OrderBy(x => x.Id, StringComparer.OrdinalIgnoreCase)
                .ThenBy(x => x.Version, StringComparer.OrdinalIgnoreCase)
                .Distinct()
                .ToList();
        }
    }
}
