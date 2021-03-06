﻿// <auto-generated />

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Knapcode.ExplorePackages;

namespace Knapcode.ExplorePackages.Worker.OwnersToCsv
{
    /* Kusto DDL:

    .drop table JverPackageOwners ifexists;

    .create table JverPackageOwners (
        AsOfTimestamp: datetime,
        LowerId: string,
        Id: string,
        Owners: string
    );

    .alter-merge table JverPackageOwners policy retention softdelete = 30d;

    .alter table JverPackageOwners policy partitioning '{'
      '"PartitionKeys": ['
        '{'
          '"ColumnName": "LowerId",'
          '"Kind": "Hash",'
          '"Properties": {'
            '"Function": "XxHash64",'
            '"MaxPartitionCount": 256'
          '}'
        '}'
      ']'
    '}';

    .create table JverPackageOwners ingestion csv mapping 'JverPackageOwners_mapping'
    '['
        '{"Column":"AsOfTimestamp","DataType":"datetime","Properties":{"Ordinal":0}},'
        '{"Column":"LowerId","DataType":"string","Properties":{"Ordinal":1}},'
        '{"Column":"Id","DataType":"string","Properties":{"Ordinal":2}},'
        '{"Column":"Owners","DataType":"string","Properties":{"Ordinal":3}}'
    ']'

    */
    partial record PackageOwnerRecord
    {
        public int FieldCount => 4;

        public void WriteHeader(TextWriter writer)
        {
            writer.WriteLine("AsOfTimestamp,LowerId,Id,Owners");
        }

        public void Write(List<string> fields)
        {
            fields.Add(CsvUtility.FormatDateTimeOffset(AsOfTimestamp));
            fields.Add(LowerId);
            fields.Add(Id);
            fields.Add(Owners);
        }

        public void Write(TextWriter writer)
        {
            writer.Write(CsvUtility.FormatDateTimeOffset(AsOfTimestamp));
            writer.Write(',');
            CsvUtility.WriteWithQuotes(writer, LowerId);
            writer.Write(',');
            CsvUtility.WriteWithQuotes(writer, Id);
            writer.Write(',');
            CsvUtility.WriteWithQuotes(writer, Owners);
            writer.WriteLine();
        }

        public async Task WriteAsync(TextWriter writer)
        {
            await writer.WriteAsync(CsvUtility.FormatDateTimeOffset(AsOfTimestamp));
            await writer.WriteAsync(',');
            await CsvUtility.WriteWithQuotesAsync(writer, LowerId);
            await writer.WriteAsync(',');
            await CsvUtility.WriteWithQuotesAsync(writer, Id);
            await writer.WriteAsync(',');
            await CsvUtility.WriteWithQuotesAsync(writer, Owners);
            await writer.WriteLineAsync();
        }

        public PackageOwnerRecord Read(Func<string> getNextField)
        {
            return new PackageOwnerRecord
            {
                AsOfTimestamp = CsvUtility.ParseDateTimeOffset(getNextField()),
                LowerId = getNextField(),
                Id = getNextField(),
                Owners = getNextField(),
            };
        }
    }
}
