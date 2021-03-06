﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Security;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Knapcode.MiniZip;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Knapcode.ExplorePackages.Worker.PackageAssemblyToCsv
{
    public class PackageAssemblyToCsvDriver : ICatalogLeafToCsvDriver<PackageAssembly>
    {
        private readonly CatalogClient _catalogClient;
        private readonly PackageFileService _packageFileService;
        private readonly FlatContainerClient _flatContainerClient;
        private readonly TempStreamService _tempStreamService;
        private readonly IOptions<ExplorePackagesWorkerSettings> _options;
        private readonly ILogger<PackageAssemblyToCsvDriver> _logger;

        private static readonly HashSet<string> FileExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".dll", ".exe" };

        public PackageAssemblyToCsvDriver(
            CatalogClient catalogClient,
            PackageFileService packageFileService,
            FlatContainerClient flatContainerClient,
            TempStreamService tempStreamService,
            IOptions<ExplorePackagesWorkerSettings> options,
            ILogger<PackageAssemblyToCsvDriver> logger)
        {
            _catalogClient = catalogClient;
            _packageFileService = packageFileService;
            _flatContainerClient = flatContainerClient;
            _tempStreamService = tempStreamService;
            _options = options;
            _logger = logger;
        }

        public string ResultsContainerName => _options.Value.PackageAssemblyContainerName;
        public List<PackageAssembly> Prune(List<PackageAssembly> records)
        {
            return PackageRecord.Prune(records);
        }

        public async Task InitializeAsync()
        {
            await _packageFileService.InitializeAsync();
        }

        public async Task<DriverResult<List<PackageAssembly>>> ProcessLeafAsync(CatalogLeafItem item)
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
                return DriverResult.Success(new List<PackageAssembly> { new PackageAssembly(scanId, scanTimestamp, leaf) });
            }
            else
            {
                var leaf = (PackageDetailsCatalogLeaf)await _catalogClient.GetCatalogLeafAsync(item.Type, item.Url);

                var zipDirectory = await _packageFileService.GetZipDirectoryAsync(item);

                if (zipDirectory == null)
                {
                    return MakeEmptyResults();
                }

                if (!zipDirectory.Entries.Any(x => FileExtensions.Contains(Path.GetExtension(x.GetName()))))
                {
                    return MakeNoAssemblies(scanId, scanTimestamp, leaf);
                }

                using var result = await _flatContainerClient.DownloadPackageContentToFileAsync(
                    item.PackageId,
                    item.PackageVersion,
                    CancellationToken.None);

                if (result == null)
                {
                    return MakeEmptyResults();
                }

                if (result.Type == TempStreamResultType.SemaphoreNotAvailable)
                {
                    return DriverResult.TryAgainLater<List<PackageAssembly>>();
                }

                using var zipArchive = new ZipArchive(result.Stream);
                var entries = zipArchive
                    .Entries
                    .Where(x => FileExtensions.Contains(Path.GetExtension(x.FullName)))
                    .ToList();

                if (!entries.Any())
                {
                    return MakeNoAssemblies(scanId, scanTimestamp, leaf);
                }

                var assemblies = new List<PackageAssembly>();
                foreach (var entry in entries)
                {
                    var assemblyResult = await AnalyzeAsync(scanId, scanTimestamp, leaf, entry);
                    if (assemblyResult.Type == DriverResultType.TryAgainLater)
                    {
                        return DriverResult.TryAgainLater<List<PackageAssembly>>();
                    }

                    assemblies.Add(assemblyResult.Value);
                }

                return DriverResult.Success(assemblies);
            }
        }

        private static DriverResult<List<PackageAssembly>> MakeEmptyResults()
        {
            // Ignore packages where the .nupkg is missing. A subsequent scan will produce a deleted asset record.
            return DriverResult.Success(new List<PackageAssembly>());
        }

        private static DriverResult<List<PackageAssembly>> MakeNoAssemblies(Guid? scanId, DateTimeOffset? scanTimestamp, PackageDetailsCatalogLeaf leaf)
        {
            return DriverResult.Success(new List<PackageAssembly> { new PackageAssembly(scanId, scanTimestamp, leaf, PackageAssemblyResultType.NoAssemblies) });
        }

        private async Task<DriverResult<PackageAssembly>> AnalyzeAsync(Guid? scanId, DateTimeOffset? scanTimestamp, PackageDetailsCatalogLeaf leaf, ZipArchiveEntry entry)
        {
            var assembly = new PackageAssembly(scanId, scanTimestamp, leaf, PackageAssemblyResultType.ValidAssembly)
            {
                Path = entry.FullName,
                FileName = Path.GetFileName(entry.FullName),
                FileExtension = Path.GetExtension(entry.FullName),
                TopLevelFolder = PathUtility.GetTopLevelFolder(entry.FullName),

                CompressedLength = entry.CompressedLength,
                EntryUncompressedLength = entry.Length,
            };

            var result = await AnalyzeAsync(assembly, entry);
            if (result.Type == DriverResultType.TryAgainLater)
            {
                return DriverResult.TryAgainLater<PackageAssembly>();
            }

            assembly.HasException = assembly.AssemblyNameHasCultureNotFoundException.GetValueOrDefault(false)
                || assembly.AssemblyNameHasFileLoadException.GetValueOrDefault(false)
                || assembly.PublicKeyTokenHasSecurityException.GetValueOrDefault(false);

            return DriverResult.Success(assembly);
        }

        private async Task<DriverResult> AnalyzeAsync(PackageAssembly assembly, ZipArchiveEntry entry)
        {
            _logger.LogInformation("Analyzing ZIP entry {FullName} of length {Length} bytes.", entry.FullName, entry.Length);

            TempStreamResult tempStreamResult = null;
            try
            {
                try
                {
                    tempStreamResult = await _tempStreamService.CopyToTempStreamAsync(() => entry.Open(), entry.Length, SHA256.Create);
                }
                catch (InvalidDataException ex)
                {
                    assembly.ResultType = PackageAssemblyResultType.InvalidZipEntry;
                    _logger.LogWarning(ex, "Package {Id} {Version} has an invalid ZIP entry: {Path}", assembly.Id, assembly.Version, assembly.Path);
                    return DriverResult.Success();
                }

                if (tempStreamResult.Type == TempStreamResultType.SemaphoreNotAvailable)
                {
                    return DriverResult.TryAgainLater();
                }

                assembly.ActualUncompressedLength = tempStreamResult.Stream.Length;
                assembly.FileSHA256 = tempStreamResult.Hash.ToBase64();

                using var peReader = new PEReader(tempStreamResult.Stream);
                if (!peReader.HasMetadata)
                {
                    assembly.ResultType = PackageAssemblyResultType.NoManagedMetadata;
                    return DriverResult.Success();
                }

                var metadataReader = peReader.GetMetadataReader();
                if (!metadataReader.IsAssembly)
                {
                    assembly.ResultType = PackageAssemblyResultType.DoesNotContainAssembly;
                    return DriverResult.Success();
                }

                var assemblyDefinition = metadataReader.GetAssemblyDefinition();

                assembly.AssemblyName = metadataReader.GetString(assemblyDefinition.Name);
                assembly.AssemblyVersion = assemblyDefinition.Version;
                assembly.Culture = metadataReader.GetString(assemblyDefinition.Culture);
                assembly.HashAlgorithm = assemblyDefinition.HashAlgorithm;
                SetPublicKeyInfo(assembly, metadataReader, assemblyDefinition);
                var assemblyName = GetAssemblyName(assembly, assemblyDefinition);
                if (assemblyName != null)
                {
                    SetPublicKeyTokenInfo(assembly, assemblyName);
                }

                return DriverResult.Success();
            }
            catch (BadImageFormatException ex)
            {
                assembly.ResultType = PackageAssemblyResultType.NotManagedAssembly;
                _logger.LogWarning(ex, "Package {Id} {Version} has an unmanaged assembly: {Path}", assembly.Id, assembly.Version, assembly.Path);
                return DriverResult.Success();
            }
            finally
            {
                tempStreamResult?.Dispose();
            }
        }

        private AssemblyName GetAssemblyName(PackageAssembly assembly, AssemblyDefinition assemblyDefinition)
        {
            AssemblyName assemblyName = null;
            try
            {
                assemblyName = assemblyDefinition.GetAssemblyName();
                assembly.AssemblyNameHasCultureNotFoundException = false;
                assembly.AssemblyNameHasFileLoadException = false;
            }
            catch (CultureNotFoundException ex)
            {
                assembly.AssemblyNameHasCultureNotFoundException = true;
                _logger.LogWarning(ex, "Package {Id} {Version} has an invalid culture: {Path}", assembly.Id, assembly.Version, assembly.Path);
            }
            catch (FileLoadException ex)
            {
                assembly.AssemblyNameHasFileLoadException = true;
                _logger.LogWarning(ex, "Package {Id} {Version} has an AssemblyName that can't be loaded: {Path}", assembly.Id, assembly.Version, assembly.Path);
            }

            return assemblyName;
        }

        private static void SetPublicKeyInfo(PackageAssembly assembly, MetadataReader metadataReader, AssemblyDefinition assemblyDefinition)
        {
            if (assemblyDefinition.PublicKey.IsNil)
            {
                assembly.HasPublicKey = false;
                return;
            }

            assembly.HasPublicKey = true;
            var publicKey = metadataReader.GetBlobBytes(assemblyDefinition.PublicKey);
            assembly.PublicKeyLength = publicKey.Length;

            using var algorithm = SHA1.Create(); // SHA1 because that is what is used for the public key token
            assembly.PublicKeySHA1 = algorithm.ComputeHash(publicKey).ToBase64();
        }

        private void SetPublicKeyTokenInfo(PackageAssembly assembly, AssemblyName assemblyName)
        {
            byte[] publicKeyTokenBytes = null;
            try
            {
                publicKeyTokenBytes = assemblyName.GetPublicKeyToken();
                assembly.PublicKeyTokenHasSecurityException = false;
            }
            catch (SecurityException ex)
            {
                assembly.PublicKeyTokenHasSecurityException = true;
                _logger.LogWarning(ex, "Package {Id} {Version} has an invalid public key: {Path}", assembly.Id, assembly.Version, assembly.Path);
            }

            if (publicKeyTokenBytes != null)
            {
                assembly.PublicKeyToken = publicKeyTokenBytes.ToHex();
            }
        }
    }
}
