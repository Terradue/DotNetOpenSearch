﻿//
//  AtomFeed.cs
//
//  Author:
//       Emmanuel Mathot <emmanuel.mathot@terradue.com>
//
//  Copyright (c) 2014 Terradue

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

/*!

\defgroup Atom Atom Feed
@{
This is the representation in Atom of OpenSearch query results

\ingroup Model

\xrefitem cptype_document "Document" "Documents" represents \ref Syndication

\xrefitem norm "Normative References" "Normative References" [OpenSearch 1.1](http://www.opensearch.org/Specifications/OpenSearch/1.1)

\xrefitem norm "Normative References" "Normative References" [The Atom Syndication Format RFC4287](https://tools.ietf.org/html/rfc4287)

@}

*/

namespace Terradue.OpenSearch.Result {
    public class AtomFeed : SyndicationFeed, IOpenSearchResultCollection {

        List<AtomItem> items;

        public AtomFeed() : base()  {
            items = new List<AtomItem>();
            base.LastUpdatedTime = DateTime.UtcNow;
            AttributeExtensions.Add(
                new XmlQualifiedName("dc", XNamespace.Xmlns.ToString()),
                "http://purl.org/dc/elements/1.1/");
            AttributeExtensions.Add(
                new XmlQualifiedName("os", XNamespace.Xmlns.ToString()),
                "http://a9.com/-/spec/opensearch/1.1/");
        }

        public AtomFeed(IEnumerable<AtomItem> items) : base()  {
            items = new List<AtomItem>(items);
            base.LastUpdatedTime = DateTime.UtcNow;
            AttributeExtensions.Add(
                new XmlQualifiedName("dc", XNamespace.Xmlns.ToString()),
                "http://purl.org/dc/elements/1.1/");
            AttributeExtensions.Add(
                new XmlQualifiedName("os", XNamespace.Xmlns.ToString()),
                "http://a9.com/-/spec/opensearch/1.1/");
        }

        public AtomFeed(string title, string description, Uri feedAlternateLink, string id, DateTimeOffset date) : base (title,description,feedAlternateLink,id,date){
            items = new List<AtomItem>();
            base.LastUpdatedTime = DateTime.UtcNow;
            AttributeExtensions.Add(
                new XmlQualifiedName("dc", XNamespace.Xmlns.ToString()),
                "http://purl.org/dc/elements/1.1/");
            AttributeExtensions.Add(
                new XmlQualifiedName("os", XNamespace.Xmlns.ToString()),
                "http://a9.com/-/spec/opensearch/1.1/");
        }

        public AtomFeed(SyndicationFeed feed) : base(feed, false) {
            items = feed.Items.Select(i => new AtomItem(i)).ToList();
            base.LastUpdatedTime = DateTime.UtcNow;
            if (!AttributeExtensions.ContainsKey(new XmlQualifiedName("dc", XNamespace.Xmlns.ToString()))){
                AttributeExtensions.Add(
                    new XmlQualifiedName("dc", XNamespace.Xmlns.ToString()),
                    "http://purl.org/dc/elements/1.1/");
            }
            if (!AttributeExtensions.ContainsKey(new XmlQualifiedName("os", XNamespace.Xmlns.ToString()))){
                AttributeExtensions.Add(
                new XmlQualifiedName("os", XNamespace.Xmlns.ToString()),
                "http://a9.com/-/spec/opensearch/1.1/");
            }
        }

