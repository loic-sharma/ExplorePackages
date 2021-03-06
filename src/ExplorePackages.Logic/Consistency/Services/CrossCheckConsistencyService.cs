﻿using System.Threading.Tasks;

namespace Knapcode.ExplorePackages
{
    public class CrossCheckConsistencyService : IConsistencyService<CrossCheckConsistencyReport>
    {
        private readonly PackagesContainerConsistencyService _packagesContainer;
        private readonly FlatContainerConsistencyService _flatContainer;

        public CrossCheckConsistencyService(
            PackagesContainerConsistencyService packagesContainer,
            FlatContainerConsistencyService flatContainer)
        {
            _packagesContainer = packagesContainer;
            _flatContainer = flatContainer;
        }

        public async Task<CrossCheckConsistencyReport> GetReportAsync(
            PackageConsistencyContext context,
            PackageConsistencyState state,
            IProgressReporter progressReporter)
        {
            await PopulateStateAsync(context, state, progressReporter);

            var doPackageContentsMatch = state.PackagesContainer.PackageContentMetadata?.ContentMD5 == state.FlatContainer.PackageContentMetadata?.ContentMD5;

            var isConsistent = doPackageContentsMatch;

            return new CrossCheckConsistencyReport(
                isConsistent,
                doPackageContentsMatch);
        }

        public async Task<bool> IsConsistentAsync(
            PackageConsistencyContext context,
            PackageConsistencyState state,
            IProgressReporter progressReporter)
        {
            var report = await GetReportAsync(context, state, progressReporter);

            return report.IsConsistent;
        }

        public async Task PopulateStateAsync(
            PackageConsistencyContext context,
            PackageConsistencyState state,
            IProgressReporter progressReporter)
        {
            await _packagesContainer.PopulateStateAsync(context, state, progressReporter);
            await _flatContainer.PopulateStateAsync(context, state, progressReporter);
        }
    }
}
