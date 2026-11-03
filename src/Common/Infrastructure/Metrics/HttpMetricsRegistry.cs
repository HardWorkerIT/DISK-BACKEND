using System.Collections.Concurrent;
using System.Globalization;
using System.Text;

namespace Common.Infrastructure.Metrics;

public sealed class HttpMetricsRegistry
{
    private static readonly double[] DurationBuckets = [5, 10, 25, 50, 100, 250, 500, 1000];
    private readonly ConcurrentDictionary<(string Method, int StatusCode), RequestMeasurements> _measurements = new();

    public void Record(string method, int statusCode, double durationMilliseconds)
        => _measurements.GetOrAdd((method, statusCode), static _ => new RequestMeasurements())
            .Record(durationMilliseconds, DurationBuckets);

    public string ExportPrometheus()
    {
        var output = new StringBuilder()
            .AppendLine("# HELP diskapi_http_requests_total Total HTTP requests handled by the API.")
            .AppendLine("# TYPE diskapi_http_requests_total counter");

        foreach (var (key, value) in _measurements.OrderBy(item => item.Key.Method).ThenBy(item => item.Key.StatusCode))
        {
            var labels = $"method=\"{Escape(key.Method)}\",status_code=\"{key.StatusCode}\"";
            output.Append("diskapi_http_requests_total{").Append(labels).Append("} ")
                .AppendLine(value.Count.ToString(CultureInfo.InvariantCulture));
        }

        output.AppendLine("# HELP diskapi_http_request_duration_milliseconds HTTP request duration in milliseconds.")
            .AppendLine("# TYPE diskapi_http_request_duration_milliseconds histogram");
        foreach (var (key, value) in _measurements.OrderBy(item => item.Key.Method).ThenBy(item => item.Key.StatusCode))
        {
            var labels = $"method=\"{Escape(key.Method)}\",status_code=\"{key.StatusCode}\"";
            var cumulative = 0L;
            for (var index = 0; index < DurationBuckets.Length; index++)
            {
                cumulative += value.GetBucketCount(index);
                output.Append("diskapi_http_request_duration_milliseconds_bucket{").Append(labels)
                    .Append(",le=\"").Append(DurationBuckets[index].ToString(CultureInfo.InvariantCulture)).Append("\"} ")
                    .AppendLine(cumulative.ToString(CultureInfo.InvariantCulture));
            }
            output.Append("diskapi_http_request_duration_milliseconds_bucket{").Append(labels).Append(",le=\"+Inf\"} ")
                .AppendLine(value.Count.ToString(CultureInfo.InvariantCulture));
            output.Append("diskapi_http_request_duration_milliseconds_sum{").Append(labels).Append("} ")
                .AppendLine(value.DurationSumMilliseconds.ToString("F3", CultureInfo.InvariantCulture));
            output.Append("diskapi_http_request_duration_milliseconds_count{").Append(labels).Append("} ")
                .AppendLine(value.Count.ToString(CultureInfo.InvariantCulture));
        }
        return output.ToString();
    }

    private static string Escape(string value) => value.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n");

    private sealed class RequestMeasurements
    {
        private readonly long[] _buckets = new long[DurationBuckets.Length];
        private long _count;
        private long _durationMicroseconds;

        public long Count => Interlocked.Read(ref _count);
        public double DurationSumMilliseconds => Interlocked.Read(ref _durationMicroseconds) / 1000d;
        public long GetBucketCount(int index) => Interlocked.Read(ref _buckets[index]);

        public void Record(double durationMilliseconds, double[] buckets)
        {
            Interlocked.Increment(ref _count);
            Interlocked.Add(ref _durationMicroseconds, (long)(durationMilliseconds * 1000));
            for (var index = 0; index < buckets.Length; index++)
            {
                if (durationMilliseconds > buckets[index])
                    continue;
                Interlocked.Increment(ref _buckets[index]);
                break;
            }
        }
    }
}
