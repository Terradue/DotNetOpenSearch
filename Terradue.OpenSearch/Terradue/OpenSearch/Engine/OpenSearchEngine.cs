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
using System.IO;

/*!

\defgroup OpenSearchEngine OpenSearch Engine
@{
This is the core of the OpenSearch mechanism

\xrefitem int "Interfaces" "Interfaces" process \ref OpenSearchable entities to expose them in OpenSearch interface.

\xrefitem dep "Dependencies" "Dependencies" uses \ref Syndication model to represent \ref OpenSearchable entities.

\xrefitem norm "Normative References" "Normative References" [OpenSearch 1.1](http://www.opensearch.org/Specifications/OpenSearch/1.1)

\ingroup OpenSearch

@}

*/
using log4net;

namespace Terradue.OpenSearch.Engine {

    /// <summary>
    /// The engine for making OpenSearch request
    /// </summary>
    public sealed partial class OpenSearchEngine : IOpenSearchableFactory {

        public const int DEFAULT_COUNT = 20;
        public const int DEFAULT_MAXCOUNT = 100;
        public const int DEFAULT_VALIDITY = 3600;

        private log4net.ILog log = log4net.LogManager.GetLogger
            (System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

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
            MaxCount = DEFAULT_MAXCOUNT;
        }

        public delegate void PreFilterAction(ref OpenSearchRequest request);

        public delegate void PostFilterAction(OpenSearchRequest request, ref IOpenSearchResponse response);

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

