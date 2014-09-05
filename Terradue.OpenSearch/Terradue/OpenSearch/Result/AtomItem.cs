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

    public class AtomItem : SyndicationItem, IOpenSearchResultItem, IEquatable<AtomItem> {
        public AtomItem() {
        }

        public AtomItem(string title, string description, Uri feedAlternateLink, string id, DateTimeOffset date) : base (title,description,feedAlternateLink,id,date){}

        public AtomItem(string title, SyndicationContent content, Uri feedAlternateLink, string id, DateTimeOffset date) : base (title,content,feedAlternateLink,id,date){}

        public AtomItem(AtomItem si) : base(si) {

        }

        public AtomItem(SyndicationItem si) : base(si) {

        }

        public SyndicationItem ToSyndicationItem() {
            return this;
        }

        #region IOpenSearchResultItem implementation

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

        public new string Title {
            get {
                return base.Title.Text;
            }
            protected set {
                base.Title = new TextSyndicationContent(value);
            }
        }

        public DateTime Date {
            get {
                return base.PublishDate.DateTime;
            }
            protected set {
                base.PublishDate = new DateTimeOffset(value);
            }
        }

        public string Identifier {
            get {
                var identifier = ElementExtensions.ReadElementExtensions<string>("identifier", "http://purl.org/dc/elements/1.1/");
                return identifier.Count == 0 ? base.Id : identifier[0];
            }
            protected set {
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
            }
        }

        #endregion

        #region IEquatable implementation

        public bool Equals(AtomItem other) {
            if (this.Id == other.Id) return true;
            return false;
        }

        #endregion

        public override int GetHashCode()
        {

            //Get hash code for the Name field if it is not null.
            int hashProductName = Id == null ? 0 : Id.GetHashCode();

            return hashProductName;
        }

        public static AtomItem FromOpenSearchResultItem(IOpenSearchResultItem result) {
            if (result == null)
                throw new ArgumentNullException("result");

            AtomItem item = new AtomItem();

            item.ElementExtensions = new SyndicationElementExtensionCollection(result.ElementExtensions);

            item.Id = result.Id;
            item.Identifier = result.Identifier;
            if ( result.Date.Ticks != 0 )
                item.Date = result.Date;
            item.Links = result.Links;
            item.Title = result.Title;
            item.LastUpdatedTime = DateTime.UtcNow;

            return item;
        }
    }
}
