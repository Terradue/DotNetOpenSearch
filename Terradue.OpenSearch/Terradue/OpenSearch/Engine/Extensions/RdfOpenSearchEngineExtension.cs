//
//  RdfOpenSearchEngineExtension.cs
//
//  Author:
//       Emmanuel Mathot <emmanuel.mathot@terradue.com>
//
//  Copyright (c) 2014 Terradue

using System;
using Mono.Addins;
using System.Collections.Generic;
using System.Net;
using System.Web;
using System.Xml;
using System.Diagnostics;
using Terradue.OpenSearch.Result;
using System.Text.RegularExpressions;
using System.Collections.Specialized;
using Terradue.OpenSearch.Response;

namespace Terradue.OpenSearch.Engine.Extensions {
    /// <summary>
    /// Rdf open search engine extension.
    /// </summary>
    /// <description>
    /// Extension that allows to query and transform Rdf OpenSearchable source to Rdf XML document (Atom).
    /// </description>
    [Extension(typeof(IOpenSearchEngineExtension))]
    [ExtensionNode("RDF", "RDF native query")]
    public class RdfOpenSearchEngineExtension : OpenSearchEngineExtension<RdfXmlDocument> {
        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="Terradue.OpenSearch.EngineExtensions.RdfOpenSearchEngineExtension"/> class.
        /// </summary>
        public RdfOpenSearchEngineExtension() {
        }

        #region OpenSearchEngineExtension implementation

        public override string Identifier {
            get {
                return "Rdf";
            }
        }

        public override string Name {
            get {
                return "RDF Xml Document";
            }
        }

        public override string[] GetInputFormatTransformPath() {
            return new string[] { "application/rdf+xml" };
        }

        public override object TransformResponse(OpenSearchResponse response) {
            if (response.ContentType == "application/rdf+xml") return TransformRdfResponseToRdfXmlDocument(response);

            throw new NotSupportedException("RDF extension does not transform OpenSearch response from " + response.ContentType);
        }

        public override string DiscoveryContentType {
            get {
                return "application/rdf+xml";
            }
        }

        public override OpenSearchUrl FindOpenSearchDescriptionUrlFromResponse(OpenSearchResponse response) {
            RdfXmlDocument rdfDoc = TransformRdfResponseToRdfXmlDocument(response);

            XmlNamespaceManager xnsm = new XmlNamespaceManager(rdfDoc.NameTable);
            xnsm.AddNamespace("rdf", "http://www.w3.org/1999/02/22-rdf-syntax-ns#");
            xnsm.AddNamespace("atom", "http://www.w3.org/2005/Atom");
            XmlNode link = rdfDoc.SelectSingleNode("rdf:RDF/rdf:Description/atom:link[@atom:rel='search']", xnsm);

            if (link == null) return null;
            return new OpenSearchUrl(link.Attributes.GetNamedItem("href", "http://www.w3.org/2005/Atom").InnerText);
        }

        public override System.ServiceModel.Syndication.SyndicationLink[] GetEnclosures(IOpenSearchResult result) {
            throw new NotImplementedException();
        }

        #endregion

        /// <summary>
        /// Transforms the rdf response to rdf xml document.
        /// </summary>
        /// <returns>The rdf response to rdf xml document.</returns>
        /// <param name="response">Response.</param>
        public static RdfXmlDocument TransformRdfResponseToRdfXmlDocument(OpenSearchResponse response) {
            RdfXmlDocument rdfDoc;
			
            try {

                XmlReader reader = XmlReader.Create(response.GetResponseStream());

                rdfDoc = new RdfXmlDocument();
                rdfDoc.Load(reader);

                XmlElement elem = rdfDoc.CreateElement("queryTime", "http://a9.com/-/spec/opensearch/1.1/");
                elem.InnerText = response.RequestTime.Milliseconds.ToString();
                rdfDoc.DocumentElement.AppendChild(elem);

            } catch (Exception e) {
                throw new InvalidOperationException("Error during transformation : " + e.Message, e);
            }

            RdfOpenSearchEngineExtension.CorrectRdfAbout(rdfDoc);

            return rdfDoc;
        }

        /// <summary>
        /// Corrects the rdf about.
        /// </summary>
        /// <param name="rdfDoc">Rdf document.</param>
        public static void CorrectRdfAbout(RdfXmlDocument rdfDoc) {

            XmlNamespaceManager xnsm = new XmlNamespaceManager(rdfDoc.NameTable);
            xnsm.AddNamespace("dclite4g", "http://xmlns.com/2008/dclite4g#");
            xnsm.AddNamespace("dct", "http://purl.org/dc/terms/");
            xnsm.AddNamespace("rdf", "http://www.w3.org/1999/02/22-rdf-syntax-ns#");
            xnsm.AddNamespace("dc", "http://purl.org/dc/elements/1.1/");
            xnsm.AddNamespace("os", "http://a9.com/-/spec/opensearch/1.1/");
            XmlNodeList xmlnl = rdfDoc.SelectNodes("rdf:RDF/dclite4g:DataSet", xnsm);

            foreach (XmlNode dataSet in xmlnl) {

                UriBuilder url;
                try {
                    url = new UriBuilder(dataSet.Attributes.GetNamedItem("about", "http://www.w3.org/1999/02/22-rdf-syntax-ns#").Value);
                } catch (UriFormatException) {
                    continue;
                }

                Match match = Regex.Match(url.Path, @"(.*)/(.*)/rdf");

                if (match.Success) {

                    url.Path = string.Format("{0}/rdf", match.Groups[1].Value);
                    NameValueCollection nvc = HttpUtility.ParseQueryString(url.Query);
                    nvc.Add("uid", match.Groups[2].Value);
                    url.Query = nvc.ToString();
                    XmlAttribute attr = rdfDoc.CreateAttribute("rdf:about", "http://www.w3.org/1999/02/22-rdf-syntax-ns#");
                    attr.Value = url.ToString();
                    dataSet.Attributes.SetNamedItem(attr);
                }
            }
        }
    }
}