//
//  OpenSearchEngine.cs
//
//  Author:
//       Emmanuel Mathot <emmanuel.mathot@terradue.com>
//
//  Copyright (c) 2014 Terradue

using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections;
using System.Collections.Specialized;
using System.Net;
using System.Web;
using System.Diagnostics;
using Mono.Addins;
using Terradue.ServiceModel.Syndication;
using System.Xml.Serialization;
using System.Xml;
using Terradue.OpenSearch.Request;
using Terradue.OpenSearch.Response;
using Terradue.OpenSearch.Result;
using Terradue.OpenSearch.Schema;
using System.Xml.Linq;

namespace Terradue.OpenSearch.Engine {

    /// <summary>
    /// The engine for making OpenSearch request
    /// </summary>
    public sealed partial class OpenSearchEngine : IOpenSearchableFactory {

        internal const int DEFAULT_COUNT = 20;


        Dictionary<int, IOpenSearchEngineExtension> extensions;
        List<PreFilterAction> preFilters;
        List<PostFilterAction> postFilters;

        /// <summary>
        /// Initializes a new instance of the <see cref="Terradue.OpenSearch.OpenSearchEngine"/> class.
        /// </summary>
        public OpenSearchEngine() {
            extensions = new Dictionary<int,IOpenSearchEngineExtension>();
            preFilters = new List<PreFilterAction>();
            postFilters = new List<PostFilterAction>();
            DefaultCount = DEFAULT_COUNT;
            DefaultTimeOut = 10000;
        }

        public delegate void PreFilterAction(ref OpenSearchRequest request);

        public delegate void PostFilterAction(OpenSearchRequest request, ref OpenSearchResponse response);

