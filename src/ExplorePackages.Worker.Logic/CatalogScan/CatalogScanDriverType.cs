﻿namespace Knapcode.ExplorePackages.Worker
{
    public enum CatalogScanDriverType
    {
        /// <summary>
        /// Implemented by <see cref="FindLatestCatalogLeafScan.LatestCatalogLeafScanStorage"/> and <see cref="FindLatestLeafDriver{T}"/>.
        /// This is a helper catalog scan used by <see cref="CatalogIndexScanMessageProcessor"/> when the driver returns
        /// <see cref="CatalogIndexScanResult.ExpandLatestLeaves"/>. This allows another driver to only process the latest
        /// catalog leaf per version instead of duplicating effort per version which is inevitable in the NuGet.org catalog.
        /// </summary>
        Internal_FindLatestCatalogLeafScan,

        /// <summary>
        /// Implemented by <see cref="LoadPackageArchive.LoadPackageArchiveDriver"/>. Downloads interesting parts of the .nupkg
        /// file stored in the V3 flat container and stores the data in Azure Table Storage for other drivers to use.
        /// </summary>
        LoadPackageArchive,

        /// <summary>
        /// Implemented by <see cref="LoadPackageManifest.LoadPackageManifestDriver"/>. Downloads the .nuspec from the
        /// V3 flat container and stores it in Azure Table Storage for other drivers to use.
        /// </summary>
        LoadPackageManifest,

        /// <summary>
        /// Implemented by <see cref="PackageArchiveEntryToCsv.PackageArchiveEntryToCsvDriver"/>.
        /// Extracts metadata about each entry in a package's ZIP archive.
        /// </summary>
        PackageArchiveEntryToCsv,

        /// <summary>
        /// Implemented by <see cref="PackageAssemblyToCsv.PackageAssemblyToCsvDriver"/>. For packages that contain
        /// assemblies, downloads the entire .nupkg and extract metadata about each assembly.
        /// </summary>
        PackageAssemblyToCsv,

        /// <summary>
        /// Implemented by <see cref="PackageAssetToCsv.PackageAssetToCsvDriver"/>. Runs NuGet restore pattern sets against
        /// all files in each packages. This essentially determines all package assets the NuGet PackageReference restore
        /// recognizes.
        /// </summary>
        PackageAssetToCsv,

        /// <summary>
        /// Implemented by <see cref="PackageSignatureToCsv.PackageSignatureToCsvDriver"/>. Extracts information from each
        /// NuGet package signature, determining things like certificate issuers and whether the package is author signed.
        /// </summary>
        PackageSignatureToCsv,

        /// <summary>
        /// Implemented by <see cref="PackageManifestToCsv.PackageManifestToCsvDriver"/>. This driver reads all package
        /// manifests (a.k.a. .nuspec files) and dumps the metadata to CSV. This implementation uses NuGet's
        /// <see cref="NuGet.Packaging.NuspecReader"/> to interpret the data as the NuGet client would.
        /// </summary>
        PackageManifestToCsv,

        /// <summary>
        /// Implemented by <see cref="CatalogLeafItemToCsv.CatalogLeafItemToCsvDriver"/>. Reads all catalog leaf items
        /// and their associated page metadata. The catalog leaf item is described here:
        /// https://docs.microsoft.com/en-us/nuget/api/catalog-resource#catalog-item-object-in-a-page
        /// </summary>
        CatalogLeafItemToCsv,

        /// <summary>
        /// Implemented by <see cref="FindLatestPackageLeaf.LatestPackageLeafStorage"/> and <see cref="FindLatestLeafDriver{T}"/>.
        /// This driver records the latest catalog leaf URL for each package version to Azure Table Storage.
        /// </summary>
        FindLatestPackageLeaf,
    }
}
