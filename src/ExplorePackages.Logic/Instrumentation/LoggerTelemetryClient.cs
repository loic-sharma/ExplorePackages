﻿using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Knapcode.ExplorePackages
{
    public class LoggerTelemetryClient : ITelemetryClient
    {
        private readonly ILogger<LoggerTelemetryClient> _logger;

        public LoggerTelemetryClient(ILogger<LoggerTelemetryClient> logger)
        {
            _logger = logger;
        }

        public IMetric GetMetric(string metricId)
        {
            return new LoggerMetric(metricId, Array.Empty<string>(), _logger);
        }

        public IMetric GetMetric(string metricId, string dimension1Name)
        {
            return new LoggerMetric(metricId, new[] { dimension1Name }, _logger);
        }

        public IMetric GetMetric(string metricId, string dimension1Name, string dimension2Name)
        {
            return new LoggerMetric(metricId, new[] { dimension1Name, dimension2Name }, _logger);
        }

        public void TrackMetric(string name, double value, IDictionary<string, string> properties)
        {
            _logger.LogInformation("Metric emitted: {MetricName} = {MetricValue} with properties {Properties}", name, value, JsonConvert.SerializeObject(properties));
        }
    }
}
