//
//  FileOpenSearchRequest.cs
//
//  Author:
//       Emmanuel Mathot <emmanuel.mathot@terradue.com>
//
//  Copyright (c) 2014 Terradue

using System;
using System.Collections.Specialized;
using System.IO;
using System.Web;
using Terradue.OpenSearch.Response;
using Terradue.OpenSearch.Result;
using System.Diagnostics;

namespace Terradue.OpenSearch.Request
{
    /// <summary>
    /// OpenSearchRequest in memory that doesnot actually performs a request but is a container
    /// for the response
    /// </summary>
    public class AtomOpenSearchRequest : OpenSearchRequest
	{

		NameValueCollection parameters;
        Func<NameValueCollection, AtomFeed> feedGenerator;


        /// <summary>
        /// Initializes a new instance of the <see cref="Terradue.OpenSearch.Request.MemoryOpenSearchRequest"/> class.
        /// </summary>
        /// <param name="url">URL.</param>
        /// <param name="contentType">Content type.</param>
        public AtomOpenSearchRequest(OpenSearchUrl url, Func<NameValueCollection, AtomFeed> FeedGenerator) : base(url, "application/atom+xml"){
            this.feedGenerator = FeedGenerator;
            this.parameters = HttpUtility.ParseQueryString(url.Query);
		}

		#region implemented abstract members of OpenSearchRequest

		public override IOpenSearchResponse GetResponse() {
            Stopwatch sw = Stopwatch.StartNew();
            var feed = feedGenerator(parameters);
            sw.Stop();
            return new AtomOpenSearchResponse(feed, sw.Elapsed);
		}

        public override OpenSearchUrl OpenSearchUrl {
            get {
                return base.OpenSearchUrl;
            }
        }

        public override NameValueCollection OriginalParameters {
            get {
                return parameters;
            }
            set {
                parameters = value;
            }
        }

		#endregion

	}
}


