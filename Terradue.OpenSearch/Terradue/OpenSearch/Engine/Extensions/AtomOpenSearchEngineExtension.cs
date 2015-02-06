//
//  AtomOpenSearchEngineExtension.cs
//
//  Author:
//       Emmanuel Mathot <emmanuel.mathot@terradue.com>
//
//  Copyright (c) 2014 Terradue

using System;
using System.Linq;
using Mono.Addins;
using Terradue.ServiceModel.Syndication;
using System.Collections.Generic;
using System.Xml;
using System.Net;
using System.Diagnostics;
using System.Collections.Specialized;
using Terradue.OpenSearch.Response;
using Terradue.OpenSearch.Result;
using Terradue.OpenSearch.Schema;
using System.IO;

namespace Terradue.OpenSearch.Engine.Extensions {
    /// <summary>
    /// Atom open search engine extension.
    /// </summary>
    [Extension(typeof(IOpenSearchEngineExtension))]
    [ExtensionNode("Atom", "Atom native query")]
    public class AtomOpenSearchEngineExtension : OpenSearchEngineExtension<AtomFeed> {
        public AtomOpenSearchEngineExtension() {
        }

        #region implemented abstract members of OpenSearchEngineExtension

        public override string Identifier {
            get {
                return "atom";
            }
        }

        public override string Name {
            get {
                return "ATOM Syndication Feed";
            }
        }

        public override IOpenSearchResultCollection ReadNative(IOpenSearchResponse response) {

            if (response.ObjectType == typeof(byte[])) {
                if (response.ContentType == "application/atom+xml" || response.ContentType == "application/xml")
                    return TransformAtomResponseToAtomFeed((OpenSearchResponse<byte[]>)response);
                throw new InvalidOperationException("Atom extension does not transform OpenSearch response with content type " + response.ContentType);
            }
            if (response.ObjectType == typeof(AtomFeed)) {
                return (AtomFeed)response.GetResponseObject();
            }
            throw new InvalidOperationException("Atom extension does not transform OpenSearch response of type " + response.ObjectType);
        }

        public override string DiscoveryContentType {
            get {
                return "application/atom+xml";
            }
        }

        public override OpenSearchUrl FindOpenSearchDescriptionUrlFromResponse(IOpenSearchResponse response) {
            AtomFeed feed = (AtomFeed)ReadNative(response);
            SyndicationLink link = feed.Links.FirstOrDefault(l => l.RelationshipType == "search" && l.MediaType.Contains("opensearch"));
            if (link == null)
                return null;
            return new OpenSearchUrl(link.Uri);
        }

        public override IOpenSearchResultCollection CreateOpenSearchResultFromOpenSearchResult(IOpenSearchResultCollection results) {
            if (results is AtomFeed)
                return results;

            return AtomFeed.CreateFromOpenSearchResultCollection(results);
        }

        #endregion

        //---------------------------------------------------------------------------------------------------------------------
        /// <summary>Queries the products of a series using the specified search parameters and returns it as an Atom feed</summary>
        /// <param name="context">the execution environment context</param>
        /// <param name="urlTemplate">the urlTemplate to be queried</param>
        /// <param name="searchParameters">a dictionary of key/value pairs for the OpenSearch parameters to be used in the query</param>
        /// <returns>an <c>XmlDocument</c> containing the result</returns>
        /// <remarks>The match between URL template parameters and search parameters is based on the URL query parameter name (i.e. <c>bbox={geo:box}</c> will be replaced with the value of the <c>bbox</c> item in the <c>searchParameters</c> dictionary if it contains such an item.</remarks>
        public static AtomFeed TransformAtomResponseToAtomFeed(OpenSearchResponse<byte[]> response) {

            XmlReader reader;
            AtomFeed result;

            try {

                reader = XmlReader.Create(new MemoryStream((byte[])response.GetResponseObject()));

                result = AtomFeed.Load(reader);
                result.LastUpdatedTime = DateTime.UtcNow;
                result.ElementExtensions.Add("queryTime", "http://a9.com/-/spec/opensearch/1.1/", response.RequestTime.TotalMilliseconds.ToString());

            } catch (Exception e) {
                throw new InvalidOperationException("Error during transformation : " + e.Message, e);
            }

            return result;

        }
        //---------------------------------------------------------------------------------------------------------------------

    }
}

