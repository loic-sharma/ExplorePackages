﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Knapcode.ExplorePackages.Logic;
using Newtonsoft.Json;
using NuGet.Common;
using NuGet.Versioning;

namespace Knapcode.ExplorePackages.Commands
{
    public class CheckPackageCommand : ICommand
    {
        private readonly PackageConsistencyService _service;
        private readonly PackageQueryContextBuilder _contextBuilder;
        private readonly ILogger _log;

        public CheckPackageCommand(PackageConsistencyService service, PackageQueryContextBuilder contextBuilder, ILogger log)
        {
            _service = service;
            _contextBuilder = contextBuilder;
            _log = log;
        }

        public async Task ExecuteAsync(IReadOnlyList<string> args, CancellationToken token)
        {
            if (args.Count < 3)
            {
                _log.LogError("The second and third parameters should be the package ID then the package version.");
                return;
            }

            var id = args[1].Trim();
            var version = args[2].Trim();

            if (!NuGetVersion.TryParse(version, out var parsedVersion))
            {
                _log.LogError($"The version '{version}' could not be parsed.");
                return;
            }

            var context = await _contextBuilder.GetPackageQueryContextAsync(id, version);
            if (context == null)
            {
                _log.LogError($"The package {id} {version} could not be found.");
            }

            var report = await _service.GetReportAsync(context);

            var output = new
            {
                Context = new
                {
                    report.Context.Package,
                    report.Context.IsSemVer2,
                },
                report.IsConsistent,
                report.V2,
                report.PackagesContainer,
                report.FlatContainer,
                report.RegistrationOriginal,
                report.RegistrationGzipped,
                report.RegistrationSemVer2,
            };
            Console.WriteLine(JsonConvert.SerializeObject(output, Formatting.Indented));
        }
    }
}