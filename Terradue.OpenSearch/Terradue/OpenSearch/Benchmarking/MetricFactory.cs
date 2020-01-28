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

        public static XmlSerializer Serializer = new XmlSerializer(typeof(Metrics), new Type[] { typeof(Metric), typeof(DoubleMetric), typeof(LongMetric) });

        public static XmlSerializerNamespaces Namespaces = new XmlSerializerNamespaces(new XmlQualifiedName[] { new XmlQualifiedName("t2m", "http://www.terradue.com/metrics") });

        public static void GenerateBasicMetrics(OpenSearchRequest request, ref IOpenSearchResponse response)
        {
            List<Metric> metrics = new List<Metric>();
            metrics.Add(new LongMetric("size", ((byte[])response.GetResponseObject()).LongLength, "bytes", "Size of the query bytes retrieved"));
            metrics.AddRange(response.Metrics);

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
