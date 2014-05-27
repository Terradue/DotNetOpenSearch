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
using System.ServiceModel.Syndication;
using System.Xml.Serialization;
using System.Xml.Linq;

namespace Terradue.OpenSearch.Result {
    /// <summary>
    /// Rdf xml document.
    /// </summary>
    public class RdfXmlDocument : XDocument, IOpenSearchResultCollection {
        Dictionary<string, XNamespace> xnsm;
        XElement rdf,series,description;
        XNamespace rdfns, dclite4g, dc, os;

        /// <summary>
        /// Initializes a new instance of the <see cref="Terradue.OpenSearch.Result.RdfXmlDocument"/> class.
        /// </summary>
        internal RdfXmlDocument(XDocument doc) : base(doc) {
            xnsm = doc.Root.Attributes().
                Where(a => a.IsNamespaceDeclaration).
                GroupBy(a => a.Name.Namespace == XNamespace.None ? String.Empty : a.Name.LocalName,
                        a => XNamespace.Get(a.Value)).
                ToDictionary(g => g.Key, 
                             g => g.First());

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
            series = rdf.Element( dclite4g + "Series");

        }

        /// <summary>
        /// Load the specified reader.
        /// </summary>
        /// <param name="reader">Reader.</param>
        public new static RdfXmlDocument Load(XmlReader reader) {
            return new RdfXmlDocument(XDocument.Load(reader));
        }

        #region IResultCollection implementation

        public List<IOpenSearchResultItem> Items {
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
                        return rdf.Element(dclite4g + "Series").Element(dc + "title").ToString();
            }
        }

        public DateTime Date {
            get {
                try {
                            return DateTime.Parse(description.Element(dc + "date").ToString());
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
                            return long.Parse(description.Element(os + "totalResults").ToString());
                } catch (Exception) {
                    return -1;
                }
            }
        }

        public SyndicationElementExtensionCollection ElementExtensions {
            get {
                var feed = new SyndicationFeed();
                foreach (XElement elem in series.Descendants()) {
                    feed.ElementExtensions.Add(elem);
                }
                return feed.ElementExtensions;
            }
        }

        public List<System.ServiceModel.Syndication.SyndicationLink> Links {
            get {
                return description.Elements("http://www.w3.org/2005/Atom" + "link")
                    .Select(l => new SyndicationLink(new Uri(l.Attribute("href").Value), 
                                                     l.Attribute("rel").Value, 
                                                     l.Attribute("title").Value, 
                                                     l.Attribute("type").Value, 
                                                     long.Parse(l.Attribute("length").Value)))
                    .ToList();
            }
        }

        public void Serialize(System.IO.Stream stream) {
            throw new NotImplementedException();
        }

        #endregion
    }

    public class RdfXmlResult : IOpenSearchResultItem {
        Dictionary<string, XNamespace> xnsm;
        XElement root;

        public RdfXmlResult(XElement root) : base() {

            this.root = root;
            xnsm = root.Attributes().
            Where(a => a.IsNamespaceDeclaration).
            GroupBy(a => a.Name.Namespace == XNamespace.None ? String.Empty : a.Name.LocalName,
                    a => XNamespace.Get(a.Value)).
            ToDictionary(g => g.Key, 
                         g => g.First());

            links = InitLinks();
        }

        #region IOpenSearchResult implementation

        public string Id {
            get {
                return root.Attribute("about").Value;
            }
        }

        public string Title {
            get {
                try {
                    return root.Element("http://purl.org/dc/elements/1.1/" + "title").ToString();
                } catch {
                    return null;
                }
            }
        }

        public DateTime Date {
            get {
                try {
                    return DateTime.Parse(root.Element("http://purl.org/dc/elements/1.1/" + "date").ToString());
                } catch {
                    return DateTime.UtcNow;
                }
            }
        }

        public string Identifier {
            get {
                return root.Element("http://purl.org/dc/elements/1.1/" + "identifier").ToString();
            }
        }

        public SyndicationElementExtensionCollection ElementExtensions {
            get {
                var feed = new SyndicationFeed();
                foreach (XElement elem in root.Descendants()) {
                    feed.ElementExtensions.Add(elem);
                }
                return feed.ElementExtensions;
            }
        }

        List<System.ServiceModel.Syndication.SyndicationLink> links;

        public List<System.ServiceModel.Syndication.SyndicationLink> Links {
            get {
                return links;
            }
        }

        #endregion

        private List<SyndicationLink> InitLinks() {
            List<SyndicationLink> links = new List<SyndicationLink>();
            try {
                links.Add(SyndicationLink.CreateSelfLink(new Uri(root.Attribute("about").Value)));
            } catch {
            }
            foreach (XElement elem in root.Elements("http://xmlns.com/2008/dclite4g#" + "dclite4g:onlineResource") ) {
                try {
                    links.Add(new SyndicationLink(new Uri(elem.Elements().First().Attribute("about").Value), "enclosure", elem.Elements().First().Name.LocalName, "application/x-binary", 0));
                } catch {
                    continue;
                }
            }
            return links;
        }
    }
}

