﻿using System.Xml.Serialization;

namespace Terradue.OpenSearch.Benchmarking
{
    [XmlRootAttribute("Metric", Namespace = "http://www.terradue.com/metrics", IsNullable = false)]
    public class DoubleMetric : Metric
    {
        public DoubleMetric() { }

        public DoubleMetric(string id, double value, string uom, string description = null)
        {
            this.Identifier = id;
            this.val = value;
            this.Uom = uom;
            this.Description = description;
        }

        double val;

        [XmlIgnore]
        public override object Value
        {
            get
            {
                return val;
            }
            set
            {
                val = (double)value;
            }
        }

        [XmlAttribute]
        public override string Uom { get; set; }

        [XmlAttribute]
        public override string Description { get; set; }

        [XmlText]
        public override string Text
        {
            get
            {
                return val.ToString();
            }

            set
            {
                val = double.Parse(value);
            }
        }

        [XmlAttribute]
        public override string Identifier { get; set; }
    }
}
