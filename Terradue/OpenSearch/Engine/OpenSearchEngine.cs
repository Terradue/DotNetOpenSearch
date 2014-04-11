using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections;
using System.Collections.Specialized;
using System.Net;
using System.Web;
using System.Diagnostics;
using Mono.Addins;
using System.ServiceModel.Syndication;
using System.Xml.Serialization;
using System.Xml;
using Terradue.Util;

namespace Terradue.OpenSearch {
    /// <summary>
    /// The engine for making OpenSearch request
    /// </summary>
    public sealed partial class OpenSearchEngine : IOpenSearchableFactory {
        Dictionary<Type, IOpenSearchEngineExtension> extensions;
        List<PreFilterAction> preFilters;
        List<PostFilterAction> postFilters;

        /// <summary>
        /// Initializes a new instance of the <see cref="Terradue.OpenSearch.OpenSearchEngine"/> class.
        /// </summary>
        public OpenSearchEngine() {
            extensions = new Dictionary<Type,IOpenSearchEngineExtension>();
            preFilters = new List<PreFilterAction>();
            postFilters = new List<PostFilterAction>();
            DefaultCount = 20;
            DefaultTimeOut = 10000;
        }

        public delegate void PreFilterAction(ref OpenSearchRequest request);

        public delegate void PostFilterAction(OpenSearchRequest request, ref OpenSearchResponse response);

        /// <summary>
        /// Gets the IOpenSearchEngineExtension registered for the given Type.
        /// </summary>
        /// <returns>IOpenSearchEngineExtension that manage the Type.</returns>
        /// <param name="type">Type registered.</param>
        /// <exception cref="InvalidOperationException">The type does not exists in the registered extensions</exception>
        public IOpenSearchEngineExtension GetExtension(Type type) {
            try {
                return extensions[type];
            } catch (Exception) {
                throw new InvalidOperationException("No OpenSearch extension found for type " + type.Name);
            }
        }

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
        public void RegisterType(Type type, IOpenSearchEngineExtension extension) {
            extensions.Add(type, extension);
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

            return Query(entity, parameters, GetTypeByExtensionName(resultName));

        }

        /// <summary>
        /// Gets the name of the type by extension's result name
        /// </summary>
        /// <returns>The type by extension name.</returns>
        /// <param name="resultName">Result name.</param>
        public Type GetTypeByExtensionName(string resultName) {
            foreach (IOpenSearchEngineExtension extension in extensions.Values) {
                if (extension.GetTransformName() == resultName) return  extension.GetTransformType();
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
            Tuple<string,Func<OpenSearchResponse, object>> transformFunction;
            // Results
            IOpenSearchResult osr = null;

            // 1) Find the best match between urlTemplates and result format
            transformFunction = entity.GetTransformFunction(this, resultType);
            if (transformFunction == null) throw new ImpossibleSearchException(String.Format("No engine extension to query {0} in order to return {1}", entity.Identifier, resultType.FullName));

            // 2) Create the request
            OpenSearchRequest request = entity.Create(transformFunction.Item1, parameters);

            // 5) Apply the pre-search functions
            ApplyPreSearchFilters(ref request);

            // 6) Perform the Search
            OpenSearchResponse response = request.GetResponse();
            response.Entity = entity;

            // 5) Apply the pre-search functions
            ApplyPostSearchFilters(request, ref response);

            // 7) Transform the response         
            osr = new OpenSearchResult(transformFunction.Item2(response), request.Parameters);

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

            IOpenSearchEngineExtension osee = GetFirstExtensionByContentTypeAbility(response.ContentType);

            if (osee == null) throw new ImpossibleSearchException("No registered extension is able to read content of type " + response.ContentType);

            OpenSearchUrl descriptionUrl = osee.FindOpenSearchDescriptionUrlFromResponse(response);

            if (descriptionUrl == null) throw new ImpossibleSearchException("No Opensearch Description link found in results of " + url.ToString());

            OpenSearchDescription osd = LoadOpenSearchDescriptionDocument(descriptionUrl);

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

            IOpenSearchEngineExtension osee = extensions[type];

            if (osee == null) throw new InvalidOperationException("No registered extensions able to get media enclosures for " + type.ToString());

            return osee.GetEnclosures(result.Result);

        }

        /// <summary>
        /// Applies the post search filters.
        /// </summary>
        /// <param name="request">Request.</param>
        /// <param name="response">Response.</param>
        private void ApplyPostSearchFilters(OpenSearchRequest request, ref OpenSearchResponse response) {
            foreach (PostFilterAction filter in postFilters) {

                filter(request, ref response);

            }
        }

        /// <summary>
        /// Applies the pre search filters.
        /// </summary>
        /// <param name="request">Request.</param>
        private void ApplyPreSearchFilters(ref OpenSearchRequest request) {
           
            foreach (PreFilterAction filter in preFilters) {

                filter(ref request);

            }

        }

        /// <summary>
        /// Loads the plugins automatically based on Mono.Addins
        /// </summary>
        public void LoadPlugins() {

            foreach (TypeExtensionNode node in AddinManager.GetExtensionNodes (typeof(IOpenSearchEngineExtension))) {
                IOpenSearchEngineExtension osee = (IOpenSearchEngineExtension)node.CreateInstance();
                Type type = osee.GetTransformType();
                this.RegisterType(type, osee);
            }

        }

        /// <summary>
        /// Lists the transformation path.
        /// </summary>
        /// <returns>The transformation path.</returns>
        /// <param name="inputType">Input type.</param>
        public List<Tuple<string, string>> ListTransformationPath(string inputType = null) {

            List<Tuple<string, string>> transformPaths = new List<Tuple<string, string>>();

            foreach (Type type in extensions.Keys) {
                foreach (string contentType in extensions[type].GetInputFormatTransformPath()) {
                    if (inputType != null && contentType != inputType) continue;
                    transformPaths.Add(new Tuple<string,string>(contentType, type.Name));
                }
            }

            return transformPaths;
        }

        /// <summary>
        /// Gets the first extension by content type ability.
        /// </summary>
        /// <returns>The first extension by content type ability.</returns>
        /// <param name="contentType">Content type.</param>
        public IOpenSearchEngineExtension GetFirstExtensionByContentTypeAbility(string contentType) {
            foreach (IOpenSearchEngineExtension osee in extensions.Values) {

                if (osee.GetInputFormatTransformPath().FirstOrDefault(s => s == contentType) != null) return osee;
            }

            return null;
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

