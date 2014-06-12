using System;
using System.Linq;
using Terradue.ServiceModel.Syndication;
using System.Xml;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Terradue.OpenSearch.Result {
    public class AtomFeed : SyndicationFeed, IOpenSearchResultCollection {
        public AtomFeed(SyndicationFeed feed) :base(feed, true) {
        }

        List<AtomItem> AtomItems {
            get {
                return base.Items.Select(si => new AtomItem(si)).ToList();
            }
            set {
                List<SyndicationItem> list = new List<SyndicationItem>();
                value.FirstOrDefault(ai => {
                    list.Add(ai.ToSyndicationItem());
                    return false;
                });
                base.Items = list;
            }
        }

        #region IOpenSearchResultCollection implementation

        public void Serialize(System.IO.Stream stream) {
            var sw = XmlWriter.Create(stream);
            Atom10FeedFormatter atomFormatter = new Atom10FeedFormatter(this);
            atomFormatter.WriteTo(sw);
            sw.Flush();
            sw.Close();
        }

        IEnumerable<IOpenSearchResultItem> IOpenSearchResultCollection.Items {
            get {
                return AtomItems;
            }
        }

        string IOpenSearchResultCollection.Title {
            get {
                return base.Title.Text;
            }
        }

        public DateTime Date {
            get {
                return base.LastUpdatedTime.DateTime;
            }
        }

        public string Identifier {
            get {
                List<XmlElement> elements = new List<XmlElement>();
                foreach (var ext in ElementExtensions) {
                    XmlElement element = ext.GetObject<XmlElement>();
                    if (element.LocalName == "identifier")
                        return element.InnerText;
                }
                return Id;
            }
        }

        public long Count {
            get {
                Collection<XmlElement> elements = ElementExtensions.ReadElementExtensions<XmlElement>("totalResults", "http://a9.com/-/spec/opensearch/1.1/");
                long value = 0;
                if (elements.Count > 0)
                    long.TryParse(elements[0].InnerText, out value);;
                return value;
            }
        }


        bool showNamespaces;
        public bool ShowNamespaces {
            get {
                return showNamespaces;
            }
            set {
                showNamespaces = value;
            }
        }
        #endregion
    }

    public class AtomItem : SyndicationItem, IOpenSearchResultItem {
        public AtomItem() {
        }

        public AtomItem(SyndicationItem si) : base(si) {

        }

        public SyndicationItem ToSyndicationItem() {
            return this;
        }

        #region IOpenSearchResultItem implementation

        public new string Title {
            get {
                return base.Title.Text;
            }
        }

        public DateTime Date {
            get {
                return base.PublishDate.DateTime;
            }
        }

        public string Identifier {
            get {
                List<XmlElement> elements = new List<XmlElement>();
                foreach (var ext in this.ElementExtensions) {
                    XmlElement element = ext.GetObject<XmlElement>();
                    if (element.LocalName == "identifier")
                        return element.InnerText;
                }
                return base.Id;
            }
        }


        public bool ShowNamespaces {
            get {
                return true;
            }
            set {
                ;;
            }
        }
        #endregion
    }
}

