using System;
using System.Linq;
using Terradue.ServiceModel.Syndication;
using System.Xml;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Collections.Specialized;
using System.Xml.Linq;
using System.Web;

namespace Terradue.OpenSearch.Result {
    public class AtomFeed : SyndicationFeed, IOpenSearchResultCollection {

        List<AtomItem> items;

        public AtomFeed() : base()  {
            items = new List<AtomItem>();
        }

        public AtomFeed(IEnumerable<AtomItem> items) : base()  {
            items = new List<AtomItem>(items);
        }

        public AtomFeed(string title, string description, Uri feedAlternateLink, string id, DateTimeOffset date) : base (title,description,feedAlternateLink,id,date){
            items = new List<AtomItem>();
        }

        public AtomFeed(AtomFeed feed) : base(feed, false) {
            items = feed.Items.Select(i => new AtomItem(i)).ToList();
        }

        public AtomFeed(SyndicationFeed feed) : base(feed, false) {
            items = feed.Items.Select(i => new AtomItem(i)).ToList();
        }

        public AtomFeed(AtomFeed feed, bool cloneItems) : base(feed, false) {
            if (cloneItems == true) {
                items = feed.Items.Select(i => new AtomItem(i)).ToList();
            } else
                items = feed.items;

        }

        public new static AtomFeed Load(XmlReader reader) {
            var feed = Load<SyndicationFeed>(reader);
            return new AtomFeed(feed);
        }

        public new  IEnumerable<AtomItem> Items {
            get {
                return items.ToArray();
            }

            set{
                items = value.ToList();
            }
        }

        public SyndicationFeed Feed {
            get {
                SyndicationFeed feed = new SyndicationFeed(this, true);
                feed.Items = this.Items.Select(i => new SyndicationItem(i));
                return feed;
            }
        }

        #region IOpenSearchResultCollection implementation

        public void SerializeToStream(System.IO.Stream stream) {
            var sw = XmlWriter.Create(stream);
            Atom10FeedFormatter atomFormatter = new Atom10FeedFormatter(this.Feed);
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
                return Items;
            }
        }

        string IOpenSearchResultCollection.Title {
            get {
                return base.Title.Text;
            }
            set {
                base.Title = new TextSyndicationContent(value);
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
                return HttpUtility.UrlEncode(Id);
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
                feed.Items = items;
            }
            return feed;
        }
    }

}

