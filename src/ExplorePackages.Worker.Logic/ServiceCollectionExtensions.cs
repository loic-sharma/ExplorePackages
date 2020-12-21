﻿using Knapcode.ExplorePackages.Worker.FindPackageAssets;
using Knapcode.ExplorePackages.Worker.RunRealRestore;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;

namespace Knapcode.ExplorePackages.Worker
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddExplorePackagesWorker(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddTransient<IRawMessageEnqueuer, QueueStorageEnqueuer>();
            serviceCollection.AddTransient<IWorkerQueueFactory, UnencodedWorkerQueueFactory>();

            serviceCollection.AddTransient<GenericMessageProcessor>();
            serviceCollection.AddTransient<SchemaSerializer>();
            serviceCollection.AddTransient<MessageBatcher>();
            serviceCollection.AddTransient<MessageEnqueuer>();

            serviceCollection.AddTransient<CatalogScanStorageService>();
            serviceCollection.AddTransient<LatestPackageLeafStorageService>();
            serviceCollection.AddTransient<CursorStorageService>();

            serviceCollection.AddTransient<AppendResultStorageService>();
            serviceCollection.AddTransient<TaskStateStorageService>();

            serviceCollection.AddTransient<CatalogScanDriverFactory>();
            serviceCollection.AddTransient<FindLatestLeavesCatalogScanDriver>();

            serviceCollection.AddTransient<CatalogScanService>();

            serviceCollection.AddTransient<ICsvReader, NRecoCsvReader>();

            serviceCollection.AddRunRealRestore();

            foreach (var (serviceType, implementationType) in typeof(ServiceCollectionExtensions).Assembly.GetClassesImplementingGeneric(typeof(IMessageProcessor<>)))
            {
                serviceCollection.AddTransient(serviceType, implementationType);
            }

            foreach (var (serviceType, implementationType) in typeof(ServiceCollectionExtensions).Assembly.GetClassesImplementingGeneric(typeof(ICatalogLeafToCsvDriver<>)))
            {
                // Add the driver
                serviceCollection.AddTransient(serviceType, implementationType);

                // Add the catalog scan adapter
                var recordType = serviceType.GenericTypeArguments.Single();
                serviceCollection.AddTransient(
                    typeof(CatalogLeafToCsvAdapter<>).MakeGenericType(recordType));

                // Add the compact processor
                serviceCollection.AddTransient(
                    typeof(IMessageProcessor<>).MakeGenericType(typeof(CatalogLeafToCsvCompactMessage<>).MakeGenericType(recordType)),
                    typeof(CatalogLeafToCsvCompactProcessor<>).MakeGenericType(recordType));
            } 

            return serviceCollection;
        }

        private static void AddRunRealRestore(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddTransient<ProjectHelper>();
        }
    }
}
