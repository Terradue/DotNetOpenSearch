﻿using System;
using Terradue.OpenSearch.Engine.Extensions;
using Terradue.OpenSearch.Request;
using System.Collections.Specialized;
using System.IO;
using Terradue.OpenSearch.Result;
using System.Collections.Generic;
using Terradue.Util;
using Terradue.ServiceModel.Syndication;
using System.Xml;
using System.Web;
using System.Linq;
using Terradue.OpenSearch.Schema;

namespace Terradue.OpenSearch.Test {

    public class TestOpenSearchable : IOpenSearchable {

        public TestOpenSearchable() {
        }

        #region IOpenSearchable implementation
        public QuerySettings GetQuerySettings(Terradue.OpenSearch.Engine.OpenSearchEngine ose) {
            IOpenSearchEngineExtension osee = new AtomOpenSearchEngineExtension();
            return new QuerySettings(osee.DiscoveryContentType, osee.ReadNative);
        }
        public OpenSearchRequest Create(string type, NameValueCollection parameters) {
            UriBuilder url = new UriBuilder("file://");
            url.Path += "/test";
            var array = (from key in parameters.AllKeys
                         from value in parameters.GetValues(key)
                         select string.Format("{0}={1}", HttpUtility.UrlEncode(key), HttpUtility.UrlEncode(value)))
                .ToArray();
            url.Query = string.Join("&", array);

            MemoryOpenSearchRequest request = new MemoryOpenSearchRequest(new OpenSearchUrl(url.ToString()), type);

            Stream input = request.MemoryInputStream;

            GenerateSyndicationFeed(input, parameters);

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

            UriBuilder urlb = new UriBuilder("file:///test/description");

            OpenSearchDescriptionUrl url = new OpenSearchDescriptionUrl("application/opensearchdescription+xml", urlb.ToString(), "self");
            urls.Add(url);

            urlb = new UriBuilder("file:///test/search");
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
        public void ApplyResultFilters(Terradue.OpenSearch.Request.OpenSearchRequest request, ref Terradue.OpenSearch.Result.IOpenSearchResultCollection osr) {

        }
        public string Identifier {
            get {
                return "test";
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
        #endregion

        private void GenerateSyndicationFeed(Stream stream, NameValueCollection parameters) {
            UriBuilder myUrl = new UriBuilder("file:///test");
            string[] queryString = Array.ConvertAll(parameters.AllKeys, key => String.Format("{0}={1}", key, parameters[key]));
            myUrl.Query = string.Join("&", queryString);

            AtomFeed feed = new AtomFeed("Discovery feed for "+this.Identifier,
                                         "This OpenSearch Service allows the discovery of the different items which are part of the "+this.Identifier+" collection" +
                                         "This search service is in accordance with the OGC 10-032r3 specification.",
                                         myUrl.Uri, myUrl.ToString(), DateTimeOffset.UtcNow);

            feed.Generator = "Terradue Web Server";

            List<AtomItem> items = new List<AtomItem>();

            // Load all avaialable Datasets according to the context

            PaginatedList<TestItem> pds = new PaginatedList<TestItem>();

            int startIndex = 1;
            if (parameters["startIndex"] != null) startIndex = int.Parse(parameters["startIndex"]);

            pds.AddRange(Items);

            pds.PageNo = 1;
            if (parameters["startPage"] != null) pds.PageNo = int.Parse(parameters["startPage"]);

            pds.PageSize = 20;
            if (parameters["count"] != null) pds.PageSize = int.Parse(parameters["count"]);

            pds.StartIndex = startIndex-1;

            if(this.Identifier != null) feed.ElementExtensions.Add("identifier", "http://purl.org/dc/elements/1.1/", this.Identifier);

            foreach (TestItem s in pds.GetCurrentPage()) {

                if (s is IAtomizable) {
                    AtomItem item = (s as IAtomizable).ToAtomItem(parameters);
                    if(item != null) items.Add(item);
                } else {

                    string fIdentifier = s.Identifier;
                    string fName = s.Name;
                    string fText = (s.TextContent != null ? s.TextContent : "");

                    if (parameters["q"] != null) {  
                        string q = parameters["q"];
                        if (!(fName.Contains(q) || fIdentifier.Contains(q) || fText.Contains(q)))
                            continue;
                    }

                    Uri alternate = new Uri("file:///test/search?count=0&id=" + fIdentifier);
                    Uri id = new Uri(s.Id);

                    AtomItem entry = new AtomItem(fIdentifier, fName, alternate, id.ToString(), s.Date);
                    entry.PublishDate = s.Date;
                    entry.Categories.Add(new SyndicationCategory(this.Identifier));

                    entry.Summary = new TextSyndicationContent(fName);
                    entry.ElementExtensions.Add("identifier", "http://purl.org/dc/elements/1.1/", fIdentifier);

                    items.Add(entry);
                }
            }

            feed.Items = items;

            //Atomizable.SerializeToStream ( res, stream.OutputStream );
            var sw = XmlWriter.Create(stream);
            Atom10FeedFormatter atomFormatter = new Atom10FeedFormatter(feed.Feed);
            atomFormatter.WriteTo(sw);
            sw.Flush();
            sw.Close();
        }

        IEnumerable<TestItem> Items {
            get;
            set;
        }

        public static TestOpenSearchable GenerateNumberedItomFeed(string lid, int n){

            TestOpenSearchable test = new TestOpenSearchable();
            List<TestItem> items = new List<TestItem>();

            for (int i = 1; i <= n; i++) {

                TestItem item = new TestItem(i);
                item.Identifier = lid+i;
                item.Name = "Item" + lid+i;
                item.TextContent = "This is the text for item " + lid+i;

                items.Add(item);

            }

            test.Items = items;

            return test;

        }

        class TestItem {
            int i;

            public TestItem(int i) {
                this.i = i;
            }

            public string Id {
                get {

                    return "file:///test/search?id=" + Identifier;

                }
            }

            public string Identifier {
                get;
                set;
            }

            public string Name {
                get;
                set;
            }

            public string TextContent {
                get;
                set;
            }

            public DateTimeOffset Date {
                get {
                    return new DateTime(1900, 01, 01, 01, 01, 01).AddYears(100-i);
                }
            }
        }
    }
}
