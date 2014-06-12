//
//  RdfOpenSearchEngineExtension.cs
//
//  Author:
//       Emmanuel Mathot <emmanuel.mathot@terradue.com>
//
//  Copyright (c) 2014 Terradue

using System;
using System.Linq;
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
using Terradue.ServiceModel.Syndication;
using System.Xml.Linq;

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

        public override IOpenSearchResultCollection TransformResponse(OpenSearchResponse response) {
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

            SyndicationLink link = rdfDoc.Links.SingleOrDefault(l => l.RelationshipType == "search");

            if (link == null) return null;
            return new OpenSearchUrl(link.Uri);
        }

        public override Terradue.ServiceModel.Syndication.SyndicationLink[] GetEnclosures(IOpenSearchResult result) {
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

                rdfDoc = RdfXmlDocument.Load(reader);

                rdfDoc.Root.Add(new XElement(XNamespace.Get("http://a9.com/-/spec/opensearch/1.1/") + "queryTime", 
                                        response.RequestTime.Milliseconds.ToString()));
                
            } catch (Exception e) {
                throw new InvalidOperationException("Error during transformation : " + e.Message, e);
            }

            return rdfDoc;
        }
    }
}