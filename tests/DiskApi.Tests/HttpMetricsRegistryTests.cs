using Common.Infrastructure.Metrics;
using Xunit;

namespace DiskApi.Tests;

public sealed class HttpMetricsRegistryTests
{
    [Fact]
    public void ExportsRequestCountersAndCumulativeDurationBuckets()
    {
        var registry = new HttpMetricsRegistry();
        registry.Record("GET", 200, 4);
        registry.Record("GET", 200, 30);

        var output = registry.ExportPrometheus();

        Assert.Contains("diskapi_http_requests_total{method=\"GET\",status_code=\"200\"} 2", output);
        Assert.Contains("diskapi_http_request_duration_milliseconds_bucket{method=\"GET\",status_code=\"200\",le=\"5\"} 1", output);
        Assert.Contains("diskapi_http_request_duration_milliseconds_bucket{method=\"GET\",status_code=\"200\",le=\"50\"} 2", output);
        Assert.Contains("diskapi_http_request_duration_milliseconds_count{method=\"GET\",status_code=\"200\"} 2", output);
    }

    [Fact]
    public void EscapesLabelValues()
    {
        var registry = new HttpMetricsRegistry();
        registry.Record("GET\n\"\\", 500, 1);

        Assert.Contains("method=\"GET\\n\\\"\\\\\"", registry.ExportPrometheus());
    }
}
