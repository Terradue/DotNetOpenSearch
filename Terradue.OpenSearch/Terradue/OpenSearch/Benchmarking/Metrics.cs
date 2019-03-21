using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace Terradue.OpenSearch.Benchmarking
{
    [SerializableAttribute()]
    [XmlRootAttribute(Namespace = "http://www.terradue.com/metrics", IsNullable = false)]
    public class Metrics
    {
        List<Metric> metrics = new List<Metric>();

        [XmlElement("Metric")]
        public List<Metric> Metric
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
}
