using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Knapcode.MiniZip;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Knapcode.ExplorePackages.Worker.FindPackageItem
{
    public class FindPackageItemDriver : ICatalogLeafToCsvDriver<PackageItem>
    {
        private readonly CatalogClient _catalogClient;
        private readonly PackageFileService _packageFileService;
        private readonly IOptions<ExplorePackagesWorkerSettings> _options;
        private readonly ILogger<FindPackageItemDriver> _logger;

        public FindPackageItemDriver(
            CatalogClient catalogClient,
            PackageFileService packageFileService,
            IOptions<ExplorePackagesWorkerSettings> options,
            ILogger<FindPackageItemDriver> logger)
        {
            _catalogClient = catalogClient;
            _packageFileService = packageFileService;
            _options = options;
            _logger = logger;
        }

        public string ResultsContainerName => _options.Value.PackageItemContainerName;

        public List<PackageItem> Prune(List<PackageItem> records)
        {
            return PackageRecord.Prune(records);
        }

        public async Task InitializeAsync()
        {
            await _packageFileService.InitializeAsync();
        }

        public async Task<DriverResult<List<PackageItem>>> ProcessLeafAsync(CatalogLeafItem item)
        {
            Guid? scanId = null;
            DateTimeOffset? scanTimestamp = null;
            if (_options.Value.AppendResultUniqueIds)
            {
                scanId = Guid.NewGuid();
                scanTimestamp = DateTimeOffset.UtcNow;
            }

            if (item.Type == CatalogLeafType.PackageDelete)
            {
                var leaf = (PackageDeleteCatalogLeaf)await _catalogClient.GetCatalogLeafAsync(item.Type, item.Url);
                return DriverResult.Success(new List<PackageItem> { new PackageItem(scanId, scanTimestamp, leaf) });
            }
            else
            {
                var leaf = (PackageDetailsCatalogLeaf)await _catalogClient.GetCatalogLeafAsync(item.Type, item.Url);

                var zipDirectory = await _packageFileService.GetZipDirectoryAsync(item);
                if (zipDirectory == null)
                {
                    // Ignore packages where the .nupkg is missing. A subsequent scan will produce a deleted asset record.
                    return DriverResult.Success(new List<PackageItem>());
                }

                var items = new List<PackageItem>();

                foreach (var entry in zipDirectory.Entries)
                {
                    var path = entry.GetName();

                    items.Add(new PackageItem(scanId, scanTimestamp, leaf, PackageItemResultType.AvailableItems)
                    {
                        Path = path,
                        FileName = Path.GetFileName(path),
                        FileExtension = Path.GetExtension(path),

                        UncompressedSize = entry.UncompressedSize,
                        Crc32 = entry.Crc32,
                    });
                }

                if (items.Count == 0)
                {
                    items = new List<PackageItem> { new PackageItem(scanId, scanTimestamp, leaf, PackageItemResultType.NoItems) };
                }

                return DriverResult.Success(items);
            }
        }
    }
}
