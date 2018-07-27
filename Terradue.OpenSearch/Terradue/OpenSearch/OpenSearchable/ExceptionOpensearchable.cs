using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using Terradue.OpenSearch.Engine;
using Terradue.OpenSearch.Engine.Extensions;
using Terradue.OpenSearch.Request;
using Terradue.OpenSearch.Result;
using Terradue.OpenSearch.Schema;

namespace Terradue.OpenSearch {
	public class ExceptionOpensearchable : IOpenSearchable {
        
		readonly AtomFeed feed;

		public ExceptionOpensearchable(AtomFeed feed) {
            this.feed = feed;
        }

        public bool CanCache {
            get {
                return false;
            }
        }

        public string DefaultMimeType {
            get {
                return "application/atom+xml";
            }
        }

        public string Identifier {
            get {
                return feed.Identifier;
            }
        }

        public long TotalResults {
            get {
                return feed.Items.Count();
            }
        }

        public void ApplyResultFilters(OpenSearchRequest request, ref IOpenSearchResultCollection osr, string finalContentType) {

        }

        public OpenSearchRequest Create(QuerySettings querySettings, NameValueCollection parameters) {
            UriBuilder url = new UriBuilder("dummy://localhost");
            url.Path += Identifier + "/search";
            var array = (from key in parameters.AllKeys
                         from value in parameters.GetValues(key)
                         select string.Format("{0}={1}", HttpUtility.UrlEncode(key), HttpUtility.UrlEncode(value)))
                .ToArray();
            url.Query = string.Join("&", array);

			AtomOpenSearchRequest request = new AtomOpenSearchRequest(new OpenSearchUrl(url.Uri), GetExceptionAtomFeed);

            return request;
        }

        public OpenSearchDescription GetOpenSearchDescription() {
            OpenSearchDescription osd = new OpenSearchDescription();
            osd.ShortName = "test";
            osd.Contact = "info@terradue.com";
            osd.SyndicationRight = "open";
            osd.AdultContent = "false";
            osd.Language = "en-us";
            osd.OutputEncoding = "UTF-8";
            osd.InputEncoding = "UTF-8";
            osd.Developer = "Terradue OpenSearch Development Team";
            osd.Attribution = "Terradue";

            List<OpenSearchDescriptionUrl> urls = new List<OpenSearchDescriptionUrl>();

            UriBuilder urlb = new UriBuilder("dummy://localhost/test/description");

            OpenSearchDescriptionUrl url = new OpenSearchDescriptionUrl("application/opensearchdescription+xml", urlb.ToString(), "self");
            url.Parameters = OpenSearchFactory.GetDefaultParametersDescription(100).ToArray();
            urls.Add(url);

            urlb = new UriBuilder("dummy://localhost/test/search");
            NameValueCollection query = GetOpenSearchParameters("application/atom+xml");

            string[] queryString = Array.ConvertAll(query.AllKeys, key => string.Format("{0}={1}", key, query[key]));
            urlb.Query = string.Join("&", queryString);
            url = new OpenSearchDescriptionUrl("application/atom+xml", urlb.ToString(), "search");
            url.IndexOffset = 1;
            urls.Add(url);

            osd.Url = urls.ToArray();

            return osd;
        }

        public NameValueCollection GetOpenSearchParameters(string mimeType) {
            return OpenSearchFactory.GetBaseOpenSearchParameter();
        }
        
        public QuerySettings GetQuerySettings(OpenSearchEngine ose) {
            IOpenSearchEngineExtension osee = new AtomOpenSearchEngineExtension();
            return new QuerySettings(osee.DiscoveryContentType, osee.ReadNative);
        }

		AtomFeed GetExceptionAtomFeed(NameValueCollection parameters) {
			return feed;
        }
  
	}
}
