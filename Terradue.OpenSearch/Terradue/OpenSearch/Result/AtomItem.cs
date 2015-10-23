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

        public AtomItem(string title, string description, Uri feedAlternateLink, string id, DateTimeOffset date) : base(title, description, feedAlternateLink, id, date) {
        }

        public AtomItem(string title, SyndicationContent content, Uri feedAlternateLink, string id, DateTimeOffset date) : base(title, content, feedAlternateLink, id, date) {
        }

        public AtomItem(AtomItem si) : base(si) {
            this.ReferenceData = si.ReferenceData;
        }

        public AtomItem(SyndicationItem si) : base(si) {

        }

        public SyndicationItem ToSyndicationItem() {
            return this;
        }

        public object ReferenceData { get; set; }

        #region IOpenSearchResultItem implementation

        public new string Id {
            get {
                var links = Links.Where(l => l.RelationshipType == "self").ToArray();
                if (links.Count() > 0)
                    return links[0].Uri.ToString();
                return base.Id;
            }
            set {
                base.Id = value;
            }
        }

        public new DateTime LastUpdatedTime {
            get {
                return base.LastUpdatedTime.DateTime;
            }
            set {
                base.LastUpdatedTime = new DateTimeOffset(value);
            }
        }

        public new DateTime PublishDate {
            get {
                return base.PublishDate.DateTime;
            }
            set {
                base.PublishDate = new DateTimeOffset(value);
            }
        }

        public string Identifier {
            get {
                var identifier = ElementExtensions.ReadElementExtensions<string>("identifier", "http://purl.org/dc/elements/1.1/");
                return identifier.Count == 0 ? base.Id : identifier[0];
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
            }
        }

        string sortKey;
        public string SortKey {
            get {
                if (sortKey == null)
                    return LastUpdatedTime.ToUniversalTime().ToString("O");
                return sortKey.ToString();
            }
            set {
                sortKey = value;
            }
        }

        #endregion

        #region IEquatable implementation

        public bool Equals(AtomItem other) {
            if (this.Id == other.Id)
                return true;
            return false;
        }

        #endregion

        public override int GetHashCode() {

            //Get hash code for the Name field if it is not null.
            int hashProductName = Id == null ? 0 : Id.GetHashCode();

            return hashProductName;
        }

        public static AtomItem FromOpenSearchResultItem(IOpenSearchResultItem result) {
            if (result == null)
                throw new ArgumentNullException("result");

            if (result is AtomItem)
                return (AtomItem)result;

            AtomItem item = new AtomItem();

            item.ElementExtensions = new SyndicationElementExtensionCollection(result.ElementExtensions);

            item.Id = result.Id;
            item.Identifier = result.Identifier;
            if (result.LastUpdatedTime.Ticks != 0)
                item.LastUpdatedTime = result.LastUpdatedTime;
            if (result.PublishDate.Ticks != 0)
                item.PublishDate = result.PublishDate;
            item.Links = result.Links;
            item.Title = result.Title;
            item.Summary = result.Summary;
            item.Authors.Clear();
            result.Authors.FirstOrDefault(a => {
                item.Authors.Add(a);
                return false;
            });
            result.Categories.FirstOrDefault(c => {
                item.Categories.Add(c);
                return false;
            });
            item.Content = result.Content;
            result.Contributors.FirstOrDefault(c => {
                item.Contributors.Add(c);
                return false;
            });
            item.Copyright = result.Copyright;

            item.sortKey = result.SortKey;

            return item;
        }
    }
}
