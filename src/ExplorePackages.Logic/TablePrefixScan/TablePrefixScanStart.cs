﻿using System;
using System.Diagnostics;

namespace Knapcode.ExplorePackages.TablePrefixScan
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class TablePrefixScanStart : TablePrefixScanStep
    {
        public TablePrefixScanStart(TableQueryParameters parameters, string partitionKeyPrefix)
            : base(parameters, depth: 0)
        {
            PartitionKeyPrefix = partitionKeyPrefix ?? throw new ArgumentNullException(nameof(partitionKeyPrefix));
        }

        public override string DebuggerDisplay => $"start PK = '{PartitionKeyPrefix}*'";

        public string PartitionKeyPrefix { get; }
    }
}