        /// <summary>
        /// Gets or sets the default time out.
        /// </summary>
        /// <value>The default time out.</value>
        public int DefaultTimeOut {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the default count.
        /// </summary>
        /// <value>The default count.</value>
        public int DefaultCount {
            get;
            set;
        }

        /// <summary>
        /// Registers an extension for a specific type.
        /// </summary>
        /// <param name="type">Type to be registered</param>
        /// <param name="extension">Extension associated with the type</param>
        /// <exception cref="ArgumentException">An extension with the same typoe already registered.</exception>
        public void RegisterExtension(IOpenSearchEngineExtension extension) {
            extensions.Add(extension.GetHashCode(), extension);
        }

        /// <summary>
        /// Registers a pre search filter.
        /// </summary>
        /// <param name="filter">Filter.</param>
        public void RegisterPreSearchFilter(PreFilterAction filter) {
            preFilters.Add(filter);
        }

        /// <summary>
        /// Registers a post search filter.
        /// </summary>
        /// <param name="filter">Filter.</param>
        public void RegisterPostSearchFilter(PostFilterAction filter) {
            postFilters.Add(filter);
        }

        /// <summary>
        /// Query the specified IOpenSearchable entity with specific parameters and result based on the extension's result name.
        /// </summary>
        /// <param name="entity">Entity.</param>
        /// <param name="parameters">Parameters.</param>
        /// <param name="resultName">Result name.</param>
        public IOpenSearchResult Query(IOpenSearchable entity, NameValueCollection parameters, string resultName) {

            // Transform action to invoke
            QuerySettings querySettings;
            // Results
            IOpenSearchResult osr = null;

            // 1) Find the plugin to match with urlTemplates
            querySettings = entity.GetQuerySettings(this);
            if (querySettings == null) throw new ImpossibleSearchException(String.Format("No engine extension to query {0}", entity.Identifier));

            // 2) Create the request
            OpenSearchRequest request = entity.Create(querySettings.PreferredContentType, parameters);

            // 5) Apply the pre-search functions
            ApplyPreSearchFilters(ref request);

            // 6) Perform the Search
            OpenSearchResponse response = request.GetResponse();
            response.Entity = entity;

            // 5) Apply the pre-search functions
            ApplyPostSearchFilters(request, ref response);

            // 7) Transform the response
            IOpenSearchResultCollection results = querySettings.ReadNative.Invoke(response);

            // 8) Change format
            IOpenSearchEngineExtension osee = GetExtensionByExtensionName(resultName);
            IOpenSearchResultCollection newResults = osee.CreateOpenSearchResultFromOpenSearchResult(results);

            // 9) Create Result
            osr = CreateOpenSearchResult(newResults, request, response);

            // 8) Assign the original entity to the IOpenSearchResult
            osr.OpenSearchableEntity = entity;

            // 9) Apply post search filters
            entity.ApplyResultFilters(ref osr);

            return osr;

        }

        /// <summary>
        /// Gets the name of the type by extension's result name
        /// </summary>
        /// <returns>The type by extension name.</returns>
        /// <param name="resultName">Result name.</param>
        public Type GetTypeByExtensionName(string resultName) {
            foreach (IOpenSearchEngineExtension extension in extensions.Values) {
                if (extension.Identifier == resultName)
                    return  extension.GetTransformType();
            }
            throw new KeyNotFoundException(string.Format("Engine extension to transform to {0} not found", resultName));
        }

        public IOpenSearchEngineExtension GetExtensionByExtensionName(string resultName) {
            foreach (IOpenSearchEngineExtension extension in extensions.Values) {
                if (extension.Identifier == resultName)
                    return  extension;
            }
            throw new KeyNotFoundException(string.Format("Engine extension to transform to {0} not found", resultName));
        }

        /// <summary>
        /// Query the specified IOpenSearchable entity with specific parameters and result with the specific result type.
        /// </summary>
        /// <param name="entity">Entity.</param>
        /// <param name="parameters">Parameters.</param>
        /// <param name="resultType">Result type.</param>
        public IOpenSearchResult Query(IOpenSearchable entity, NameValueCollection parameters, Type resultType) {

            // Transform action to invoke
            QuerySettings querySettings;

            // Results
            IOpenSearchResult osr = null;

            // 1) Find the best match between urlTemplates and result format
            querySettings = entity.GetQuerySettings(this);
            if (querySettings == null) throw new ImpossibleSearchException(String.Format("No engine extension to query {0} in order to return {1}", entity.Identifier, resultType.FullName));

            // 2) Create the request
            OpenSearchRequest request = entity.Create(querySettings.PreferredContentType, parameters);

            // 5) Apply the pre-search functions
            ApplyPreSearchFilters(ref request);

            // 6) Perform the Search
            OpenSearchResponse response = request.GetResponse();
            response.Entity = entity;

            // 5) Apply the pre-search functions
            ApplyPostSearchFilters(request, ref response);

            // 7) Transform the response
            IOpenSearchResultCollection results = querySettings.ReadNative.Invoke(response);

            // 8) Change format
            IOpenSearchEngineExtension osee = GetFirstExtensionByTypeAbility(resultType);
            IOpenSearchResultCollection newResults = osee.CreateOpenSearchResultFromOpenSearchResult(results);

            // 9) Create Result container
            osr = CreateOpenSearchResult(newResults, request, response);

            // 10) Assign the original entity to the IOpenSearchResult
            osr.OpenSearchableEntity = entity;

            // 11) Apply post search filters
            entity.ApplyResultFilters(ref osr);

            return osr;
        }

        /// <summary>
        /// Query the specified IOpenSearchable entity with specific parameters and returns result in native format.
        /// </summary>
        /// <param name="entity">Entity.</param>
        /// <param name="parameters">Parameters.</param>
        public IOpenSearchResult Query(IOpenSearchable entity, NameValueCollection parameters) {

            // Transform action to invoke
            QuerySettings querySettings;
            // Results
            IOpenSearchResult osr = null;

            // 1) Find the plugin to match with urlTemplates
            querySettings = entity.GetQuerySettings(this);
            if (querySettings == null) throw new ImpossibleSearchException(String.Format("No engine extension to query {0}", entity.Identifier));

            // 2) Create the request
            OpenSearchRequest request = entity.Create(querySettings.PreferredContentType, parameters);

            // 5) Apply the pre-search functions
            ApplyPreSearchFilters(ref request);

            // 6) Perform the Search
            OpenSearchResponse response = request.GetResponse();
            response.Entity = entity;

            // 5) Apply the pre-search functions
            ApplyPostSearchFilters(request, ref response);

            // 7) Transform the response    
            osr = CreateOpenSearchResult(querySettings.ReadNative.Invoke(response), request, response);

            // 8) Assign the original entity to the IOpenSearchResult
            osr.OpenSearchableEntity = entity;

            // 9) Apply post search filters
            entity.ApplyResultFilters(ref osr);

            return osr;
        }

        /// <summary>
        /// Try to discover the OpenSearchDescription from an URL.
        /// </summary>
        /// <returns>An OpenSearchDescription</returns>
        /// <param name="url">URL.</param>
        public OpenSearchDescription AutoDiscoverFromQueryUrl(OpenSearchUrl url) {

            OpenSearchRequest request = OpenSearchRequest.Create(url);

            ApplyPreSearchFilters(ref request);

            OpenSearchResponse response = request.GetResponse();

            ApplyPostSearchFilters(request, ref response);

            IOpenSearchEngineExtension osee = GetExtensionByContentTypeAbility(response.ContentType);

            if (osee == null)
                throw new ImpossibleSearchException("No registered extension is able to read content of type " + response.ContentType);

            OpenSearchUrl descriptionUrl = osee.FindOpenSearchDescriptionUrlFromResponse(response);

            if (descriptionUrl == null)
                throw new ImpossibleSearchException("No Opensearch Description link found in results of " + url.ToString());

            OpenSearchDescription osd = LoadOpenSearchDescriptionDocument(descriptionUrl);

            osd.DefaultUrl = osd.Url.Single(u => u.Type == response.ContentType);

            return osd;
        }

        /// <summary>
        /// Loads an OpenSearchDescription document from an Url.
        /// </summary>
        /// <returns>The open search description document.</returns>
        /// <param name="url">URL.</param>
        public OpenSearchDescription LoadOpenSearchDescriptionDocument(OpenSearchUrl url) {

            OpenSearchRequest request = OpenSearchRequest.Create(url);

            ApplyPreSearchFilters(ref request);

            OpenSearchResponse response = request.GetResponse();

            ApplyPostSearchFilters(request, ref response);

            try {
                XmlSerializer ser = new XmlSerializer(typeof(OpenSearchDescription));
                return (OpenSearchDescription)ser.Deserialize(XmlReader.Create(response.GetResponseStream()));
            } catch (Exception e) {
                throw new Exception("Exception querying OpenSearch description at " + url.ToString() + " : " + e.Message, e);
            }

        }

        /// <summary>
        /// Gets the enclosures.
        /// </summary>
        /// <returns>The enclosures.</returns>
        /// <param name="result">Result.</param>
        public SyndicationLink[] GetEnclosures(IOpenSearchResult result) {

            Type type = result.Result.GetType();

            IOpenSearchEngineExtension osee = GetFirstExtensionByTypeAbility(type);

            if (osee == null)
                throw new InvalidOperationException("No registered extensions able to get media enclosures for " + type.ToString());

            return osee.GetEnclosures(result);

        }

        /// <summary>
        /// Applies the post search filters.
        /// </summary>
        /// <param name="request">Request.</param>
        /// <param name="response">Response.</param>
        private void ApplyPostSearchFilters(OpenSearchRequest request, ref OpenSearchResponse response) {
            foreach (PostFilterAction filter in postFilters) {

                filter.Invoke(request, ref response);

            }
        }

        /// <summary>
        /// Applies the pre search filters.
        /// </summary>
        /// <param name="request">Request.</param>
        private void ApplyPreSearchFilters(ref OpenSearchRequest request) {
           
            foreach (PreFilterAction filter in preFilters) {

                filter.Invoke(ref request);

            }

        }

        /// <summary>
        /// Loads the plugins automatically based on Mono.Addins
        /// </summary>
        public void LoadPlugins() {

            foreach (TypeExtensionNode node in AddinManager.GetExtensionNodes (typeof(IOpenSearchEngineExtension))) {
                IOpenSearchEngineExtension osee = (IOpenSearchEngineExtension)node.CreateInstance();
                Type type = osee.GetTransformType();
                this.RegisterExtension(osee);
            }

        }

        /// <summary>
        /// Gets the first extension by content type ability.
        /// </summary>
        /// <returns>The first extension by content type ability.</returns>
        /// <param name="contentType">Content type.</param>
        public IOpenSearchEngineExtension GetExtensionByContentTypeAbility(string contentType) {
            foreach (IOpenSearchEngineExtension osee in extensions.Values) {

                if (osee.DiscoveryContentType == contentType)
                    return osee;
            }

            return null;
        }

        public IOpenSearchEngineExtension GetFirstExtensionByTypeAbility(Type type) {
            foreach (IOpenSearchEngineExtension osee in extensions.Values) {

                if (osee.GetTransformType() == type)
                    return osee;
            }

            return null;
        }

        public Dictionary<int, IOpenSearchEngineExtension> Extensions {
            get {
                return extensions;
            }
        }

        IOpenSearchResult CreateOpenSearchResult(IOpenSearchResultCollection newResults, OpenSearchRequest request, OpenSearchResponse response) {

            bool totalResults = false;

            foreach (SyndicationElementExtension ext in newResults.ElementExtensions.ToArray()) {
                if ( ext.OuterName == "startIndex" && ext.OuterNamespace == "http://a9.com/-/spec/opensearch/1.1/")
                    newResults.ElementExtensions.Remove(ext);
                if ( ext.OuterName == "itemsPerPage" && ext.OuterNamespace == "http://a9.com/-/spec/opensearch/1.1/")
                    newResults.ElementExtensions.Remove(ext);
                if ( ext.OuterName == "Query" && ext.OuterNamespace == "http://a9.com/-/spec/opensearch/1.1/")
                    newResults.ElementExtensions.Remove(ext);
            }
            newResults.ElementExtensions.Add("startIndex", "http://a9.com/-/spec/opensearch/1.1/", request.OpenSearchUrl.IndexOffset);
            newResults.ElementExtensions.Add("itemsPerPage", "http://a9.com/-/spec/opensearch/1.1/", request.OpenSearchUrl.Count);
            var query = newResults.Links.SingleOrDefault(l => l.RelationshipType == "self");
            newResults.ElementExtensions.Add("Query", "http://a9.com/-/spec/opensearch/1.1/", query == null ? "" : query.Uri.ToString());

            OpenSearchResult osr = new OpenSearchResult(newResults, request.Parameters);

            return osr;
     
        }

        #region IOpenSearchableFactory implementation

        /// <summary>
        /// Create an IOpenSearchable from an OpenSearchUrl
        /// </summary>
        /// <param name="url">URL to either a search or a description</param>
        public IOpenSearchable Create(OpenSearchUrl url) {
            return new GenericOpenSearchable(url, this);
        }

        /// <summary>
        /// Create an IOpenSearchable from an OpenSearchUrl
        /// </summary>
        /// <param name="url">URL to either a search or a description</param>
        /// <param name="osd">Osd.</param>
        public IOpenSearchable Create(OpenSearchDescription osd) {
            return new GenericOpenSearchable(osd, this);
        }

        #endregion
    }
}