        public AtomFeed(AtomFeed feed, bool cloneItems = false) : base(feed, false) {
            if (cloneItems == true) {
                items = feed.items.Select(i => new AtomItem(i)).ToList();
            } else
                items = feed.items;
            if (!AttributeExtensions.ContainsKey(new XmlQualifiedName("dc", XNamespace.Xmlns.ToString()))){
                AttributeExtensions.Add(
                    new XmlQualifiedName("dc", XNamespace.Xmlns.ToString()),
                    "http://purl.org/dc/elements/1.1/");
            }
            if (!AttributeExtensions.ContainsKey(new XmlQualifiedName("os", XNamespace.Xmlns.ToString()))){
                AttributeExtensions.Add(
                new XmlQualifiedName("os", XNamespace.Xmlns.ToString()),
                "http://a9.com/-/spec/opensearch/1.1/");
            }
            QueryTimeSpan = feed.QueryTimeSpan;
            OpenSearchable = feed.OpenSearchable;
            Parameters = feed.Parameters;
            TotalResults = feed.TotalResults;

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

        TextSyndicationContent IOpenSearchResultCollection.Title {
            get {
                return base.Title;
            }
            set {
                base.Title = value;
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
                if (string.IsNullOrEmpty(value))
                    return;
                this.ElementExtensions.Add("identifier", "http://purl.org/dc/elements/1.1/", value);
            }
        }

        public long Count {
            get {
                return Items.Count();
            }
        }

        public long TotalResults {
            get {
                var totalResults = ElementExtensions.ReadElementExtensions<string>("totalResults", "http://a9.com/-/spec/opensearch/1.1/");
                return totalResults.Count == 0 ? 0 : long.Parse(totalResults[0]);
            }
            set {
                foreach (var ext in this.ElementExtensions.ToArray()) {
                    if (ext.OuterName == "totalResults" && ext.OuterNamespace == "http://a9.com/-/spec/opensearch/1.1/") {
                        this.ElementExtensions.Remove(ext);
                        continue;
                    }
                }
                this.ElementExtensions.Add("totalResults", "http://a9.com/-/spec/opensearch/1.1/", value);
            }
        }

        public string ContentType {
            get {
                return "application/atom+xml";
            }
        }


        IOpenSearchable openSearchable;
        public IOpenSearchable OpenSearchable {
            get {
                return openSearchable;
            }
            set {
                openSearchable = value;
            }
        }

        NameValueCollection parameters;
        public NameValueCollection Parameters {
            get {
                return parameters;
            }
            set {
                parameters = value;
            }
        }

        public TimeSpan QueryTimeSpan {
            get {
                var duration = ElementExtensions.ReadElementExtensions<double>("queryTime", "http://purl.org/dc/elements/1.1/");
                return duration.Count == 0 ? new TimeSpan() : TimeSpan.FromMilliseconds(duration[0]);
            }
            set {
                foreach (var ext in this.ElementExtensions.ToArray()) {
                    if (ext.OuterName == "queryTime" && ext.OuterNamespace == "http://purl.org/dc/elements/1.1/") {
                        this.ElementExtensions.Remove(ext);
                        continue;
                    }
                }
                this.ElementExtensions.Add("queryTime", "http://purl.org/dc/elements/1.1/", value.TotalMilliseconds);
            }
        }

        public IComparer<IOpenSearchResultItem> SortingComparer => new MethodSortingItemComparer();
        #endregion

        public static IOpenSearchResultCollection CreateFromOpenSearchResultCollection(IOpenSearchResultCollection results) {
            if (results == null)
                throw new ArgumentNullException("results");

            AtomFeed feed = new AtomFeed(new SyndicationFeed());

            feed.Id = results.Id;
            feed.Identifier = results.Identifier;
            feed.Title = results.Title;

            foreach ( var author in results.Authors ){
                feed.Authors.Add(author);
            }

            if ( results.ElementExtensions != null )
                feed.ElementExtensions = new SyndicationElementExtensionCollection(results.ElementExtensions);

            if ( results.LastUpdatedTime.Ticks > 0 )
                feed.LastUpdatedTime = results.LastUpdatedTime;
            feed.Links = new Collection<SyndicationLink>(results.Links);

            if (results.Items != null) {
                List<AtomItem> items = new List<AtomItem>();
                foreach (var item in results.Items) {
                    items.Add(AtomItem.FromOpenSearchResultItem(item));
                }
                feed.Items = items;
            }

            feed.QueryTimeSpan = results.QueryTimeSpan;
            feed.OpenSearchable = results.OpenSearchable;
            feed.TotalResults = results.TotalResults;

            return feed;

        }

        public object Clone() {
            return new AtomFeed(this, true);
        }
    }

}

