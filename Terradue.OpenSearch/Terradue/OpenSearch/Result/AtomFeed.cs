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
            items = feed.items.Select(i => new AtomItem(i)).ToList();
        }

        public AtomFeed(SyndicationFeed feed) : base(feed, false) {
            items = feed.Items.Select(i => new AtomItem(i)).ToList();
        }

        public AtomFeed(AtomFeed feed, bool cloneItems) : base(feed, false) {
            if (cloneItems == true) {
                items = feed.items.Select(i => new AtomItem(i)).ToList();
            } else
                items = feed.items;

        }

        public new static AtomFeed Load(XmlReader reader) {
            var feed = Load<SyndicationFeed>(reader);
            return new AtomFeed(feed);
        }

        public SyndicationFeed Feed {
            get {
                SyndicationFeed feed = new SyndicationFeed(this, true);
                feed.Items = this.Items.Cast<SyndicationItem>().Select(i => new SyndicationItem(i));
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

        public new string Id {
            get {
                var links = Links.Where(l => l.RelationshipType == "self").ToArray();
                if (links.Count() > 0) return links[0].Uri.ToString();
                return base.Id;
            }
            set {
                base.Id = value;
            }
        }

        public new IEnumerable<IOpenSearchResultItem> Items {
            get {
                return items.Cast<IOpenSearchResultItem>();
            }
            set{
                items = value.Cast<AtomItem>().ToList();
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
                var identifiers = ElementExtensions.ReadElementExtensions<string>("identifier", "http://purl.org/dc/elements/1.1/");
                if (identifiers.Count > 0)
                    return identifiers[0];
                return null;
            }
            set {
                var identifiers = ElementExtensions.ReadElementExtensions<SyndicationElementExtension>("identifier", "http://purl.org/dc/elements/1.1/");
                foreach (var identifier in identifiers) {
                    ElementExtensions.Remove(identifier);
                }
                ElementExtensions.Add("identifier", "http://purl.org/dc/elements/1.1/", value);
            }
        }

        public long Count {
            get {
                return Items.Count();
            }
        }

        public long TotalResults {
            get {
                var el = ElementExtensions.ReadElementExtensions<string>("totalResults", "http://a9.com/-/spec/opensearch/1.1/");
                if (el.Count > 0)
                    return long.Parse(el[0]);
                return 0;
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

            feed.Id = results.Id;
            feed.Identifier = results.Identifier;
            foreach ( var author in results.Authors ){
                feed.Authors.Add(author);
            }

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

