//
//  RdfXmlDocument.cs
//
//  Author:
//       Emmanuel Mathot <emmanuel.mathot@terradue.com>
//
//  Copyright (c) 2014 Terradue

using System;
using System.Linq;
using System.Xml;
using System.Collections.Generic;
using Terradue.ServiceModel.Syndication;
using System.Xml.Serialization;
using System.Xml.Linq;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using System.IO;

namespace Terradue.OpenSearch.Result {
    /// <summary>
    /// Rdf xml document.
    /// </summary>
    public class RdfXmlDocument : XDocument, IOpenSearchResultCollection {
        XElement rdf, series, description;
        XNamespace rdfns, dclite4g, dc, os, atom;
        XDocument doc;

        List<RdfXmlResult> items;

        /// <summary>
        /// Initializes a new instance of the <see cref="Terradue.OpenSearch.Result.RdfXmlDocument"/> class.
        /// </summary>
        internal RdfXmlDocument(XDocument doc) : base(doc) {

            this.doc = doc;
            rdfns = XNamespace.Get("http://www.w3.org/1999/02/22-rdf-syntax-ns#");
            rdf = this.Element(rdfns + "RDF");
            if (rdf == null)
                throw new FormatException("Not a RDF document");
            description = rdf.Element(rdfns + "Description");
            if (description == null)
                throw new FormatException("RDF document does not contain a description node");
            dclite4g = XNamespace.Get("http://xmlns.com/2008/dclite4g#");
            dc = XNamespace.Get("http://purl.org/dc/elements/1.1/");
            os = XNamespace.Get("http://a9.com/-/spec/opensearch/1.1/");
            atom = XNamespace.Get("http://www.w3.org/2005/Atom");
            series = rdf.Element(dclite4g + "Series");

            items = LoadItems(rdf);

        }

        public RdfXmlDocument(IOpenSearchResultCollection results) : base() {

            doc = new XDocument();
            rdf = new XElement(XName.Get("RDF", "http://www.w3.org/1999/02/22-rdf-syntax-ns#"));
            doc.Add(rdf);

            description = new XElement(XName.Get("Description", "http://www.w3.org/1999/02/22-rdf-syntax-ns#"));

            if (results.ElementExtensions != null) {
                foreach (SyndicationElementExtension ext in results.ElementExtensions) {
                    rdf.Add(ext.GetObject<XElement>());
                }
            }

            if (results.Date.Ticks > 0)
                description.Add(new XElement(XName.Get("date", "http://purl.org/dc/elements/1.1/")), results.Date.ToString("yyyy-MM-ddThh:mm:ss.fZ"));

            if (results.Items != null) {
                List<RdfXmlResult> items = new List<RdfXmlResult>();
                foreach (var item in results.Items) {
                    var newItem = new RdfXmlResult(item);
                    items.Add(newItem);
                    rdf.Add(newItem.Root);
                }
                this.items = items;
            }
        }

        /// <summary>
        /// Load the specified reader.
        /// </summary>
        /// <param name="reader">Reader.</param>
        public new static RdfXmlDocument Load(XmlReader reader) {
            return new RdfXmlDocument(XDocument.Load(reader));
        }

        public XmlNameTable NameTable {
            get {
                return doc.CreateReader().NameTable;
            }
        }

        public static List<RdfXmlResult> LoadItems(XElement rdf) {

            List<RdfXmlResult> items = new List<RdfXmlResult>();

            foreach (XElement dataSet in rdf.Elements(XName.Get("DataSet","http://xmlns.com/2008/dclite4g#"))) {

                items.Add(new RdfXmlResult(dataSet));

            }

            return items;
        }

        #region IResultCollection implementation

        public string Id {
            get {
                var link = Links.Single(l => l.RelationshipType == "self");
                return link == null ? description.Attribute(rdfns + "about").Value : link.Uri.ToString();
            }
            set{
            }
        }

        public IEnumerable<IOpenSearchResultItem> Items {
            get {
                return items.ToArray();
            }
        }

        public string Title {
            get {
                return rdf.Element(dclite4g + "Series").Element(dc + "title").Value;
            }
            set {

            }
        }

        public DateTime Date {
            get {
                try {
                    return DateTime.Parse(description.Element(dc + "date").Value);
                } catch {
                    return DateTime.UtcNow;
                }
            }
        }

        public string Identifier {
            get {
                try {
                    return description.Attribute(rdfns + "about").Value;
                } catch {
                    return null;
                }
            }
        }

        public long Count {
            get {
                try {
                    return long.Parse(description.Element(os + "totalResults").Value);
                } catch (Exception) {
                    return -1;
                }
            }
        }

        public SyndicationElementExtensionCollection ElementExtensions {
            get {
                return ChildrenElementToSyndicationElementExtensionCollection(doc.Root);
            }
        }

        public Collection<Terradue.ServiceModel.Syndication.SyndicationLink> Links {
            get {
                return new Collection<SyndicationLink>(description.Elements(atom + "link")
                                                   .Select(l => SyndicationLinkFromXElement(l)).ToList());                       
            }
        }

