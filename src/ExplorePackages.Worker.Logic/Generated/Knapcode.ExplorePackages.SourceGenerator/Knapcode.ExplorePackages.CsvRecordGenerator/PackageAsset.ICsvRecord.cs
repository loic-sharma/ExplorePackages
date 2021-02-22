﻿// <auto-generated />

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Knapcode.ExplorePackages;

namespace Knapcode.ExplorePackages.Worker.PackageAssetToCsv
{
    /* Kusto DDL:

    .drop table JverPackageAssets ifexists;

    .create table JverPackageAssets (
        ScanId: guid,
        ScanTimestamp: datetime,
        LowerId: string,
        Identity: string,
        Id: string,
        Version: string,
        CatalogCommitTimestamp: datetime,
        Created: datetime,
        ResultType: string,
        PatternSet: string,
        PropertyAnyValue: string,
        PropertyCodeLanguage: string,
        PropertyTargetFrameworkMoniker: string,
        PropertyLocale: string,
        PropertyManagedAssembly: string,
        PropertyMSBuild: string,
        PropertyRuntimeIdentifier: string,
        PropertySatelliteAssembly: string,
        Path: string,
        FileName: string,
        FileExtension: string,
        TopLevelFolder: string,
        RoundTripTargetFrameworkMoniker: string,
        FrameworkName: string,
        FrameworkVersion: string,
        FrameworkProfile: string,
        PlatformName: string,
        PlatformVersion: string
    );

    .alter-merge table JverPackageAssets policy retention softdelete = 30d;

    .alter table JverPackageAssets policy partitioning '{'
      '"PartitionKeys": ['
        '{'
          '"ColumnName": "Identity",'
          '"Kind": "Hash",'
          '"Properties": {'
            '"Function": "XxHash64",'
            '"MaxPartitionCount": 256'
          '}'
        '}'
      ']'
    '}';

    .create table JverPackageAssets ingestion csv mapping 'JverPackageAssets_mapping'
    '['
        '{"Column":"ScanId","DataType":"guid","Properties":{"Ordinal":0}},'
        '{"Column":"ScanTimestamp","DataType":"datetime","Properties":{"Ordinal":1}},'
        '{"Column":"LowerId","DataType":"string","Properties":{"Ordinal":2}},'
        '{"Column":"Identity","DataType":"string","Properties":{"Ordinal":3}},'
        '{"Column":"Id","DataType":"string","Properties":{"Ordinal":4}},'
        '{"Column":"Version","DataType":"string","Properties":{"Ordinal":5}},'
        '{"Column":"CatalogCommitTimestamp","DataType":"datetime","Properties":{"Ordinal":6}},'
        '{"Column":"Created","DataType":"datetime","Properties":{"Ordinal":7}},'
        '{"Column":"ResultType","DataType":"string","Properties":{"Ordinal":8}},'
        '{"Column":"PatternSet","DataType":"string","Properties":{"Ordinal":9}},'
        '{"Column":"PropertyAnyValue","DataType":"string","Properties":{"Ordinal":10}},'
        '{"Column":"PropertyCodeLanguage","DataType":"string","Properties":{"Ordinal":11}},'
        '{"Column":"PropertyTargetFrameworkMoniker","DataType":"string","Properties":{"Ordinal":12}},'
        '{"Column":"PropertyLocale","DataType":"string","Properties":{"Ordinal":13}},'
        '{"Column":"PropertyManagedAssembly","DataType":"string","Properties":{"Ordinal":14}},'
        '{"Column":"PropertyMSBuild","DataType":"string","Properties":{"Ordinal":15}},'
        '{"Column":"PropertyRuntimeIdentifier","DataType":"string","Properties":{"Ordinal":16}},'
        '{"Column":"PropertySatelliteAssembly","DataType":"string","Properties":{"Ordinal":17}},'
        '{"Column":"Path","DataType":"string","Properties":{"Ordinal":18}},'
        '{"Column":"FileName","DataType":"string","Properties":{"Ordinal":19}},'
        '{"Column":"FileExtension","DataType":"string","Properties":{"Ordinal":20}},'
        '{"Column":"TopLevelFolder","DataType":"string","Properties":{"Ordinal":21}},'
        '{"Column":"RoundTripTargetFrameworkMoniker","DataType":"string","Properties":{"Ordinal":22}},'
        '{"Column":"FrameworkName","DataType":"string","Properties":{"Ordinal":23}},'
        '{"Column":"FrameworkVersion","DataType":"string","Properties":{"Ordinal":24}},'
        '{"Column":"FrameworkProfile","DataType":"string","Properties":{"Ordinal":25}},'
        '{"Column":"PlatformName","DataType":"string","Properties":{"Ordinal":26}},'
        '{"Column":"PlatformVersion","DataType":"string","Properties":{"Ordinal":27}}'
    ']'

    */
    partial record PackageAsset
    {
        public int FieldCount => 28;

