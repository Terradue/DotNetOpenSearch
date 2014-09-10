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

        public static XNamespace rdfns = XNamespace.Get("http://www.w3.org/1999/02/22-rdf-syntax-ns#"), 
            dclite4gns = XNamespace.Get("http://xmlns.com/2008/dclite4g#"),
            dcns = XNamespace.Get("http://purl.org/dc/elements/1.1/"),
            osns = XNamespace.Get("http://a9.com/-/spec/opensearch/1.1/"),
            atomns = XNamespace.Get("http://www.w3.org/2005/Atom");

        XDocument doc;

        List<RdfXmlResult> items;

        /// <summary>
        /// Initializes a new instance of the <see cref="Terradue.OpenSearch.Result.RdfXmlDocument"/> class.
        /// </summary>
        internal RdfXmlDocument(XDocument doc) : base(doc) {

            this.doc = doc;
            rdf = this.Element(rdfns + "RDF");
            if (rdf == null)
                throw new FormatException("Not a RDF document");
            description = rdf.Element(rdfns + "Description");
            if (description == null) {
                description = new XElement(XName.Get("Description", rdfns.NamespaceName));
                rdf.Add(description);
            }
            series = rdf.Element(dclite4gns + "Series");

            items = LoadItems(rdf);

            AlignNameSpaces(rdf);

        }

        public RdfXmlDocument(IOpenSearchResultCollection results) : base() {

            doc = new XDocument();
            rdf = new XElement(XName.Get("RDF", RdfXmlDocument.rdfns.NamespaceName));
            doc.Add(rdf);
           
            description = new XElement(XName.Get("Description", RdfXmlDocument.rdfns.NamespaceName));

            Title = results.Title;
            Identifier = results.Identifier;
            Id = results.Id;

            if (results.ElementExtensions != null) {
                foreach (SyndicationElementExtension ext in results.ElementExtensions) {
                    using (XmlReader xr = ext.GetReader()) {
                        XElement xEl = XElement.ReadFrom(xr) as XElement;
                        rdf.Add(xEl);
                    }
                }
            }

            if (results.Date.Ticks > 0)
                description.Add(new XElement(XName.Get("date", RdfXmlDocument.dcns.NamespaceName)), results.Date.ToString("yyyy-MM-ddThh:mm:ss.fZ"));

            if (results.Items != null) {
                List<RdfXmlResult> items = new List<RdfXmlResult>();
                foreach (var item in results.Items) {
                    var newItem = new RdfXmlResult(item);
                    items.Add(newItem);
                    rdf.Add(newItem.Root);
                }
                this.items = items;
            }

            AlignNameSpaces(rdf);
        }

        /// <summary>
        /// Load the specified reader.
        /// </summary>
        /// <param name="reader">Reader.</param>
        public new static RdfXmlDocument Load(XmlReader reader) {
            return new RdfXmlDocument(XDocument.Load(reader));
        }

        void AlignNameSpaces(XElement rdf) {
            rdf.SetAttributeValue(XName.Get("rdf", XNamespace.Xmlns.NamespaceName), "http://www.w3.org/1999/02/22-rdf-syntax-ns#");
            rdf.SetAttributeValue(XName.Get("dclite4g", XNamespace.Xmlns.NamespaceName), "http://xmlns.com/2008/dclite4g#");
            rdf.SetAttributeValue(XName.Get("atom", XNamespace.Xmlns.NamespaceName), "http://www.w3.org/2005/Atom");
            rdf.SetAttributeValue(XName.Get("os", XNamespace.Xmlns.NamespaceName), "http://a9.com/-/spec/opensearch/1.1/");
            rdf.SetAttributeValue(XName.Get("time", XNamespace.Xmlns.NamespaceName), "http://a9.com/-/opensearch/extensions/time/1.0/");
            rdf.SetAttributeValue(XName.Get("dc", XNamespace.Xmlns.NamespaceName), "http://purl.org/dc/elements/1.1/");
            rdf.SetAttributeValue(XName.Get("dct", XNamespace.Xmlns.NamespaceName), "http://purl.org/dc/terms/");
            rdf.SetAttributeValue(XName.Get("dc", XNamespace.Xmlns.NamespaceName), "http://purl.org/dc/elements/1.1/");
        }

        public XmlNameTable NameTable {
            get {
                return doc.CreateReader().NameTable;
            }
        }

        public static List<RdfXmlResult> LoadItems(XElement rdf) {

            List<RdfXmlResult> items = new List<RdfXmlResult>();

            foreach (XElement dataSet in rdf.Elements(XName.Get("DataSet",RdfXmlDocument.dclite4gns.NamespaceName))) {

                items.Add(new RdfXmlResult(dataSet));

            }

            return items;
        }

        #region IResultCollection implementation

        public string Id {
            get {
                return description != null ? description.Attribute(rdfns + "about").Value : Identifier;
            }
            set {
            }
        }

        public IEnumerable<IOpenSearchResultItem> Items {
            get {
                return items.Cast<IOpenSearchResultItem>();
            }
            set {
                items = value.Cast<RdfXmlResult>().ToList();
            }
        }

        public string Title {
            get {
                return rdf.Element(dclite4gns + "Series").Element(dcns + "title").Value;
            }
            set {
                try {
                    rdf.Elements(XName.Get("title", dcns.NamespaceName)).Remove();
                } catch {
                }
                rdf.Add(new XElement(XName.Get("title", dcns.NamespaceName)), value);
            }
        }

        public DateTime Date {
            get {
                try {
                    return DateTime.Parse(description.Element(dcns + "date").Value);
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
            set {
                description.SetAttributeValue(rdfns + "about", value);
            }
        }

        public long Count {
            get {
                try {
                    return long.Parse(description.Element(osns + "totalResults").Value);
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
                return new Collection<SyndicationLink>(description.Elements(atomns + "link")
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

}

