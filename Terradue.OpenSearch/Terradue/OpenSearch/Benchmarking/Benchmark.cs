using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace Terradue.OpenSearch.Benchmarking
{
    [SerializableAttribute()]
    [XmlTypeAttribute(Namespace = "http://www.terradue.com/benchmark")]
    [XmlRootAttribute(Namespace = "http://www.terradue.com/benchmark", IsNullable = false)]
    public class Benchmark
    {
        Collection<Metric> metrics = new Collection<Metric>();

        static XmlSerializer ser = new XmlSerializer(typeof(Benchmark));

        static XmlSerializerNamespaces ns = new XmlSerializerNamespaces(new XmlQualifiedName[] { new XmlQualifiedName("t2bench", "http://www.terradue.com/benchmark") });

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

        public XmlReader CreateReader()
        {

            MemoryStream ms = new MemoryStream();

            XmlWriter xw = XmlWriter.Create(ms);

            ser.Serialize(xw, this, ns);

            ms.Seek(0, SeekOrigin.Begin);

            return XmlReader.Create(ms);
        }
    }

    public class Metric
    {
        public Metric() { }

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