        public void Write(List<string> fields)
        {
            fields.Add(ScanId.ToString());
            fields.Add(CsvUtility.FormatDateTimeOffset(ScanTimestamp));
            fields.Add(LowerId);
            fields.Add(Identity);
            fields.Add(Id);
            fields.Add(Version);
            fields.Add(CsvUtility.FormatDateTimeOffset(CatalogCommitTimestamp));
            fields.Add(CsvUtility.FormatDateTimeOffset(Created));
            fields.Add(ResultType.ToString());
            fields.Add(PatternSet);
            fields.Add(PropertyAnyValue);
            fields.Add(PropertyCodeLanguage);
            fields.Add(PropertyTargetFrameworkMoniker);
            fields.Add(PropertyLocale);
            fields.Add(PropertyManagedAssembly);
            fields.Add(PropertyMSBuild);
            fields.Add(PropertyRuntimeIdentifier);
            fields.Add(PropertySatelliteAssembly);
            fields.Add(Path);
            fields.Add(FileName);
            fields.Add(FileExtension);
            fields.Add(TopLevelFolder);
            fields.Add(RoundTripTargetFrameworkMoniker);
            fields.Add(FrameworkName);
            fields.Add(FrameworkVersion);
            fields.Add(FrameworkProfile);
            fields.Add(PlatformName);
            fields.Add(PlatformVersion);
        }

        public void Write(TextWriter writer)
        {
            writer.Write(ScanId);
            writer.Write(',');
            writer.Write(CsvUtility.FormatDateTimeOffset(ScanTimestamp));
            writer.Write(',');
            CsvUtility.WriteWithQuotes(writer, LowerId);
            writer.Write(',');
            CsvUtility.WriteWithQuotes(writer, Identity);
            writer.Write(',');
            CsvUtility.WriteWithQuotes(writer, Id);
            writer.Write(',');
            CsvUtility.WriteWithQuotes(writer, Version);
            writer.Write(',');
            writer.Write(CsvUtility.FormatDateTimeOffset(CatalogCommitTimestamp));
            writer.Write(',');
            writer.Write(CsvUtility.FormatDateTimeOffset(Created));
            writer.Write(',');
            writer.Write(ResultType);
            writer.Write(',');
            CsvUtility.WriteWithQuotes(writer, PatternSet);
            writer.Write(',');
            CsvUtility.WriteWithQuotes(writer, PropertyAnyValue);
            writer.Write(',');
            CsvUtility.WriteWithQuotes(writer, PropertyCodeLanguage);
            writer.Write(',');
            CsvUtility.WriteWithQuotes(writer, PropertyTargetFrameworkMoniker);
            writer.Write(',');
            CsvUtility.WriteWithQuotes(writer, PropertyLocale);
            writer.Write(',');
            CsvUtility.WriteWithQuotes(writer, PropertyManagedAssembly);
            writer.Write(',');
            CsvUtility.WriteWithQuotes(writer, PropertyMSBuild);
            writer.Write(',');
            CsvUtility.WriteWithQuotes(writer, PropertyRuntimeIdentifier);
            writer.Write(',');
            CsvUtility.WriteWithQuotes(writer, PropertySatelliteAssembly);
            writer.Write(',');
            CsvUtility.WriteWithQuotes(writer, Path);
            writer.Write(',');
            CsvUtility.WriteWithQuotes(writer, FileName);
            writer.Write(',');
            CsvUtility.WriteWithQuotes(writer, FileExtension);
            writer.Write(',');
            CsvUtility.WriteWithQuotes(writer, TopLevelFolder);
            writer.Write(',');
            CsvUtility.WriteWithQuotes(writer, RoundTripTargetFrameworkMoniker);
            writer.Write(',');
            CsvUtility.WriteWithQuotes(writer, FrameworkName);
            writer.Write(',');
            CsvUtility.WriteWithQuotes(writer, FrameworkVersion);
            writer.Write(',');
            CsvUtility.WriteWithQuotes(writer, FrameworkProfile);
            writer.Write(',');
            CsvUtility.WriteWithQuotes(writer, PlatformName);
            writer.Write(',');
            CsvUtility.WriteWithQuotes(writer, PlatformVersion);
            writer.WriteLine();
        }

