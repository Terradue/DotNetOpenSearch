using System;
using System.Linq;
using Terradue.ServiceModel.Syndication;
using System.Xml;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Collections.Specialized;
using System.Xml.Linq;

namespace Terradue.OpenSearch.Result {
    public class AtomFeed : SyndicationFeed, IOpenSearchResultCollection {

        public AtomFeed() : base()  {
        }

        public AtomFeed(IEnumerable<SyndicationItem> items) : base(items)  {
        }

        public AtomFeed(string title, string description, Uri feedAlternateLink, string id, DateTimeOffset date) : base (title,description,feedAlternateLink,id,date){}

        public AtomFeed(SyndicationFeed feed) : base(feed, true) {
        }

        public AtomFeed(SyndicationFeed feed, bool cloneItems) : base(feed, cloneItems) {
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

        public void SerializeToStream(System.IO.Stream stream) {
            var sw = XmlWriter.Create(stream);
            Atom10FeedFormatter atomFormatter = new Atom10FeedFormatter(this);
            atomFormatter.WriteTo(sw);
            sw.Flush();
            sw.Close();
        }

        public string SerializeToString() {
            MemoryStream ms = new MemoryStream();
            SerializeToStream(ms);
            ms.Seek(0, SeekOrigin.Begin);
            StreamReader sr = new StreamReader(ms);
            return sr.ReadToEnd();
        }

        public IOpenSearchResultCollection DeserializeFromStream(Stream stream) {
            throw new NotImplementedException();
        }

        IEnumerable<IOpenSearchResultItem> IOpenSearchResultCollection.Items {
            get {
                return AtomItems.ToArray();
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
            set {
                base.LastUpdatedTime = new DateTimeOffset(value);
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
                    long.TryParse(elements[0].InnerText, out value);
                ;
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

        public string ContentType {
            get {
                return "application/atom+xml";
            }
        }

        #endregion

        public static IOpenSearchResultCollection CreateFromOpenSearchResultCollection(IOpenSearchResultCollection results) {
            if (results == null)
                throw new ArgumentNullException("results");

            AtomFeed feed = new AtomFeed(new SyndicationFeed());

            if ( results.ElementExtensions != null )
                feed.ElementExtensions = new SyndicationElementExtensionCollection(results.ElementExtensions);

            if ( results.Date.Ticks > 0 )
                feed.Date = results.Date;
            feed.Links = new Collection<SyndicationLink>(results.Links);

            if (results.Items != null) {
                List<AtomItem> items = new List<AtomItem>();
                foreach (var item in results.Items) {
                    items.Add(AtomItem.FromOpenSearchResultItem(item));
                }
                feed.AtomItems = items;
            }
            return feed;
        }
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
            set {
                base.Title = new TextSyndicationContent(value);
            }
        }

        public DateTime Date {
            get {
                return base.PublishDate.DateTime;
            }
            set {
                base.PublishDate = new DateTimeOffset(value);
            }
        }

        public string Identifier {
            get {
                foreach (var ext in this.ElementExtensions) {
                    XmlElement element = ext.GetObject<XmlElement>();
                    if (element.LocalName == "identifier")
                        return element.InnerText;
                }
                return base.Id;
            }
            set {
                foreach (var ext in this.ElementExtensions.ToArray()) {
                    if (ext.OuterName == "identifier" && ext.OuterNamespace == "http://purl.org/dc/elements/1.1/") {
                        this.ElementExtensions.Remove(ext);
                        continue;
                    }
                }
                this.ElementExtensions.Add(new XElement(XName.Get("identifier", "http://purl.org/dc/elements/1.1/"), value).CreateReader());
            }
        }


        public bool ShowNamespaces {
            get {
                return true;
            }
            set {
                ;
                ;
            }
        }

        #endregion

        public static AtomItem FromOpenSearchResultItem(IOpenSearchResultItem result) {
            if (result == null)
                throw new ArgumentNullException("result");

            AtomItem item = new AtomItem();

            item.Id = result.Id;
            item.Identifier = result.Identifier;
            if ( result.Date.Ticks != 0 )
                item.Date = result.Date;
            item.Links = result.Links;
            item.Title = result.Title;

            item.ElementExtensions = new SyndicationElementExtensionCollection(result.ElementExtensions);

            return item;
        }
    }
}

