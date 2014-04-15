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

namespace Terradue.OpenSearch.Result {

    /// <summary>
    /// Rdf xml document.
    /// </summary>
    public class RdfXmlDocument : XmlDocument, IOpenSearchResultCollection {
        XmlNamespaceManager xnsm;
        XmlElement description;

        /// <summary>
        /// Initializes a new instance of the <see cref="Terradue.OpenSearch.Result.RdfXmlDocument"/> class.
        /// </summary>
        public RdfXmlDocument() : base() {
        }

        /// <summary>
        /// Load the specified reader.
        /// </summary>
        /// <param name="reader">Reader.</param>
        public override void Load(XmlReader reader) {
            base.Load(reader);
            xnsm = new XmlNamespaceManager(this.NameTable);
            xnsm.AddNamespace("rdf", "http://www.w3.org/1999/02/22-rdf-syntax-ns#");
            xnsm.AddNamespace("dclite4g", "http://xmlns.com/2008/dclite4g#");
            xnsm.AddNamespace("dct", "http://purl.org/dc/terms/");
            xnsm.AddNamespace("dc", "http://purl.org/dc/elements/1.1/");

            description = (XmlElement)this.SelectSingleNode("rdf:RDF/rdf:Description", xnsm);
            if (description == null)
                throw new FormatException("RDF document does not contain a description node");

        }

        #region IResultCollection implementation

        public List<string> GetSelfShortList() {

            List<string> about = new List<string>();

            XmlNodeList xmlnl = this.SelectNodes("//rdf:RDF/dclite4g:DataSet", xnsm);

            foreach (XmlNode dataSet in xmlnl) {

                about.Add(dataSet.Attributes["rdf:about"].Value);

            }

            return about;

        }

        public List<IOpenSearchResultItem> Items {
            get {
                List<IOpenSearchResultItem> datasets = new List<IOpenSearchResultItem>();

                XmlNodeList xmlnl = this.SelectNodes("//rdf:RDF/dclite4g:DataSet", xnsm);

                foreach (XmlNode dataSet in xmlnl) {

                    datasets.Add(new RdfXmlResult((XmlElement)dataSet));

                }

                return datasets;
            }
        }

        public string Title {
            get {
                return this.SelectSingleNode("rdf:RDF/dclite4g:Series/dc:title", xnsm).InnerText;
            }
        }

        public DateTime Date {
            get {
                try {
                    return DateTime.Parse(this.SelectSingleNode("rdf:RDF/rdf:Description/dc:date", xnsm).InnerText);
                } catch {
                    return DateTime.UtcNow;
                }
            }
        }

        public string Identifier {
            get {
                try {
                    return description.Attributes.GetNamedItem("about", "http://www.w3.org/1999/02/22-rdf-syntax-ns#").Value;
                } catch {
                    return null;
                }
            }
        }

        public long Count {
            get {
                try {
                    return long.Parse(this.SelectSingleNode("rdf:RDF/rdf:Description/os:totalResults", xnsm).InnerText);
                } catch (Exception) {
                    return -1;
                }
            }
        }

        public XmlNodeList ElementExtensions {
            get {
                return this.SelectNodes("//rdf:RDF/*");
            }
        }

        public List<System.ServiceModel.Syndication.SyndicationLink> Links {
            get {
                List<SyndicationLink> links = new List<SyndicationLink>();
                links.Add(SyndicationLink.CreateSelfLink(new Uri(Identifier)));
                return links;
            }
        }

        public void Serialize(System.IO.Stream stream) {
            throw new NotImplementedException();
        }

        #endregion
    }

    public class RdfXmlResult : IOpenSearchResultItem {

        XmlNamespaceManager xnsm;
        XmlDocument doc;
        XmlNode node;

        public RdfXmlResult(XmlNode element) : base() {
            doc = new XmlDocument();
            node = doc.ImportNode(element, true);
            doc.AppendChild(node);
            xnsm = new XmlNamespaceManager(doc.NameTable);
            xnsm.AddNamespace("rdf", "http://www.w3.org/1999/02/22-rdf-syntax-ns#");
            xnsm.AddNamespace("dclite4g", "http://xmlns.com/2008/dclite4g#");
            xnsm.AddNamespace("dct", "http://purl.org/dc/terms/");
            xnsm.AddNamespace("dc", "http://purl.org/dc/elements/1.1/");
            links = InitLinks();
        }

        #region IOpenSearchResult implementation

        public string Id {
            get {
                return node.SelectSingleNode("@rdf:about", xnsm).Value;
            }
        }

        public string Title {
            get {
                try {
                    return node.SelectSingleNode("dc:title", xnsm).InnerText;
                } catch {
                    return null;
                }
            }
        }

        public DateTime Date {
            get {
                try {
                    return DateTime.Parse(node.SelectSingleNode("dc:date", xnsm).InnerText);
                } catch {
                    return DateTime.UtcNow;
                }
            }
        }

        public string Identifier {
            get {
                return node.SelectSingleNode("dc:identifier", xnsm).InnerText;
            }
        }

        public XmlNodeList ElementExtensions {
            get {
                return doc.ChildNodes;
            }
        }

        List<System.ServiceModel.Syndication.SyndicationLink> links;

        public List<System.ServiceModel.Syndication.SyndicationLink> Links {
            get {
                return links;
            }
        }

        #endregion

        private List<SyndicationLink> InitLinks (){
            List<SyndicationLink> links = new List<SyndicationLink>();
            try {
                links.Add(SyndicationLink.CreateSelfLink(new Uri(node.SelectSingleNode("@rdf:about", xnsm).Value)));
            } catch {
            }
            XmlNodeList onlineRes = node.SelectNodes("dclite4g:onlineResource", xnsm);
            foreach (XmlNode xnode in onlineRes) {
                try {
                    links.Add(new SyndicationLink(new Uri(xnode.FirstChild.Attributes["rdf:about"].Value), "enclosure", xnode.FirstChild.LocalName, "application/x-binary", 0));
                } catch {
                    continue;
                }
            }
            return links;
        }
    }
}