        public async Task WriteAsync(TextWriter writer)
        {
            await writer.WriteAsync(ScanId.ToString());
            await writer.WriteAsync(',');
            await writer.WriteAsync(CsvUtility.FormatDateTimeOffset(ScanTimestamp));
            await writer.WriteAsync(',');
            await CsvUtility.WriteWithQuotesAsync(writer, LowerId);
            await writer.WriteAsync(',');
            await CsvUtility.WriteWithQuotesAsync(writer, Identity);
            await writer.WriteAsync(',');
            await CsvUtility.WriteWithQuotesAsync(writer, Id);
            await writer.WriteAsync(',');
            await CsvUtility.WriteWithQuotesAsync(writer, Version);
            await writer.WriteAsync(',');
            await writer.WriteAsync(CsvUtility.FormatDateTimeOffset(CatalogCommitTimestamp));
            await writer.WriteAsync(',');
            await writer.WriteAsync(CsvUtility.FormatDateTimeOffset(Created));
            await writer.WriteAsync(',');
            await writer.WriteAsync(ResultType.ToString());
            await writer.WriteAsync(',');
            await CsvUtility.WriteWithQuotesAsync(writer, PatternSet);
            await writer.WriteAsync(',');
            await CsvUtility.WriteWithQuotesAsync(writer, PropertyAnyValue);
            await writer.WriteAsync(',');
            await CsvUtility.WriteWithQuotesAsync(writer, PropertyCodeLanguage);
            await writer.WriteAsync(',');
            await CsvUtility.WriteWithQuotesAsync(writer, PropertyTargetFrameworkMoniker);
            await writer.WriteAsync(',');
            await CsvUtility.WriteWithQuotesAsync(writer, PropertyLocale);
            await writer.WriteAsync(',');
            await CsvUtility.WriteWithQuotesAsync(writer, PropertyManagedAssembly);
            await writer.WriteAsync(',');
            await CsvUtility.WriteWithQuotesAsync(writer, PropertyMSBuild);
            await writer.WriteAsync(',');
            await CsvUtility.WriteWithQuotesAsync(writer, PropertyRuntimeIdentifier);
            await writer.WriteAsync(',');
            await CsvUtility.WriteWithQuotesAsync(writer, PropertySatelliteAssembly);
            await writer.WriteAsync(',');
            await CsvUtility.WriteWithQuotesAsync(writer, Path);
            await writer.WriteAsync(',');
            await CsvUtility.WriteWithQuotesAsync(writer, FileName);
            await writer.WriteAsync(',');
            await CsvUtility.WriteWithQuotesAsync(writer, FileExtension);
            await writer.WriteAsync(',');
            await CsvUtility.WriteWithQuotesAsync(writer, TopLevelFolder);
            await writer.WriteAsync(',');
            await CsvUtility.WriteWithQuotesAsync(writer, RoundTripTargetFrameworkMoniker);
            await writer.WriteAsync(',');
            await CsvUtility.WriteWithQuotesAsync(writer, FrameworkName);
            await writer.WriteAsync(',');
            await CsvUtility.WriteWithQuotesAsync(writer, FrameworkVersion);
            await writer.WriteAsync(',');
            await CsvUtility.WriteWithQuotesAsync(writer, FrameworkProfile);
            await writer.WriteAsync(',');
            await CsvUtility.WriteWithQuotesAsync(writer, PlatformName);
            await writer.WriteAsync(',');
            await CsvUtility.WriteWithQuotesAsync(writer, PlatformVersion);
            await writer.WriteLineAsync();
        }

        public PackageAsset Read(Func<string> getNextField)
        {
            return new PackageAsset
            {
                ScanId = CsvUtility.ParseNullable(getNextField(), Guid.Parse),
                ScanTimestamp = CsvUtility.ParseNullable(getNextField(), CsvUtility.ParseDateTimeOffset),
                LowerId = getNextField(),
                Identity = getNextField(),
                Id = getNextField(),
                Version = getNextField(),
                CatalogCommitTimestamp = CsvUtility.ParseDateTimeOffset(getNextField()),
                Created = CsvUtility.ParseNullable(getNextField(), CsvUtility.ParseDateTimeOffset),
                ResultType = Enum.Parse<PackageAssetResultType>(getNextField()),
                PatternSet = getNextField(),
                PropertyAnyValue = getNextField(),
                PropertyCodeLanguage = getNextField(),
                PropertyTargetFrameworkMoniker = getNextField(),
                PropertyLocale = getNextField(),
                PropertyManagedAssembly = getNextField(),
                PropertyMSBuild = getNextField(),
                PropertyRuntimeIdentifier = getNextField(),
                PropertySatelliteAssembly = getNextField(),
                Path = getNextField(),
                FileName = getNextField(),
                FileExtension = getNextField(),
                TopLevelFolder = getNextField(),
                RoundTripTargetFrameworkMoniker = getNextField(),
                FrameworkName = getNextField(),
                FrameworkVersion = getNextField(),
                FrameworkProfile = getNextField(),
                PlatformName = getNextField(),
                PlatformVersion = getNextField(),
            };
        }
    }
}