        public string SerializeToString() {

            return doc.ToString();

        }

        public void SerializeToStream(System.IO.Stream stream) {

            doc.Save(stream);
        }

        public IOpenSearchResultCollection DeserializeFromStream(System.IO.Stream stream) {
            throw new NotImplementedException();
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

        public static IOpenSearchResultCollection CreateFromOpenSearchResultCollection(IOpenSearchResultCollection results) {
            if (results == null)
                throw new ArgumentNullException("results");

            RdfXmlDocument rdf = new RdfXmlDocument(results);

            return rdf;
        }

        public string ContentType {
            get {
                return "application/rdf+xml";
            }
        }

        Collection<SyndicationCategory> categories;
        public Collection<SyndicationCategory> Categories {
            get {
                return categories;
            }
        }

        Collection<SyndicationPerson> authors;
        public Collection<SyndicationPerson> Authors {
            get {
                return authors;
            }
            set{ }
        }

        #endregion

        internal static SyndicationLink SyndicationLinkFromXElement(XElement elem) {

            var atom = XNamespace.Get("http://www.w3.org/2005/Atom");

            SyndicationLink link = new SyndicationLink(new Uri(elem.Attribute(atom + "href").Value));
            if (elem.Attribute(atom + "rel") != null)
                link.RelationshipType = elem.Attribute(atom + "rel").Value;
            if (elem.Attribute(atom + "title") != null)
                link.Title = elem.Attribute(atom + "title").Value;
            if (elem.Attribute(atom + "type") != null)
                link.MediaType = elem.Attribute(atom + "type").Value;
            if (elem.Attribute(atom + "length") != null)
                link.Length = long.Parse(elem.Attribute(atom + "length").Value);
            return link;

        }

        internal static SyndicationElementExtensionCollection ChildrenElementToSyndicationElementExtensionCollection(XElement element) {
            var feed = new SyndicationFeed();
            foreach (XElement elem in element.Elements()) {
                feed.ElementExtensions.Add(elem.CreateReader());
            }
            return feed.ElementExtensions;
        }
    }

    public class RdfXmlResult : IOpenSearchResultItem {

        XElement root;

        public RdfXmlResult(IOpenSearchResultItem item) : base() {

            root = new XElement(XName.Get("dataset", "http://xmlns.com/2008/dclite4g#"));

            root.SetAttributeValue(XName.Get("about", "http://www.w3.org/1999/02/22-rdf-syntax-ns#"), item.Id);
            if (!string.IsNullOrEmpty(item.Title))
                root.Add(new XElement(XName.Get("title", "http://purl.org/dc/elements/1.1/"), item.Title));
            if (item.Date.Ticks != 0)
                root.Add(new XElement(XName.Get("date", "http://purl.org/dc/elements/1.1/")), item.Date.ToString("yyyy-MM-ddThh:mm:ss.fZ"));
            if (!string.IsNullOrEmpty(item.Identifier))
                root.Add(new XElement(XName.Get("identifier", "http://purl.org/dc/elements/1.1/"), item.Identifier));
            foreach (SyndicationElementExtension ext in item.ElementExtensions) {
                root.Add(XElement.Load(ext.GetReader()));
            }


        }

        public RdfXmlResult(XElement root) : base() {

            this.root = root;

        }

        public XElement Root {
            get {
                return root;
            }
        }

        #region IOpenSearchResult implementation

        public string Id {
            get {
                return root.Attribute(XName.Get("about", "http://www.w3.org/1999/02/22-rdf-syntax-ns#")).Value;
            }
            set{ }
        }

        public string Title {
            get {
                try {
                    return root.Element(XName.Get("title", "http://purl.org/dc/elements/1.1/")).Value;
                } catch {
                    return null;
                }
            }
        }

        public DateTime Date {
            get {
                try {
                    return DateTime.Parse(root.Element(XName.Get("date", "http://purl.org/dc/elements/1.1/")).Value);
                } catch {
                    return DateTime.UtcNow;
                }
            }
        }

        public string Identifier {
            get {
                return root.Element(XName.Get("identifier", "http://purl.org/dc/elements/1.1/")).Value;
            }
        }

        public SyndicationElementExtensionCollection ElementExtensions {
            get {
                return RdfXmlDocument.ChildrenElementToSyndicationElementExtensionCollection(root);
            }
        }

        public Collection<Terradue.ServiceModel.Syndication.SyndicationLink> Links {
            get {
                IEnumerable<SyndicationLink> col = root.Elements(XName.Get("link", "http://www.w3.org/2005/Atom"))
                    .Select(l => RdfXmlDocument.SyndicationLinkFromXElement(l));
                return new Collection<SyndicationLink>(col.ToList());
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


        readonly Collection<SyndicationCategory> categories;
        public Collection<SyndicationCategory> Categories {
            get {
                return categories;
            }
        }

        readonly Collection<SyndicationPerson> authors;
        public Collection<SyndicationPerson> Authors {
            get {
                return authors;
            }
        }
        #endregion
    }
}

