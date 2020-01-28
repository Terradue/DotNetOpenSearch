using System.Xml.Serialization;

namespace Terradue.OpenSearch.Benchmarking
{
    [XmlRootAttribute("Metric", Namespace = "http://www.terradue.com/metrics", IsNullable = false)]
    public class LongMetric : Metric
    {
        public LongMetric() { }

        public LongMetric(string id, long value, string uom, string description = null)
        {
            this.Identifier = id;
            this.val = value;
            this.Uom = uom;
            this.Description = description;
        }

        long val;

        [XmlIgnore]
        public override object Value
        {
            get
            {
                return val;
            }
            set
            {
                val = (long)value;
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
                val = long.Parse(value);
            }
        }

        [XmlAttribute]
        public override string Identifier { get; set; }
    }
}
