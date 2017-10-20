﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Knapcode.ExplorePackages.Entities;
using Microsoft.EntityFrameworkCore;
using NuGet.CatalogReader;
using NuGet.Common;
using NuGet.Packaging.Core;
using NuGet.Versioning;

namespace Knapcode.ExplorePackages.Logic
{
    public class PackageService
    {
        private readonly EntityContext _entityContext;
        private readonly ILogger _log;

        public PackageService(EntityContext entityContext, ILogger log)
        {
            _entityContext = entityContext;
            _log = log;
        }

        /// <summary>
        /// Adds the provided catalog entries to the database. Catalog entries are processed in the order provided.
        /// </summary>
        public async Task AddOrUpdateBatchAsync(IEnumerable<CatalogEntry> entries)
        {
            // Create a mapping from package ID + "/" + package version to package item.
            var identityToLatest = new Dictionary<string, Package>(StringComparer.OrdinalIgnoreCase);
            foreach (var entry in entries)
            {
                var latestPackage = new Package
                {
                    Id = entry.Id,
                    Version = entry.Version.ToNormalizedString(),
                    Deleted = entry.IsDelete,
                };

                latestPackage.Identity = $"{latestPackage.Id}/{latestPackage.Version}";

                identityToLatest[latestPackage.Identity] = latestPackage;
            }
            
            var getExistingStopwatch = Stopwatch.StartNew();
            var identities = identityToLatest.Keys.ToList();
            var existingPackages = await _entityContext
                .Packages
                .Where(p => identities.Contains(p.Identity))
                .ToListAsync();

            _log.LogInformation($"Got {existingPackages.Count} existing. {getExistingStopwatch.ElapsedMilliseconds}ms");

            // Update existing records.
            foreach (var existingPackage in existingPackages)
            {
                var latestPackage = identityToLatest[existingPackage.Identity];
                identityToLatest.Remove(existingPackage.Identity);
                existingPackage.Deleted = latestPackage.Deleted;
            }
            
            // Add new records.
            foreach (var pair in identityToLatest)
            {
                _entityContext.Packages.Add(pair.Value);
            }

            var commitStopwatch = Stopwatch.StartNew();
            var changes = await _entityContext.SaveChangesAsync();
            _log.LogInformation($"Committed {changes} changes. {commitStopwatch.ElapsedMilliseconds}ms");
        }
    }
}
