﻿// <auto-generated />

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Knapcode.ExplorePackages;

namespace Knapcode.ExplorePackages.Worker.FindPackageItem
{
    /* Kusto DDL:

    .drop table JverPackageItems ifexists;

    .create table JverPackageItems (
        ScanId: guid,
        ScanTimestamp: datetime,
        LowerId: string,
        Identity: string,
        Id: string,
        Version: string,
        CatalogCommitTimestamp: datetime,
        Created: datetime,
        ResultType: string,
        Path: string,
        FileName: string,
        FileExtension: string,
        UncompressedSize: long,
        Crc32: long
    );

    .alter-merge table JverPackageItems policy retention softdelete = 30d;

    .alter table JverPackageItems policy partitioning '{'
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

    .create table JverPackageItems ingestion csv mapping 'JverPackageItems_mapping'
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
        '{"Column":"Path","DataType":"string","Properties":{"Ordinal":9}},'
        '{"Column":"FileName","DataType":"string","Properties":{"Ordinal":10}},'
        '{"Column":"FileExtension","DataType":"string","Properties":{"Ordinal":11}},'
        '{"Column":"UncompressedSize","DataType":"long","Properties":{"Ordinal":12}},'
        '{"Column":"Crc32","DataType":"long","Properties":{"Ordinal":13}}'
    ']'

    */
    partial record PackageItem
    {
        public int FieldCount => 14;

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
            fields.Add(Path);
            fields.Add(FileName);
            fields.Add(FileExtension);
            fields.Add(UncompressedSize.ToString());
            fields.Add(Crc32.ToString());
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
            CsvUtility.WriteWithQuotes(writer, Path);
            writer.Write(',');
            CsvUtility.WriteWithQuotes(writer, FileName);
            writer.Write(',');
            CsvUtility.WriteWithQuotes(writer, FileExtension);
            writer.Write(',');
            writer.Write(UncompressedSize);
            writer.Write(',');
            writer.Write(Crc32);
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
            await CsvUtility.WriteWithQuotesAsync(writer, Path);
            await writer.WriteAsync(',');
            await CsvUtility.WriteWithQuotesAsync(writer, FileName);
            await writer.WriteAsync(',');
            await CsvUtility.WriteWithQuotesAsync(writer, FileExtension);
            await writer.WriteAsync(',');
            await writer.WriteAsync(UncompressedSize.ToString());
            await writer.WriteAsync(',');
            await writer.WriteAsync(Crc32.ToString());
            await writer.WriteLineAsync();
        }

        public PackageItem Read(Func<string> getNextField)
        {
            return new PackageItem
            {
                ScanId = CsvUtility.ParseNullable(getNextField(), Guid.Parse),
                ScanTimestamp = CsvUtility.ParseNullable(getNextField(), CsvUtility.ParseDateTimeOffset),
                LowerId = getNextField(),
                Identity = getNextField(),
                Id = getNextField(),
                Version = getNextField(),
                CatalogCommitTimestamp = CsvUtility.ParseDateTimeOffset(getNextField()),
                Created = CsvUtility.ParseNullable(getNextField(), CsvUtility.ParseDateTimeOffset),
                ResultType = Enum.Parse<PackageItemResultType>(getNextField()),
                Path = getNextField(),
                FileName = getNextField(),
                FileExtension = getNextField(),
                UncompressedSize = long.Parse(getNextField()),
                Crc32 = long.Parse(getNextField()),
            };
        }
    }
}