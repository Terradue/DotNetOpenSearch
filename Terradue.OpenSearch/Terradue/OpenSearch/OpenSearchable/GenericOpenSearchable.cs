//
//  GenericOpenSearchable.cs
//
//  Author:
//       Emmanuel Mathot <emmanuel.mathot@terradue.com>
//
//  Copyright (c) 2014 Terradue

using System;
using System.Web;
using System.Net;
using Terradue.OpenSearch.Engine.Extensions;
using System.Collections.Specialized;
using Terradue.OpenSearch.Request;
using Terradue.OpenSearch.Engine;
using Terradue.OpenSearch.Response;
using Terradue.OpenSearch.Result;
using Terradue.OpenSearch.Schema;

namespace Terradue.OpenSearch {
    /// <summary>
    /// Generic class to represent any OpenSearchable entity
    /// </summary>
    public class GenericOpenSearchable : IOpenSearchable {
        protected OpenSearchDescription osd;
        protected OpenSearchUrl url;
        protected OpenSearchEngine ose;

        /// <summary>
        /// Initializes a new instance of the <see cref="Terradue.OpenSearch.GenericOpenSearchable"/> class from a quaery Url
        /// </summary>
        /// <param name="url">The query URL</param>
        /// <param name="ose">An OpenSearchEngine instance, preferably with registered extensions able to read the query url</param>
        public GenericOpenSearchable(OpenSearchUrl url, OpenSearchEngine ose) {
            this.url = url;
            this.ose = ose;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Terradue.OpenSearch.GenericOpenSearchable"/> class from an OpenSearchDescription.
        /// </summary>
        /// <param name="osd">The OpenSearchDescription describing the OpenSearchable entity to represent</param>
        /// <param name="ose">An OpenSearchEngine instance, preferably with registered extensions able to read the query url</param>
        public GenericOpenSearchable(OpenSearchDescription osd, OpenSearchEngine ose) {
            this.osd = osd;
            this.ose = ose;
            url = null;
        }

        #region IOpenSearchable implementation

        public Tuple<string, Func<OpenSearchResponse, object>> GetTransformFunction(OpenSearchEngine ose, Type resultType) {
            IOpenSearchEngineExtension osee = ose.GetExtension(resultType);
            return OpenSearchFactory.BestTransformFunctionByNumberOfParam(this, osee);
        }

        public OpenSearchRequest Create(string type, NameValueCollection parameters) {
            NameValueCollection nvc = new NameValueCollection(parameters);
            if (url != null) nvc.Add(HttpUtility.ParseQueryString(url.Query));
            return OpenSearchRequest.Create(this, type, nvc);
        }

        public string Identifier {
            get {
                return GetOpenSearchDescription().ShortName;
            }
        }

        public OpenSearchDescription GetOpenSearchDescription() {
            if (osd == null) this.osd = ose.AutoDiscoverFromQueryUrl(url);
            return osd;
        }

        public NameValueCollection GetOpenSearchParameters(string mimeType) {
            foreach (OpenSearchDescriptionUrl url in GetOpenSearchDescription().Url) {
                if (url.Type == mimeType) {
                    return HttpUtility.ParseQueryString(new Uri(url.Template).Query);
                }
            }

            return null;
        }

        public ulong TotalResults() {
            throw new NotImplementedException();
        }

        public void ApplyResultFilters(ref IOpenSearchResult osr) {
            // do nothing.
        }

        public OpenSearchUrl GetSearchBaseUrl(string mimetype) {
            throw new InvalidOperationException("Base Url unknow for Generic OpenSearchable");
        }

        #endregion
    }
}

