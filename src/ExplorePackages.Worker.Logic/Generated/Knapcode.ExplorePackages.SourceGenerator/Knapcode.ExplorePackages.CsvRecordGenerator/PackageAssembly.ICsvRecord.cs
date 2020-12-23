﻿// <auto-generated />
using System;
using System.IO;
using Knapcode.ExplorePackages;

namespace Knapcode.ExplorePackages.Worker.FindPackageAssemblies
{
    partial class PackageAssembly
    {
        public void Write(TextWriter writer)
        {
            writer.Write(ScanId);
            writer.Write(',');
            writer.Write(CsvUtility.FormatDateTimeOffset(ScanTimestamp));
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
            CsvUtility.WriteWithQuotes(writer, Name);
            writer.Write(',');
            writer.Write(AssemblyVersion);
            writer.Write(',');
            CsvUtility.WriteWithQuotes(writer, Culture);
            writer.Write(',');
            CsvUtility.WriteWithQuotes(writer, PublicKeyToken);
            writer.Write(',');
            writer.Write(HashAlgorithm);
            writer.WriteLine();
        }

        public void Read(Func<string> getNextField)
        {
            ScanId = CsvUtility.ParseNullable(getNextField(), Guid.Parse);
            ScanTimestamp = CsvUtility.ParseNullable(getNextField(), CsvUtility.ParseDateTimeOffset);
            Id = getNextField();
            Version = getNextField();
            CatalogCommitTimestamp = CsvUtility.ParseDateTimeOffset(getNextField());
            Created = CsvUtility.ParseNullable(getNextField(), CsvUtility.ParseDateTimeOffset);
            ResultType = Enum.Parse<PackageAssemblyResultType>(getNextField());
            Path = getNextField();
            Name = getNextField();
            AssemblyVersion = System.Version.Parse(getNextField());
            Culture = getNextField();
            PublicKeyToken = getNextField();
            HashAlgorithm = Enum.Parse<System.Reflection.AssemblyHashAlgorithm>(getNextField());
        }
    }
}
