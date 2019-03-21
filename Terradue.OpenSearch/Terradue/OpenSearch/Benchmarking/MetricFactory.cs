using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using Terradue.OpenSearch.Request;
using Terradue.OpenSearch.Response;

namespace Terradue.OpenSearch.Benchmarking
{
    public static class MetricFactory
    {

        public static XmlSerializer Serializer = new XmlSerializer(typeof(Metrics), new Type[] { typeof(Metric), typeof(DoubleMetric) });

        public static XmlSerializerNamespaces Namespaces = new XmlSerializerNamespaces(new XmlQualifiedName[] { new XmlQualifiedName("t2m", "http://www.terradue.com/metrics") });

        public static void GenerateBasicMetrics(OpenSearchRequest request, ref IOpenSearchResponse response)
        {
            List<Metric> metrics = new List<Metric>();

            Metric requestTime = response.Metrics.FirstOrDefault(m => m.Identifier == "requestTime" && m.Value is double);
            if (requestTime != null && response is OpenSearchResponse<byte[]>)
            {
                metrics.Add(requestTime);
                metrics.Add(new DoubleMetric("size", (double)((byte[])response.GetResponseObject()).LongLength, "bytes", "Size of the query bytes retrieved"));
                metrics.Add(new DoubleMetric("throughput", (double)((byte[])response.GetResponseObject()).LongLength / (((double)requestTime.Value)/1000), "bytes/s", "Throughput"));
            }

            response.Metrics = metrics;
        }

        public static XmlReader CreateReader(this IEnumerable<Metric> metrics)
        {

            MemoryStream ms = new MemoryStream();

            XmlWriter xw = XmlWriter.Create(ms);

            Metrics metric = new Metrics() { Metric = metrics.ToList() };
            MetricFactory.Serializer.Serialize(xw, metric, MetricFactory.Namespaces);

            ms.Seek(0, SeekOrigin.Begin);

            return XmlReader.Create(ms);
        }
    }
}
