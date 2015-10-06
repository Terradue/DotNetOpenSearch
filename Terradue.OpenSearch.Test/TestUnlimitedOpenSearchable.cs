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

namespace Terradue.OpenSearch.Test {

    public class TestUnlimitedOpenSearchable : IMonitoredOpenSearchable {

        public TestUnlimitedOpenSearchable() {
        }

        #region IOpenSearchable implementation

        public QuerySettings GetQuerySettings(Terradue.OpenSearch.Engine.OpenSearchEngine ose) {
            IOpenSearchEngineExtension osee = new AtomOpenSearchEngineExtension();
            return new QuerySettings(osee.DiscoveryContentType, osee.ReadNative);
        }

        public OpenSearchRequest Create(string type, NameValueCollection parameters) {
            UriBuilder url = new UriBuilder("dummy://localhost");
            url.Path += Identifier + "/search";
            var array = (from key in parameters.AllKeys
                                  from value in parameters.GetValues(key)
                                  select string.Format("{0}={1}", HttpUtility.UrlEncode(key), HttpUtility.UrlEncode(value)))
                .ToArray();
            url.Query = string.Join("&", array);

            AtomOpenSearchRequest request = new AtomOpenSearchRequest(new OpenSearchUrl(url.Uri), GenerateSyndicationFeed);

            return request;
        }

        public Terradue.OpenSearch.Schema.OpenSearchDescription GetOpenSearchDescription() {
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

        public System.Collections.Specialized.NameValueCollection GetOpenSearchParameters(string mimeType) {
            return OpenSearchFactory.GetBaseOpenSearchParameter();
        }

        public void ApplyResultFilters(Terradue.OpenSearch.Request.OpenSearchRequest request, ref Terradue.OpenSearch.Result.IOpenSearchResultCollection osr, string finalContentType) {

        }

        string identifier;

        public string Identifier {
            get {
                return identifier;
            }
            set { identifier = value; }
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

        public event OpenSearchableChangeEventHandler OpenSearchableChange;

        public void OnOpenSearchableChange(object sender, OnOpenSearchableChangeEventArgs data) {
            OpenSearchableChange(sender, data);
        }

        private AtomFeed GenerateSyndicationFeed(NameValueCollection parameters) {
            UriBuilder myUrl = new UriBuilder("file:///test");
            string[] queryString = Array.ConvertAll(parameters.AllKeys, key => String.Format("{0}={1}", key, parameters[key]));
            myUrl.Query = string.Join("&", queryString);

            AtomFeed feed = new AtomFeed("Discovery feed for " + this.Identifier,
                                         "This OpenSearch Service allows the discovery of the different items which are part of the " + this.Identifier + " collection" +
                                         "This search service is in accordance with the OGC 10-032r3 specification.",
                                         myUrl.Uri, myUrl.ToString(), DateTimeOffset.UtcNow);

            feed.Generator = "Terradue Web Server";

            List<AtomItem> items = new List<AtomItem>();

            // Load all avaialable Datasets according to the context

            PaginatedList<TestItem> pds = new PaginatedList<TestItem>();

            int startIndex = 1;
            if (parameters["startIndex"] != null)
                startIndex = int.Parse(parameters["startIndex"]);

            pds.AddRange(Items);

            pds.PageNo = 1;
            if (parameters["startPage"] != null)
                pds.PageNo = int.Parse(parameters["startPage"]);

            pds.PageSize = 20;
            if (parameters["count"] != null)
                pds.PageSize = int.Parse(parameters["count"]);

            pds.StartIndex = startIndex - 1;

            if (this.Identifier != null)
                feed.Identifier = this.Identifier;

            foreach (TestItem s in pds.GetCurrentPage()) {

                if (s is IAtomizable) {
                    AtomItem item = (s as IAtomizable).ToAtomItem(parameters);
                    if (item != null)
                        items.Add(item);
                } else {

                    string fIdentifier = s.Identifier;
                    string fName = s.Name;
                    string fText = (s.TextContent != null ? s.TextContent : "");

                    if (!string.IsNullOrEmpty(parameters["q"])) {  
                        string q = parameters["q"];
                        if (!(fName.Contains(q) || fIdentifier.Contains(q) || fText.Contains(q)))
                            continue;
                    }

                    Uri alternate = new Uri("file:///test/search?count=0&id=" + fIdentifier);
                    Uri id = new Uri(s.Id);

                    AtomItem entry = new AtomItem(fIdentifier, fName, alternate, id.ToString(), s.Date);
                    entry.PublishDate = s.Date.DateTime;
                    entry.LastUpdatedTime = s.Date.DateTime;
                    entry.Categories.Add(new SyndicationCategory(this.Identifier));

                    entry.Summary = new TextSyndicationContent(fName);
                    entry.ElementExtensions.Add("identifier", "http://purl.org/dc/elements/1.1/", fIdentifier);

                    items.Add(entry);
                }
            }

            feed.Items = items;
            var tr = pds.Count();
            feed.TotalResults = tr;

            return feed;
        }

        public IEnumerable<TestItem> Items {
            get;
            set;
        }

        public static TestUnlimitedOpenSearchable GenerateNumberedItomFeed(string lid, int n, TimeSpan shift) {

            TestUnlimitedOpenSearchable test = new TestUnlimitedOpenSearchable();
            test.Identifier = lid;
            List<TestItem> items = new List<TestItem>();

            for (int i = 1; i <= n; i++) {

                TestItem item = new TestItem(i);
                item.Identifier = lid + i;
                item.Name = "Item" + lid + i;
                item.TextContent = "This is the text for item " + lid + i;
                item.Shift = shift;
                items.Add(item);

            }

            test.Items = items;

            return test;

        }

       
    }
}

