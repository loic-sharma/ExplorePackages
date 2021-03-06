﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Knapcode.ExplorePackages.Entities
{
    public interface IPackageService
    {
        Task<IReadOnlyDictionary<string, PackageRegistrationEntity>> AddPackageRegistrationsAsync(IEnumerable<string> ids, bool includeCatalogPackageRegistrations, bool includePackages);
        Task<IReadOnlyDictionary<string, long>> AddOrUpdatePackagesAsync(
            IEnumerable<CatalogLeafItem> latestEntries,
            IReadOnlyDictionary<CatalogLeafItem, DateTimeOffset> latestEntryToFirstCommitTimestamp,
            IReadOnlyDictionary<CatalogLeafItem, PackageVisibilityState> latestEntryToVisibilityState);
        Task AddOrUpdatePackagesAsync(IEnumerable<PackageArchiveMetadata> metadataSequence);
        Task AddOrUpdatePackagesAsync(IEnumerable<PackageDownloads> packageDownloads);
        Task AddOrUpdatePackagesAsync(IEnumerable<V2Package> v2Packages);
        Task<IReadOnlyList<PackageEntity>> GetBatchAsync(IReadOnlyList<PackageIdentity> identities);
        Task<PackageEntity> GetPackageOrNullAsync(string id, string version);
        Task<IReadOnlyList<PackageEntity>> GetPackagesWithDependenciesAsync(IReadOnlyList<PackageIdentity> identities);
        Task SetDeletedPackagesAsUnlistedInV2Async(IEnumerable<CatalogLeafItem> entries);
    }
}