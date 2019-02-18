using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Xml.Serialization;

namespace Terradue.OpenSearch.Benchmarking
{
    [SerializableAttribute()]
    [XmlTypeAttribute(Namespace = "http://www.terradue.com/benchmark")]
    [XmlRootAttribute(Namespace = "http://www.terradue.com/benchmark", IsNullable = false)]
    public class Benchmark
    {
        Collection<Metric> metrics = new Collection<Metric>();

        public Collection<Metric> Metrics
        {
            get
            {
                return metrics;
            }

            set
            {
                metrics = value;
            }
        }
    }

    public class Metric
    {
        private double longLength;
        private string v1;
        private string v2;

        public Metric(double value, string uom, string description)
        {
            this.Value = value;
            this.Uom = uom;
            this.Description = description;
        }

        public double Value { get; set; }

        public string Uom { get; set; }

        public string Description { get; set; }
    }
}
