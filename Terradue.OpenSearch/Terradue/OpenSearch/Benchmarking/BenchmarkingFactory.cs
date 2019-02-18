using System;
using Terradue.OpenSearch.Response;

namespace Terradue.OpenSearch.Benchmarking
{
    public static class BenchmarkingFactory
    {
        public static Benchmark CreateBenchmarkFromResponse(OpenSearchResponse<byte[]> response)
        {

            Benchmark benchmark = new Benchmark();

            benchmark.Metrics.Add(new Metric((double)((byte[])response.GetResponseObject()).LongLength, "bytes", "Size of the query bytes retrieved"));
            benchmark.Metrics.Add(new Metric(response.RequestTime.TotalMilliseconds, "ms", "Total time for retrieveing the query"));
            benchmark.Metrics.Add(new Metric((double)((byte[])response.GetResponseObject()).LongLength/response.RequestTime.TotalSeconds, "bytes/s", "Throughput"));

            return benchmark;

        }
    }
}
