﻿using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using NuGet.Versioning;

namespace Knapcode.ExplorePackages.Website.Logic
{
    public class PackageReportHub : Hub
    {
        public const string Path = "/Hubs/PackageReport";

        private readonly PackageConsistencyService _packageConsistencyService;
        private readonly PackageConsistencyContextBuilder _packageConsistencyContextBuilder;
        private readonly LatestCatalogCommitFetcher _latestCatalogCommitFetcher;
        private readonly UrlReporterProvider _urlReporterProvider;

        public PackageReportHub(
            PackageConsistencyService packageConsistencyService,
            PackageConsistencyContextBuilder packageConsistencyContextBuilder,
            LatestCatalogCommitFetcher latestCatalogCommitFetcher,
            UrlReporterProvider urlReporterProvider)
        {
            _packageConsistencyService = packageConsistencyService;
            _packageConsistencyContextBuilder = packageConsistencyContextBuilder;
            _latestCatalogCommitFetcher = latestCatalogCommitFetcher;
            _urlReporterProvider = urlReporterProvider;
        }

        private async Task ExecuteAndReportErrorAsync(Func<Task> executeAsync)
        {
            try
            {
                _urlReporterProvider.SetUrlReporter(new UrlReporter(this));
                await executeAsync();
                _urlReporterProvider.SetUrlReporter(null);
            }
            catch
            {
                await InvokeErrorAsync("An internal server error occurred.");
            }
        }

        public async Task GetLatestCatalog()
        {
            await ExecuteAndReportErrorAsync(GetLatestCatalogInternalAsync);
        }

        private async Task GetLatestCatalogInternalAsync()
        {
            await InvokeProgressAsync(0, "Fetching the latest catalog commit...");

            var commit = await _latestCatalogCommitFetcher.GetLatestCommitAsync(new ProgressReporter(this));

            var catalogItem = commit.First();
            await InvokeProgressAsync(1, $"Using package {catalogItem.PackageId} {catalogItem.PackageVersion}.");

            await InvokeFoundLatestAsync(catalogItem.PackageId, catalogItem.ParsePackageVersion().ToFullString());
        }

        public async Task Start(string id, string version)
        {
            await ExecuteAndReportErrorAsync(() => StartInternalAsync(id, version));
        }

        private async Task StartInternalAsync(string id, string version)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                await InvokeErrorAsync("You must provide a package ID.");
                return;
            }

            if (!StrictPackageIdValidator.IsValid(id))
            {
                await InvokeErrorAsync("The ID you provided is invalid.");
                return;
            }

            if (string.IsNullOrWhiteSpace(version))
            {
                await InvokeErrorAsync("You must provide a package version.");
                return;
            }

            if (!NuGetVersion.TryParse(version, out var parsedVersion))
            {
                await InvokeErrorAsync("The version you provided is invalid.");
                return;
            }

            await InvokeProgressAsync(0, $"Initializing package report for {id} {version}.");

            var state = new PackageConsistencyState();
            var context = await _packageConsistencyContextBuilder.CreateFromServerAsync(id, version, state);

            var packageDeletedStatus = state.Gallery.PackageState.PackageDeletedStatus;
            var isAvailable = packageDeletedStatus == PackageDeletedStatus.NotDeleted;
            string availability;
            switch (packageDeletedStatus)
            {
                case PackageDeletedStatus.NotDeleted:
                    availability = "is available";
                    break;
                case PackageDeletedStatus.SoftDeleted:
                    availability = "was soft deleted";
                    break;
                case PackageDeletedStatus.Unknown:
                    availability = "was hard deleted or never published";
                    break;
                default:
                    throw new NotImplementedException(
                        $"The package deleted status {packageDeletedStatus} is not supported.");
            }

            await InvokeProgressAsync(
                0,
                $"{context.Id} {context.Version} {availability}" +
                $"{(isAvailable && context.IsSemVer2 ? " and is SemVer 2.0.0" : string.Empty)}. " +
                $"The consistency report will now be generated.");

            var report = await _packageConsistencyService.GetReportAsync(context, state, new ProgressReporter(this));

            await InvokeCompleteAsync(report);
        }

        private async Task InvokeProgressAsync(decimal percent, string message)
        {
            await InvokeOnClientAsync("Progress", percent, message);
        }

        private async Task InvokeHttpRequestAsync(string id, string method, string requestUri)
        {
            await InvokeOnClientAsync("HttpRequest", id, method, requestUri);
        }

        private async Task InvokeHttpResponseAsync(string id, int statusCode, string reasonPhrase, decimal duration)
        {
            await InvokeOnClientAsync("HttpResponse", id, statusCode, reasonPhrase, duration);
        }

        private async Task InvokeErrorAsync(string message)
        {
            await InvokeOnClientAsync("Error", message);
        }

        private async Task InvokeCompleteAsync(PackageConsistencyReport report)
        {
            await InvokeOnClientAsync("Complete", report);
        }

        private async Task InvokeFoundLatestAsync(string id, string version)
        {
            await InvokeOnClientAsync("FoundLatest", new { Id = id, Version = version });
        }

        private async Task InvokeOnClientAsync(string method, params object[] args)
        {
            await Clients
                .Client(Context.ConnectionId)
                .SendCoreAsync(method, args);
        }

        private class UrlReporter : IUrlReporter
        {
            private readonly PackageReportHub _hub;

            public UrlReporter(PackageReportHub hub)
            {
                _hub = hub;
            }

            public async Task ReportRequestAsync(Guid id, HttpRequestMessage request)
            {
                await _hub.InvokeHttpRequestAsync(
                    id.ToString(),
                    request.Method.ToString(),
                    request.RequestUri.AbsoluteUri);
            }

            public async Task ReportResponseAsync(Guid id, HttpResponseMessage response, TimeSpan duration)
            {
                await _hub.InvokeHttpResponseAsync(
                    id.ToString(),
                    (int)response.StatusCode,
                    response.ReasonPhrase,
                    (decimal)duration.TotalSeconds);
            }
        }

        private class ProgressReporter : IProgressReporter
        {
            private readonly PackageReportHub _hub;

            public ProgressReporter(PackageReportHub hub)
            {
                _hub = hub;
            }

            public async Task ReportProgressAsync(decimal percent, string message)
            {
                await _hub.InvokeProgressAsync(percent, message);
            }
        }
    }
}
