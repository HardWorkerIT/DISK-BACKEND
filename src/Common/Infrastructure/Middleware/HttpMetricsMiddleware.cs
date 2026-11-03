using System.Diagnostics.Metrics;
using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Common.Infrastructure.Metrics;

namespace Common.Infrastructure.Middleware;

public sealed class HttpMetricsMiddleware(RequestDelegate next, HttpMetricsRegistry registry)
{
    private static readonly Meter Meter = new("DiskApi.Http", "1.0.0");
    private static readonly Counter<long> Requests = Meter.CreateCounter<long>("diskapi.http.requests", "requests");
    private static readonly Histogram<double> Duration = Meter.CreateHistogram<double>("diskapi.http.request.duration", "ms");

    public async Task InvokeAsync(HttpContext context)
    {
        var started = Stopwatch.GetTimestamp();
        try
        {
            await next(context);
        }
        finally
        {
            var elapsed = Stopwatch.GetElapsedTime(started).TotalMilliseconds;
            var tags = new TagList
            {
                { "http.request.method", context.Request.Method },
                { "http.response.status_code", context.Response.StatusCode }
            };
            Requests.Add(1, tags);
            Duration.Record(elapsed, tags);
            registry.Record(context.Request.Method, context.Response.StatusCode, elapsed);
        }
    }
}
