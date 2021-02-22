﻿using System;

namespace Knapcode.ExplorePackages.Worker.PackageManifestToCsv
{
    public partial record PackageManifestRecord : PackageRecord, ICsvRecord<PackageManifestRecord>
    {
        public PackageManifestRecord()
        {
        }

        public PackageManifestRecord(Guid? scanId, DateTimeOffset? scanTimestamp, PackageDeleteCatalogLeaf leaf)
            : base(scanId, scanTimestamp, leaf)
        {
            ResultType = PackageManifestRecordResultType.Deleted;
        }

        public PackageManifestRecord(Guid? scanId, DateTimeOffset? scanTimestamp, PackageDetailsCatalogLeaf leaf)
            : base(scanId, scanTimestamp, leaf)
        {
            ResultType = PackageManifestRecordResultType.Available;
        }

        public PackageManifestRecordResultType ResultType { get; set; }

        public string OriginalId { get; set; }
        public string OriginalVersion { get; set; }

        public string MinClientVersion { get; set; }
        public bool DevelopmentDependency { get; set; }
        public bool IsServiceable { get; set; }

        public string Authors { get; set; }
        public string Copyright { get; set; }
        public string Description { get; set; }
        public string Icon { get; set; }
        public string IconUrl { get; set; }
        public string Language { get; set; }
        public string LicenseUrl { get; set; }
        public string Owners { get; set; }
        public string ProjectUrl { get; set; }
        public string ReleaseNotes { get; set; }
        public bool RequireLicenseAcceptance { get; set; }
        public string Summary { get; set; }
        public string Tags { get; set; }
        public string Title { get; set; }

        public string PackageTypes { get; set; }
        public string LicenseMetadata { get; set; }
        public string RepositoryMetadata { get; set; }

        public string ReferenceGroups { get; set; }
        public string ContentFiles { get; set; }
        public string DependencyGroups { get; set; }
        public string FrameworkAssemblyGroups { get; set; }
        public string FrameworkRefGroups { get; set; }

        public bool ContentFilesHasFormatException { get; set; }
        public bool DependencyGroupsHasMissingId { get; set; }
    }
}
