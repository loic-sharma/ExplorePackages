﻿using System.Runtime.CompilerServices;

namespace Knapcode.ExplorePackages
{
    public static class TelemetryClientExtensions
    {
        public static QueryLoopMetrics StartQueryLoopMetrics(
            this ITelemetryClient telemetryClient,
            [CallerFilePath] string sourceFilePath = "",
            [CallerMemberName] string memberName = "")
        {
            return QueryLoopMetrics.New(telemetryClient, sourceFilePath, memberName);
        }
    }
}
