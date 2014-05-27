using System;
using System.Linq;
using Terradue.ServiceModel.Syndication;
using System.Xml;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Terradue.OpenSearch.Result {
    public class AtomFeed : SyndicationFeed, IOpenSearchResultCollection {
        public AtomFeed(SyndicationFeed feed) :base(feed, false) {
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
                throw new NotImplementedException();
            }
        }

        public DateTime Date {
            get {
                throw new NotImplementedException();
            }
        }

        public string Identifier {
            get {
                throw new NotImplementedException();
            }
        }

        public long Count {
            get {
                throw new NotImplementedException();
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
                return base.Id;
            }
        }

        #endregion
    }
}

