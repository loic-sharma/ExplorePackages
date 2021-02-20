using System;

namespace Knapcode.ExplorePackages.Worker.FindPackageItem
{
    public partial record PackageItem : PackageRecord, ICsvRecord<PackageItem>
    {
        public PackageItem()
        {
        }

        public PackageItem(Guid? scanId, DateTimeOffset? scanTimestamp, PackageDeleteCatalogLeaf leaf)
            : base(scanId, scanTimestamp, leaf)
        {
            ResultType = PackageItemResultType.Deleted;
        }

        public PackageItem(Guid? scanId, DateTimeOffset? scanTimestamp, PackageDetailsCatalogLeaf leaf, PackageItemResultType resultType)
            : base(scanId, scanTimestamp, leaf)
        {
            ResultType = resultType;
        }

        public PackageItemResultType ResultType { get; set; }

        public string Path { get; set; }
        public string FileName { get; set; }
        public string FileExtension { get; set; }

        public long UncompressedSize { get; set; }
        public long Crc32 { get; set; }
    }
}
