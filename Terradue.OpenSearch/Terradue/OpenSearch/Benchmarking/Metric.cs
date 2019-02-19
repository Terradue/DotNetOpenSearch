using System;
using System.Xml.Serialization;

namespace Terradue.OpenSearch.Benchmarking
{

    [XmlTypeAttribute(Namespace = "http://www.terradue.com/metrics")]
    [XmlRootAttribute(Namespace = "http://www.terradue.com/metrics", IsNullable = false)]
    public abstract class Metric
    {
        [XmlIgnore]
        public abstract object Value { get; set; }

        [XmlAttribute]
        public abstract string Identifier { get; set; }

        [XmlAttribute]
        public abstract string Uom { get; set; }

        [XmlAttribute]
        public abstract string Description { get; set; }

        [XmlText]
        public abstract string SValue { get; }

    }
}
