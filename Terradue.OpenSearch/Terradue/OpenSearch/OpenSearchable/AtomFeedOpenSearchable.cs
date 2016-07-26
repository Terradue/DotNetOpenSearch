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

namespace Terradue.OpenSearch
{
	public class AtomFeedOpenSearchable : IOpenSearchable
	{
		readonly AtomFeed feed;

		public AtomFeedOpenSearchable(AtomFeed feed)
		{
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

		public void ApplyResultFilters(OpenSearchRequest request, ref IOpenSearchResultCollection osr, string finalContentType)
		{

		}

		public OpenSearchRequest Create(string mimetype, NameValueCollection parameters)
		{
			UriBuilder url = new UriBuilder("dummy://localhost");
			url.Path += Identifier + "/search";
			var array = (from key in parameters.AllKeys
						 from value in parameters.GetValues(key)
						 select string.Format("{0}={1}", HttpUtility.UrlEncode(key), HttpUtility.UrlEncode(value)))
				.ToArray();
			url.Query = string.Join("&", array);

			AtomOpenSearchRequest request = new AtomOpenSearchRequest(new OpenSearchUrl(url.Uri), SearchInAtom);

			return request;
		}

		public OpenSearchDescription GetOpenSearchDescription()
		{
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

		public NameValueCollection GetOpenSearchParameters(string mimeType)
		{
			return OpenSearchFactory.GetBaseOpenSearchParameter();
		}

		public QuerySettings GetQuerySettings(OpenSearchEngine ose)
		{
			IOpenSearchEngineExtension osee = new AtomOpenSearchEngineExtension();
			return new QuerySettings(osee.DiscoveryContentType, osee.ReadNative);
		}

		AtomFeed SearchInAtom(NameValueCollection parameters)
		{
            var alternate = (feed != null && feed.Links != null) ? feed.Links.FirstOrDefault(l => l.RelationshipType == "alternate") : null;
            if(alternate == null) alternate = feed.Links.FirstOrDefault(l => l.RelationshipType == "self");
			
			string[] queryString = Array.ConvertAll(parameters.AllKeys, key => String.Format("{0}={1}", key, parameters[key]));

			AtomFeed resultfeed = new AtomFeed("Discovery feed for " + this.Identifier,
										 "This OpenSearch Service allows the discovery of the different items which are part of the " + this.Identifier + " collection" +
										 "This search service is in accordance with the OGC 10-032r3 specification.",
                                               alternate != null ? alternate.Uri : new Uri(feed.Id) , feed.Id, DateTimeOffset.UtcNow);

			resultfeed.Generator = "Terradue Web Server";

			// Load all avaialable Datasets according to the context

			PaginatedList<AtomItem> pds = new PaginatedList<AtomItem>();

			int startIndex = 1;
			if (!string.IsNullOrEmpty(parameters["startIndex"]))
				startIndex = int.Parse(parameters["startIndex"]);

			pds.AddRange(SearchInItem((IEnumerable < AtomItem > )feed.Items, parameters));

			pds.PageNo = 1;
			if (!string.IsNullOrEmpty(parameters["startPage"]))
				pds.PageNo = int.Parse(parameters["startPage"]);

			pds.PageSize = 20;
			if (!string.IsNullOrEmpty(parameters["count"]))
				pds.PageSize = int.Parse(parameters["count"]);

			pds.StartIndex = startIndex - 1;

			if (this.Identifier != null)
				resultfeed.Identifier = this.Identifier;

			resultfeed.Items = pds.GetCurrentPage();
			resultfeed.TotalResults = pds.Count(); ;

			return resultfeed;
		}

		IEnumerable<AtomItem> SearchInItem(IEnumerable<AtomItem> items, NameValueCollection parameters)
		{
			return items.Where(i => {

                if (!string.IsNullOrEmpty(parameters["uid"])) {
                    if (i.Identifier.Equals(parameters["uid"])) return true;
                    return false;
                }

				if (!string.IsNullOrEmpty(parameters["q"])) {
					if (i.Identifier.Contains(parameters["q"])) return true;
					if (i.Title != null && i.Title.Text != null && i.Title.Text.Contains(parameters["q"])) return true;
					if (i.Summary != null && i.Summary.Text != null && i.Summary.Text.Contains(parameters["q"])) return true;
					return false;
				}

				return true;

			});
		}
	}
}

