using System;
using Terradue.OpenSearch.Engine.Extensions;
using Terradue.OpenSearch.Request;
using System.Collections.Specialized;
using System.IO;
using Terradue.OpenSearch.Result;
using System.Collections.Generic;
using Terradue.ServiceModel.Syndication;
using System.Xml;
using System.Web;
using System.Linq;
using Terradue.OpenSearch.Schema;
using Terradue.OpenSearch.Filters;
using Terradue.OpenSearch.Engine;

namespace Terradue.OpenSearch.Test {

    public class IllimitedOpenSearchable : IOpenSearchable {
        OpenSearchableFactorySettings settings;

        TestUnlimitedOpenSearchable entity;

        public IllimitedOpenSearchable(TestUnlimitedOpenSearchable entity, OpenSearchableFactorySettings settings){
            this.entity = entity;
            this.settings = settings;
        }

        #region IOpenSearchable implementation

        public QuerySettings GetQuerySettings(Terradue.OpenSearch.Engine.OpenSearchEngine ose) {
            IOpenSearchEngineExtension osee = new AtomOpenSearchEngineExtension();
            return new QuerySettings(osee.DiscoveryContentType, osee.ReadNative);
        }

        public OpenSearchRequest Create(QuerySettings querySettings, NameValueCollection parameters) {
            UriBuilder url = new UriBuilder("dummy://localhost");
            url.Path += "illtest/search";
            var array = (from key in parameters.AllKeys
                                  from value in parameters.GetValues(key)
                                  select string.Format("{0}={1}", HttpUtility.UrlEncode(key), HttpUtility.UrlEncode(value)))
                .ToArray();
            url.Query = string.Join("&", array);

            IllimitedOpenSearchRequest<AtomFeed, AtomItem> request = new IllimitedOpenSearchRequest<AtomFeed, AtomItem>(settings, entity, querySettings.PreferredContentType, new OpenSearchUrl(url.Uri));

            return request;
        }

        public Terradue.OpenSearch.Schema.OpenSearchDescription GetOpenSearchDescription() {
            OpenSearchDescription osd = new OpenSearchDescription();
            osd.ShortName = "illimited test";
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

        public System.Collections.Specialized.NameValueCollection GetOpenSearchParameters(string mimeType) {
            return OpenSearchFactory.GetBaseOpenSearchParameter();
        }

        public void ApplyResultFilters(Terradue.OpenSearch.Request.OpenSearchRequest request, ref Terradue.OpenSearch.Result.IOpenSearchResultCollection osr, string finalContentType) {

        }

        public string Identifier {
            get {
                return "illtest";
            }
        }

        public long TotalResults {
            get {
                return Items.Count();
            }
        }

        public string DefaultMimeType {
            get {
                return "application/atom+xml";
            }
        }

        public bool CanCache {
            get {
                return true;
            }
        }

        #endregion

        public IEnumerable<TestItem> Items {
            get;
            set;
        }

    }
}

