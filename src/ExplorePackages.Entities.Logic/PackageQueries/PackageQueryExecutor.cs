﻿using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Knapcode.Delta.Common;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Knapcode.ExplorePackages.Entities
{
    /// <summary>
    /// Executes package queries on specific package identities.
    /// </summary>
    public class PackageQueryExecutor
    {
        private readonly PackageQueryProcessor _processor;
        private readonly IPackageService _packageService;
        private readonly IOptions<ExplorePackagesEntitiesSettings> _options;
        private readonly ILogger<PackageQueryExecutor> _logger;

        public PackageQueryExecutor(
            PackageQueryProcessor processor,
            IPackageService packageService,
            IOptions<ExplorePackagesEntitiesSettings> options,
            ILogger<PackageQueryExecutor> logger)
        {
            _processor = processor;
            _packageService = packageService;
            _options = options;
            _logger = logger;
        }

        public async Task ProcessPackageAsync(IReadOnlyList<IPackageQuery> queries, IReadOnlyList<PackageIdentity> identities)
        {
            var results = new ConcurrentBag<PackageQueryResult>();

            var taskQueue = new TaskQueue<PackageQueryWork>(
                workerCount: _options.Value.WorkerCount,
                produceAsync: (p, t) => ProduceAsync(p, queries, identities, t),
                consumeAsync: (w, t) => _processor.ConsumeWorkAsync(w, results),
                logger: _logger);

            await taskQueue.RunAsync();

            await _processor.PersistResults(results);
        }

        private async Task ProduceAsync(
            IProducerContext<PackageQueryWork> producer,
            IReadOnlyList<IPackageQuery> queries,
            IReadOnlyList<PackageIdentity> identities,
            CancellationToken token)
        {
            var includeNuspec = queries.Any(x => x.NeedsNuspec);
            var includeMZip = queries.Any(x => x.NeedsMZip);

            foreach (var identity in identities)
            {
                if (token.IsCancellationRequested)
                {
                    break;
                }

                var package = await _packageService.GetPackageOrNullAsync(identity.Id, identity.Version);
                if (package == null)
                {
                    _logger.LogWarning("Package {Id} {Version} does not exist.", identity.Id, identity.Version);
                    continue;
                }

                await _processor.EnqueueAsync(
                    producer,
                    queries,
                    package,
                    includeNuspec,
                    includeMZip,
                    token);
            }
        }
    }
}
