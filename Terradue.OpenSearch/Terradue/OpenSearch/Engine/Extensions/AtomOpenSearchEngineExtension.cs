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

        public override IOpenSearchResultCollection TransformResponse(OpenSearchResponse response) {
            if (response.ContentType == "application/atom+xml") return TransformAtomResponseToAtomFeed(response);
            if (response.ContentType == "application/xml") return TransformAtomResponseToAtomFeed(response);

            throw new InvalidOperationException("Atom extension does not transform OpenSearch response from " + response.ContentType);
        }

        public override string DiscoveryContentType {
            get {
                return "application/atom+xml";
            }
        }

        public override OpenSearchUrl FindOpenSearchDescriptionUrlFromResponse(OpenSearchResponse response) {
            AtomFeed feed = TransformAtomResponseToAtomFeed(response);
            SyndicationLink link = feed.Links.FirstOrDefault(l => l.RelationshipType == "search" && l.MediaType.Contains("opensearch"));
            if (link == null) return null;
            return new OpenSearchUrl(link.Uri);
        }

        public override SyndicationLink[] GetEnclosures(IOpenSearchResult results) {

            List<SyndicationLink> links = new List<SyndicationLink>();

            foreach (SyndicationItem item in ((SyndicationFeed)results.Result).Items) {
                foreach (SyndicationLink link in item.Links) {
                    if (link.RelationshipType == "enclosure") {
                        links.Add(link);
                    }
                }
            }

            return links.ToArray();
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
        public static AtomFeed TransformAtomResponseToAtomFeed(OpenSearchResponse response) {

            XmlReader reader;
            AtomFeed result;

            try {

                reader = XmlReader.Create(response.GetResponseStream());

                result = new AtomFeed(AtomFeed.Load(reader));
                result.LastUpdatedTime = DateTime.UtcNow;
                result.ElementExtensions.Add("queryTime", "http://a9.com/-/spec/opensearch/1.1/", response.RequestTime.TotalMilliseconds.ToString());

            } catch (Exception e) {
                throw new InvalidOperationException("Error during transformation : " + e.Message, e);
            }

            return result;

        }
        //---------------------------------------------------------------------------------------------------------------------
        public static void ReplaceSelfLinks(IOpenSearchResult osr, Func<SyndicationItem,OpenSearchDescription,string,string> entryTemplate) {
            SyndicationFeed feed = (SyndicationFeed)osr.Result;

            var matchLinks = feed.Links.Where(l => l.RelationshipType == "self").ToArray();
            foreach (var link in matchLinks) {
                feed.Links.Remove(link);
            }

            IProxiedOpenSearchable entity = (IProxiedOpenSearchable)osr.OpenSearchableEntity;
            OpenSearchDescription osd = entity.GetProxyOpenSearchDescription();
            UriBuilder myUrl = new UriBuilder(OpenSearchFactory.GetOpenSearchUrlByType(osd, "application/atom+xml").Template);
            string[] queryString = Array.ConvertAll(osr.SearchParameters.AllKeys, key => string.Format("{0}={1}", key, osr.SearchParameters[key]));
            myUrl.Query = string.Join("&", queryString);

            feed.Links.Add(new SyndicationLink(myUrl.Uri, "self", "Reference link", "application/atom+xml", 0));

            foreach (SyndicationItem item in feed.Items) {
                string template = entryTemplate(item, osd, "application/atom+xml");
                if (template != null) {
                    item.Links.Add(new SyndicationLink(new Uri(template), "self", "Reference link", "application/atom+xml", 0));
                }
            }
        }

        public static void ReplaceOpenSearchDescriptionLinks(IOpenSearchResult osr) {
            SyndicationFeed feed = (SyndicationFeed)osr.Result;

            var matchLinks = feed.Links.Where(l => l.RelationshipType == "search").ToArray();
            foreach (var link in matchLinks) {
                feed.Links.Remove(link);
            }

            OpenSearchDescription osd;
            if (osr.OpenSearchableEntity is IProxiedOpenSearchable) osd = ((IProxiedOpenSearchable)osr.OpenSearchableEntity).GetProxyOpenSearchDescription();
            else osd = osr.OpenSearchableEntity.GetOpenSearchDescription();
            OpenSearchDescriptionUrl url = OpenSearchFactory.GetOpenSearchUrlByRel(osd, "self");
            if (url != null) feed.Links.Add(new SyndicationLink(new Uri(url.Template), "search", "OpenSearch Description link", "application/opensearchdescription+xml", 0));

            foreach (SyndicationItem item in feed.Items) {
                matchLinks = item.Links.Where(l => l.RelationshipType == "search").ToArray();
                foreach (var link in matchLinks) {
                    item.Links.Remove(link);
                }
                if (url != null) item.Links.Add(new SyndicationLink(new Uri(url.Template), "search", "OpenSearch Description link", "application/opensearchdescription+xml", 0));
            }

        }

        public static void ReplaceIdentifier(IOpenSearchResult osr, Func<SyndicationItem,OpenSearchDescription,string,string> entryTemplate) {

            SyndicationFeed feed = (SyndicationFeed)osr.Result;

            IProxiedOpenSearchable entity = (IProxiedOpenSearchable)osr.OpenSearchableEntity;
            OpenSearchDescription osd = entity.GetProxyOpenSearchDescription();
            UriBuilder myUrl = new UriBuilder(OpenSearchFactory.GetOpenSearchUrlByType(osd, "application/json").Template);
            string[] queryString = Array.ConvertAll(osr.SearchParameters.AllKeys, key => string.Format("{0}={1}", key, osr.SearchParameters[key]));
            myUrl.Query = string.Join("&", queryString);

            feed.Id = myUrl.ToString();

            foreach (SyndicationItem item in feed.Items) {
                string template = entryTemplate(item, osd, "application/json");
                if (template != null) {
                    item.Id = template;
                }

            }

        }
    }
}

