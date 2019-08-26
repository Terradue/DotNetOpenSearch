//
//  OpenSearchMemoryCache.cs
//
//  Author:
//       Emmanuel Mathot <emmanuel.mathot@terradue.com>
//
//  Copyright (c) 2014 Terradue

using System;
using System.Runtime.Caching;
using Terradue.OpenSearch.Response;
using Terradue.OpenSearch.Schema;

namespace Terradue.OpenSearch.Filters
{
    /// <summary>
    /// Open search response cache item.
    /// </summary>
    public class OpenSearchResponseCacheItem : CacheItem {
  
        private DateTime created;

		public OpenSearchResponseCacheItem(OpenSearchUrl url, IOpenSearchResponse clonedResponse, OpenSearchDescription osd) : base(url.ToString(),clonedResponse) {
            created = DateTime.UtcNow;
			this.OpenSearchDescription = osd;
        }

        internal OpenSearchResponseCacheItem(CacheItem item) : base(item.Key, item.Value) {
            created = DateTime.UtcNow;
        }

        public OpenSearchUrl OpenSearchUrl {
            get {
                return new OpenSearchUrl(base.Key);
            }
        }

        public IOpenSearchResponse OpenSearchResponse {
            get {
                return (IOpenSearchResponse)base.Value;
            }
        }

        public DateTime Created
        {
            get
            {
                return Created;
            }
        }

		public OpenSearchDescription OpenSearchDescription
        {
            get; protected set;
        }
    }

}

