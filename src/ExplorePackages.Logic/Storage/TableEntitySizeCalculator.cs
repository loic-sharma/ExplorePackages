﻿using System;
using Microsoft.WindowsAzure.Storage.Table;

namespace Knapcode.ExplorePackages
{
    /// <summary>
    /// Source: https://docs.microsoft.com/en-us/archive/blogs/avkashchauhan/how-the-size-of-an-entity-is-caclulated-in-windows-azure-table-storage
    /// </summary>
    public class TableEntitySizeCalculator
    {
        // Entity overhead of 4 plus a magic, discovered value.
        // See: https://github.com/MicrosoftDocs/azure-docs/issues/68661
        private const int InitialSize = 4 + (48 + 88);

        public int Size { get; private set; }

        public void Reset()
        {
            Size = 0;
        }

        public void AddEntityOverhead()
        {
            Size += InitialSize;
        }

        public void AddPartitionKeyRowKey(string partitionKey, string rowKey)
        {
            Size += (partitionKey.Length + rowKey.Length) * 2;
        }

        public void AddPartitionKey(string rowKey)
        {
            AddPartitionKey(rowKey.Length);
        }

        public void AddPartitionKey(int length)
        {
            Size += length * 2;
        }

        public void AddRowKey(string rowKey)
        {
            AddRowKey(rowKey.Length);
        }

        public void AddRowKey(int length)
        {
            Size += length * 2;
        }

        public void AddPropertyOverhead(int nameLength)
        {
            Size += GetPropertyOverhead(nameLength);
        }

        public void AddBinaryData(int binaryLength)
        {
            Size += GetBinaryDataSize(binaryLength);
        }

        public void AddInt32Data()
        {
            Size += GetInt32DataSize();
        }

        public void AddProperty(string name, EntityProperty value)
        {
            Size += GetEntityPropertySize(name.Length, value);
        }

        private static int GetEntityPropertySize(int nameLength, EntityProperty value)
        {
            int size;
            switch (value.PropertyType)
            {
                case EdmType.String when value.StringValue == null:
                    return 0;
                case EdmType.DateTime when !value.DateTime.HasValue:
                    return 0;
                case EdmType.Guid when !value.GuidValue.HasValue:
                    return 0;
                case EdmType.Double when !value.DoubleValue.HasValue:
                    return 0;
                case EdmType.Int32 when !value.Int32Value.HasValue:
                    return 0;
                case EdmType.Int64 when !value.Int64Value.HasValue:
                    return 0;
                case EdmType.Boolean when !value.BooleanValue.HasValue:
                    return 0;
                case EdmType.Binary when value.BinaryValue == null:
                    return 0;

                case EdmType.String:
                    size = value.StringValue.Length * 2 + 4;
                    break;
                case EdmType.DateTime:
                    size = 8;
                    break;
                case EdmType.Guid:
                    size = 16;
                    break;
                case EdmType.Double:
                    size = 8;
                    break;
                case EdmType.Int32:
                    size = GetInt32DataSize();
                    break;
                case EdmType.Int64:
                    size = 8;
                    break;
                case EdmType.Boolean:
                    size = 1;
                    break;
                case EdmType.Binary:
                    size = GetBinaryDataSize(value.BinaryValue.Length);
                    break;

                default:
                    throw new NotImplementedException();
            };

            return GetPropertyOverhead(nameLength) + size;
        }

        private static int GetPropertyOverhead(int nameLength)
        {
            return 8 + nameLength * 2;
        }

        private static int GetInt32DataSize()
        {
            return 4;
        }

        private static int GetBinaryDataSize(int binaryLength)
        {
            return binaryLength + 4;
        }
    }
}
