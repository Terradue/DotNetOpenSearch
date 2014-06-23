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

namespace Terradue.OpenSearch.Result {
    /// <summary>
    /// Rdf xml document.
    /// </summary>
    public class RdfXmlDocument : XDocument, IOpenSearchResultCollection {
        XElement rdf, series, description;
        XNamespace rdfns, dclite4g, dc, os, atom;

        /// <summary>
        /// Initializes a new instance of the <see cref="Terradue.OpenSearch.Result.RdfXmlDocument"/> class.
        /// </summary>
        internal RdfXmlDocument(XDocument doc) : base(doc) {

            rdfns = ((XElement)this.FirstNode).Name.Namespace;
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

        }

        /// <summary>
        /// Load the specified reader.
        /// </summary>
        /// <param name="reader">Reader.</param>
        public new static RdfXmlDocument Load(XmlReader reader) {
            return new RdfXmlDocument(XDocument.Load(reader));
        }

        #region IResultCollection implementation

        public IEnumerable<IOpenSearchResultItem> Items {
            get {
                List<IOpenSearchResultItem> datasets = new List<IOpenSearchResultItem>();

                foreach (XElement dataSet in rdf.Elements(dclite4g + "DataSet")) {

                    datasets.Add(new RdfXmlResult(dataSet));

                }

                return datasets;
            }
        }

        public string Title {
            get {
                return rdf.Element(dclite4g + "Series").Element(dc + "title").Value;
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

                var feed = new SyndicationFeed();
                foreach (XElement elem in series.Elements()) {
                    feed.ElementExtensions.Add(elem.CreateReader());
                }
                return feed.ElementExtensions;
            }
        }

        public Collection<Terradue.ServiceModel.Syndication.SyndicationLink> Links {
            get {

                return new Collection<SyndicationLink>(description.Elements(atom + "link")
                                                       .Select(l => SyndicationLinkFromXElement(l)).ToList());
                                                     
            }
        }

        public string SerializeToString() {
            throw new NotImplementedException();
        }

        public void SerializeToStream(System.IO.Stream stream) {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        public string ContentType {
            get {
                return "application/rdf+xml";
            }
        }

        #endregion

        internal SyndicationLink SyndicationLinkFromXElement(XElement elem) {

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
    }

    public class RdfXmlResult : IOpenSearchResultItem {
        XElement root;

        public RdfXmlResult(XElement root) : base() {

            this.root = root;
            links = InitLinks();
        }

        #region IOpenSearchResult implementation

        public string Id {
            get {
                return root.Attribute(XName.Get("about", "http://www.w3.org/1999/02/22-rdf-syntax-ns#")).Value;
            }
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
                var feed = new SyndicationFeed();
                foreach (XElement elem in root.Elements()) {
                    feed.ElementExtensions.Add(elem.CreateReader());
                }
                return feed.ElementExtensions;
            }
        }

        Collection<Terradue.ServiceModel.Syndication.SyndicationLink> links;

        public Collection<Terradue.ServiceModel.Syndication.SyndicationLink> Links {
            get {
                return links;
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

        private Collection<SyndicationLink> InitLinks() {
            List<SyndicationLink> links = new List<SyndicationLink>();
            try {
                links.Add(SyndicationLink.CreateSelfLink(new Uri(Id)));
            } catch {
            }
            foreach (XElement elem in root.Elements(XName.Get("onlineResource", "http://xmlns.com/2008/dclite4g#"))) {
                try {
                    links.Add(new SyndicationLink(new Uri(elem.Elements().First().Attribute("about").Value), "enclosure", elem.Elements().First().Name.LocalName, "application/x-binary", 0));
                } catch {
                    continue;
                }
            }
            return new Collection<SyndicationLink>(links);
        }
    }
}

