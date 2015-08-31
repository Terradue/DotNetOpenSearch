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
using System.Linq;

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
            this.osd = ose.AutoDiscoverFromQueryUrl(url);
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

        public QuerySettings GetQuerySettings(OpenSearchEngine ose) {
            string defaultMimeType = this.DefaultMimeType;
            if (defaultMimeType.Contains(";"))
                defaultMimeType = defaultMimeType.Split(';')[0];
            IOpenSearchEngineExtension osee = ose.GetExtensionByContentTypeAbility(defaultMimeType);
            if (osee == null)
                return null;

            return new QuerySettings(this.DefaultMimeType, osee.ReadNative);
        }

        public OpenSearchRequest Create(string type, NameValueCollection parameters) {
            NameValueCollection nvc;
            if (url != null)
                nvc = HttpUtility.ParseQueryString(url.Query);
            else
                nvc = new NameValueCollection();
            parameters.AllKeys.SingleOrDefault(k => {
                nvc.Set(k, parameters[k]);
                return false;
            });
            
            return OpenSearchRequest.Create(this, type, nvc);
        }

        public string Identifier {
            get {
                return GetOpenSearchDescription().ShortName;
            }
        }

        public OpenSearchDescription GetOpenSearchDescription() {
            if (osd == null)
                this.osd = ose.AutoDiscoverFromQueryUrl(url);
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

        long totalResults = -1;

        public long TotalResults {
            get {
                if (totalResults <= 0) {
                    NameValueCollection @params = new NameValueCollection();
                    @params.Set("count", "0");
                    var request = this.Create("application/atom+xml", @params );
                    var iosr = request.GetResponse();
                    var querySettings = this.GetQuerySettings(ose);
                    var results = querySettings.ReadNative.Invoke(iosr);
                    try { 
                        totalResults = results.TotalResults;
                    } catch {
                        totalResults = 0;
                    }
                }
                return totalResults;
            }
        }

        public void ApplyResultFilters(OpenSearchRequest request, ref IOpenSearchResultCollection osr, string finalContentType) {
            // do nothing.
        }

        public string DefaultMimeType {
            get {
                if (string.IsNullOrEmpty(GetOpenSearchDescription().DefaultUrl.Type) || GetOpenSearchDescription().DefaultUrl.Type == "application/opensearchdescription+xml")
                    return "application/atom+xml";
                
                return osd.DefaultUrl.Type;

            }
        }

        public bool CanCache {
            get {
                return true;
            }
        }

        #endregion
    }
}

