﻿namespace Knapcode.ExplorePackages.Entities
{
    public static class CursorNames
    {
        public static class NuGetOrg
        {
            private const string NuGetOrgPrefix = "NuGet.org, ";
            public const string FlatContainer = NuGetOrgPrefix + "Flat Container";
            public const string Registration = NuGetOrgPrefix + "Registration";
            public const string Search = NuGetOrgPrefix + "Search";
        }

        public const string CatalogToDatabase = "CatalogToDatabase";
        public const string Nuspecs = "CatalogToNuspecs";

        public const string V2ToDatabaseCreated = "V2ToDatabase";
        public const string V2ToDatabaseLastEdited = "V2ToDatabaseLastEdited";
        public const string MZips = "MZip";
        public const string MZipToDatabase = "MZipToDatabase";
        public const string DependenciesToDatabase = "DependenciesToDatabase";
        public const string DependencyPackagesToDatabase = "DependencyPackagesToDatabase";

        public const string ReprocessPackageQueries = "ReprocessPackageQueries";

        public const string FindMissingDependencyIdsNuspecQuery = "FindMissingDependencyIdsNuspecQuery";
        public const string FindRepositoriesNuspecQuery = "FindRepositoriesNuspecQuery";
        public const string FindPackageTypesNuspecQuery = "FindPackageTypesNuspecQuery";
        public const string FindInvalidDependencyVersionsNuspecQuery = "FindInvalidDependencyVersionsNuspecQuery";
        public const string FindMissingDependencyVersionsNuspecQuery = "FindMissingDependencyVersionsNuspecQuery";
        public const string FindEmptyDependencyVersionsNuspecQuery = "FindEmptyDependencyVersionsNuspecQuery";
        public const string FindIdsEndingInDotNumberNuspecQuery = "FindIdsEndingInDotNumberNuspecQuery";
        public const string FindFloatingDependencyVersionsNuspecQuery = "FindFloatingDependencyVersionsNuspecQuery";
        public const string FindSemVer2PackageVersionsNuspecQuery = "FindSemVer2PackageVersionsNuspecQuery";
        public const string FindSemVer2DependencyVersionsNuspecQuery = "FindSemVer2DependencyVersionsNuspecQuery";
        public const string FindNonAsciiIdsNuspecQuery = "FindNonAsciiIdsNuspecQuery";
        public const string FindInvalidPackageIdsNuspecQuery = "FindInvalidPackageIdsNuspecQuery";
        public const string FindInvalidPackageVersionsNuspecQuery = "FindInvalidPackageVersionsNuspecQuery";
        public const string FindNonNormalizedPackageVersionsNuspecQuery = "FindNonNormalizedPackageVersionsNuspecQuery";
        public const string FindPackageVersionsContainingWhitespaceNuspecQuery = "FindPackageVersionsContainingWhitespaceNuspecQuery";
        public const string FindDuplicateDependenciesNuspecQuery = "FindDuplicateDependenciesNuspecQuery";
        public const string FindMixedDependencyGroupStylesNuspecQuery = "FindMixedDependencyGroupStylesNuspecQuery";
        public const string FindWhitespaceDependencyTargetFrameworkNuspecQuery = "FindWhitespaceDependencyTargetFrameworkNuspecQuery";
        public const string FindUnsupportedDependencyTargetFrameworkNuspecQuery = "FindUnsupportedDependencyTargetFrameworkNuspecQuery";
        public const string FindInvalidDependencyTargetFrameworkNuspecQuery = "FindInvalidDependencyTargetFrameworkNuspecQuery";
        public const string FindInvalidDependencyIdNuspecQuery = "FindInvalidDependencyIdNuspecQuery";
        public const string FindDuplicateDependencyTargetFrameworksNuspecQuery = "FindDuplicateDependencyTargetFrameworksNuspecQuery";
        public const string FindDuplicateNormalizedDependencyTargetFrameworksNuspecQuery = "FindDuplicateNormalizedDependencyTargetFrameworksNuspecQuery";
        public const string FindWhitespaceDependencyIdsNuspecQuery = "FindWhitespaceDependencyIdsNuspecQuery";
        public const string FindEmptyDependencyIdsNuspecQuery = "FindEmptyDependencyIdsNuspecQuery";
        public const string FindWhitespaceDependencyVersionsNuspecQuery = "FindWhitespaceDependencyVersionsNuspecQuery";
        public const string FindCaseSensitiveDuplicateMetadataElementsNuspecQuery = "FindCaseSensitiveDuplicateMetadataElementsNuspecQuery";
        public const string FindCaseInsensitiveDuplicateMetadataElementsNuspecQuery = "FindCaseInsensitiveDuplicateMetadataElementsNuspecQuery";
        public const string FindNonAlphabetMetadataElementsNuspecQuery = "FindNonAlphabetMetadataElementsNuspecQuery";
        public const string FindCollidingMetadataElementsNuspecQuery = "FindCollidingMetadataElementsNuspecQuery";
        public const string FindUnexpectedValuesForBooleanMetadataNuspecQuery = "FindUnexpectedValuesForBooleanMetadataNuspecQuery";
        public const string FindCaseInsensitiveDuplicateTextMetadataElementsNuspecQuery = "FindCaseInsensitiveDuplicateTextMetadataElementsNuspecQuery";
        public const string FindCaseSensitiveDuplicateTextMetadataElementsNuspecQuery = "FindCaseSensitiveDuplicateTextMetadataElementsNuspecQuery";

        public const string HasRegistrationDiscrepancyInOriginalHivePackageQuery = "HasRegistrationDiscrepancyInOriginalHivePackageQuery";
        public const string HasRegistrationDiscrepancyInGzippedHivePackageQuery = "HasRegistrationDiscrepancyInGzippedHivePackageQuery";
        public const string HasRegistrationDiscrepancyInSemVer2HivePackageQuery = "HasRegistrationDiscrepancyInSemVer2HivePackageQuery";
        public const string HasPackagesContainerDiscrepancyPackageQuery = "HasPackagesContainerDiscrepancyPackageQuery";
        public const string HasFlatContainerDiscrepancyPackageQuery = "HasFlatContainerDiscrepancyPackageQuery";
        public const string HasV2DiscrepancyPackageQuery = "HasV2DiscrepancyPackageQuery";
        public const string HasSearchDiscrepancyPackageQuery = "HasSearchDiscrepancyPackageQuery";
        public const string HasCrossCheckDiscrepancyPackageQuery = "HasCrossCheckDiscrepancyPackageQuery";

        public const string HasMissingNuspecPackageQuery = "FindMissingNuspecPackageQuery";
        public const string HasMissingMZipPackageQuery = "HasMissingMZipPackageQuery";
        public const string HasInconsistentListedStateQuery = "HasInconsistentListedStateQuery";
        public const string IsMissingFromCatalogQuery = "IsMissingFromCatalogQuery";
    }
}