        public int MaxCount {
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
            log.Debug("Registering extension: " + extension.Identifier);
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
        public IOpenSearchResultCollection Query(IOpenSearchable entity, NameValueCollection parameters, string resultName) {

            // Transform action to invoke
            QuerySettings querySettings;

            // 1) Find the plugin to match with urlTemplates
            querySettings = entity.GetQuerySettings(this);
            if (querySettings == null)
                throw new ImpossibleSearchException(String.Format("No engine extension to query {0}", entity.Identifier));

            // 2) Create the request
            OpenSearchRequest request = entity.Create(querySettings.PreferredContentType, parameters);

            // 3) Apply the pre-search functions
            ApplyPreSearchFilters(ref request);

            // 4) Perform the Search
            IOpenSearchResponse response = request.GetResponse();
            response.Entity = entity;

            // 5) Apply the pre-search functions
            ApplyPostSearchFilters(request, ref response);

            // 6) Transform the response
            IOpenSearchResultCollection results = querySettings.ReadNative.Invoke(response);
            results.Parameters = request.Parameters;
            results.LastUpdatedTime = DateTime.Now;

            IOpenSearchEngineExtension osee = GetExtensionByExtensionName(resultName);

            // 7) Apply post search filters
            entity.ApplyResultFilters(request, ref results, osee.DiscoveryContentType);

            // 8) Change format
            IOpenSearchResultCollection newResults = osee.CreateOpenSearchResultFromOpenSearchResult(results);

            // 9) Create Result
            ApplyOpenSearchElements(ref newResults, request, response);

            return newResults;

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
                if (string.Compare(extension.Identifier, resultName, true) == 0)
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
        public IOpenSearchResultCollection Query(IOpenSearchable entity, NameValueCollection parameters, Type resultType) {

            // Transform action to invoke
            QuerySettings querySettings;

            // 1) Find the best match between urlTemplates and result format
            querySettings = entity.GetQuerySettings(this);
            if (querySettings == null)
                throw new ImpossibleSearchException(String.Format("No engine extension to query {0} in order to return {1}", entity.Identifier, resultType.FullName));

            // 2) Create the request
            OpenSearchRequest request = entity.Create(querySettings.PreferredContentType, parameters);

            // 3) Apply the pre-search functions
            ApplyPreSearchFilters(ref request);

            // 4) Perform the Search
            IOpenSearchResponse response = request.GetResponse();
            response.Entity = entity;

            // 5) Apply the pre-search functions
            ApplyPostSearchFilters(request, ref response);

            // 6) Transform the response
            IOpenSearchResultCollection results = querySettings.ReadNative.Invoke(response);
            results.Parameters = request.Parameters;
            results.LastUpdatedTime = DateTime.Now;

            IOpenSearchEngineExtension osee = GetFirstExtensionByTypeAbility(resultType);

            // 7) Apply post search filters
            entity.ApplyResultFilters(request, ref results, osee.DiscoveryContentType);

            // 8) Change format
            IOpenSearchResultCollection newResults = osee.CreateOpenSearchResultFromOpenSearchResult(results);

            // 9) Create Result container
            ApplyOpenSearchElements(ref newResults, request, response);

            return newResults;
        }

        /// <summary>
        /// Query the specified IOpenSearchable entity with specific parameters and returns result in native format.
        /// </summary>
        /// <param name="entity">Entity.</param>
        /// <param name="parameters">Parameters.</param>
        public IOpenSearchResultCollection Query(IOpenSearchable entity, NameValueCollection parameters) {

            // Transform action to invoke
            QuerySettings querySettings;

            // 1) Find the plugin to match with urlTemplates
            querySettings = entity.GetQuerySettings(this);
            if (querySettings == null)
                throw new ImpossibleSearchException(String.Format("No engine extension to query {0}", entity.Identifier));

            // 2) Create the request
            OpenSearchRequest request = entity.Create(querySettings.PreferredContentType, parameters);

            // 3) Apply the pre-search functions
            ApplyPreSearchFilters(ref request);

            // 4) Perform the Search
            IOpenSearchResponse response = request.GetResponse();
            response.Entity = entity;

            // 5) Apply the pre-search functions
            ApplyPostSearchFilters(request, ref response);

            // 6) Read the response 
            var results = querySettings.ReadNative.Invoke(response);
            results.Parameters = request.Parameters;
            results.LastUpdatedTime = DateTime.Now;

            // 7) Apply post search filters
            entity.ApplyResultFilters(request, ref results, results.ContentType);

            // 8 Create the container
            ApplyOpenSearchElements(ref results, request, response);

            return results;
        }

        /// <summary>
        /// Try to discover the OpenSearchDescription from an URL.
        /// </summary>
        /// <returns>An OpenSearchDescription</returns>
        /// <param name="url">URL.</param>
        public OpenSearchDescription AutoDiscoverFromQueryUrl(OpenSearchUrl url) {

            OpenSearchDescription osd = null;
            OpenSearchUrl descriptionUrl = null;

            OpenSearchRequest request = OpenSearchRequest.Create(url);

            ApplyPreSearchFilters(ref request);

            IOpenSearchResponse response = request.GetResponse();

            ApplyPostSearchFilters(request, ref response);

            string contentType = response.ContentType;
            if (contentType.Contains(";"))
                contentType = contentType.Split(';')[0];

            if (response.ContentType == "application/opensearchdescription+xml") {
                osd = this.LoadOpenSearchDescriptionDocument(url);
                descriptionUrl = url;
            } else {

                IOpenSearchEngineExtension osee = GetExtensionByContentTypeAbility(contentType);

                if (osee == null)
                    throw new ImpossibleSearchException("No registered extension is able to read content of type " + response.ContentType);

                descriptionUrl = osee.FindOpenSearchDescriptionUrlFromResponse(response);

                if (descriptionUrl == null)
                    throw new ImpossibleSearchException("No Opensearch Description link found in results of " + url.ToString());

                osd = LoadOpenSearchDescriptionDocument(descriptionUrl);
            }

            osd.Originator = descriptionUrl;

            if (contentType == "application/opensearchdescription+xml")
                contentType = "application/atom+xml";

            osd.DefaultUrl = osd.Url.FirstOrDefault(u => u.Type.StartsWith(contentType));

            return osd;
        }

        /// <summary>
        /// Loads an OpenSearchDescription document from an Url.
        /// </summary>
        /// <returns>The open search description document.</returns>
        /// <param name="url">URL.</param>
        public OpenSearchDescription LoadOpenSearchDescriptionDocument(OpenSearchUrl url) {

            OpenSearchDescription osd;

            OpenSearchRequest request = OpenSearchRequest.Create(url);

            ApplyPreSearchFilters(ref request);

            IOpenSearchResponse response = request.GetResponse();

            ApplyPostSearchFilters(request, ref response);

            if (response.ObjectType != typeof(byte[]))
                throw new InvalidOperationException("The OpenSearch Description document did not return byte[] body");

            try {
                XmlSerializer ser = new XmlSerializer(typeof(OpenSearchDescription));
                Stream stream = new MemoryStream((byte[])response.GetResponseObject());
                osd = (OpenSearchDescription)ser.Deserialize(XmlReader.Create(stream));
                stream.Flush();
                stream.Close();
            } catch (Exception e) {
                throw new Exception("Exception querying OpenSearch description at " + url.ToString() + " : " + e.Message, e);
            } 

            return osd;

        }

        /// <summary>
        /// Gets the enclosures.
        /// </summary>
        /// <returns>The enclosures.</returns>
        /// <param name="result">Result.</param>
        public SyndicationLink[] GetEnclosures(IOpenSearchResultCollection result) {

            Type type = result.GetType();

            IOpenSearchEngineExtension osee = GetFirstExtensionByTypeAbility(type);

            if (osee == null)
                throw new InvalidOperationException("No registered extensions able to get media enclosures for " + type.ToString());

            return OpenSearchFactory.GetEnclosures(result);

        }

        /// <summary>
        /// Applies the post search filters.
        /// </summary>
        /// <param name="request">Request.</param>
        /// <param name="response">Response.</param>
        private void ApplyPostSearchFilters(OpenSearchRequest request, ref IOpenSearchResponse response) {
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

        public IOpenSearchEngineExtension GetExtensionByContentTypeAbility(string[] contentType) {
            foreach (IOpenSearchEngineExtension osee in extensions.Values) {

                if (contentType.Contains(osee.DiscoveryContentType))
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

        void ApplyOpenSearchElements(ref IOpenSearchResultCollection newResults, OpenSearchRequest request, IOpenSearchResponse response) {

            foreach (SyndicationElementExtension ext in newResults.ElementExtensions.ToArray()) {
                if (ext.OuterName == "startIndex" && ext.OuterNamespace == "http://a9.com/-/spec/opensearch/1.1/")
                    newResults.ElementExtensions.Remove(ext);
                if (ext.OuterName == "itemsPerPage" && ext.OuterNamespace == "http://a9.com/-/spec/opensearch/1.1/")
                    newResults.ElementExtensions.Remove(ext);
                if (ext.OuterName == "Query" && ext.OuterNamespace == "http://a9.com/-/spec/opensearch/1.1/")
                    newResults.ElementExtensions.Remove(ext);
            }
            newResults.ElementExtensions.Add("startIndex", "http://a9.com/-/spec/opensearch/1.1/", request.OpenSearchUrl.IndexOffset);
            newResults.ElementExtensions.Add("itemsPerPage", "http://a9.com/-/spec/opensearch/1.1/", request.OpenSearchUrl.Count);

           
            XElement query = new XElement(XName.Get("Query", "http://a9.com/-/spec/opensearch/1.1/"));
            OpenSearchDescription osd = null;
            if (response.Entity is IProxiedOpenSearchable)
                osd = ((IProxiedOpenSearchable)response.Entity).GetProxyOpenSearchDescription();
            else
                osd = response.Entity.GetOpenSearchDescription();
            foreach (var ns in osd.ExtraNamespace.ToArray()) {
                if (string.IsNullOrEmpty(ns.Name) || ns.Namespace == "http://www.w3.org/2001/XMLSchema" || ns.Namespace == "http://www.w3.org/2001/XMLSchema-instance" || ns.Namespace == XNamespace.Xmlns.NamespaceName)
                    continue;
                query.Add(new XAttribute(XNamespace.Xmlns + ns.Name, ns.Namespace));
            }
            var osUrl = OpenSearchFactory.GetOpenSearchUrlByType(osd, request.ContentType);
            var osparams = OpenSearchFactory.GetOpenSearchParameters(osUrl);
            foreach (var key in request.Parameters.AllKeys) {
                string osparam = OpenSearchFactory.GetParamNameFromId(osparams, key);
                if (!string.IsNullOrEmpty(osparam)) {
                    try {
                        if (osparam.Contains(":"))
                            query.Add(new XAttribute(XName.Get(osparam.Split(':')[1], osd.ExtraNamespace.ToArray().First(n => n.Name == osparam.Split(':')[0]).Namespace), request.Parameters[key]));
                        else {
                            query.Add(new XAttribute(XName.Get(osparam, osd.ExtraNamespace.ToArray().First(n => n.Name == "os").Namespace), request.Parameters[key]));
                        }
                    } catch {
                    }
                }
            }
            newResults.ElementExtensions.Add(query.CreateReader());
     
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

