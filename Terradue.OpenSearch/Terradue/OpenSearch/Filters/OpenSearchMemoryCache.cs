//
//  OpenSearchMemoryCache.cs
//
//  Author:
//       Emmanuel Mathot <emmanuel.mathot@terradue.com>
//
//  Copyright (c) 2014 Terradue

using System;
using System.Linq;
using System.Runtime.Caching;
using System.Collections.Specialized;
using System.IO;
using System.Diagnostics;
using Terradue.OpenSearch.Request;
using Terradue.OpenSearch.Response;

namespace Terradue.OpenSearch.Filters {

    /// <summary>
    /// Class that implements a cache for OpenSearch Request and Response.
    /// </summary>
    public class OpenSearchMemoryCache {

		private MemoryCache cache;
        private NameValueCollection config;

        /// <summary>
        /// Initializes a new instance of the <see cref="Terradue.OpenSearch.Filters.OpenSearchMemoryCache"/> class.
        /// </summary>
        /// <param name="name">Name.</param>
        /// <param name="config">Config.</param>
		public OpenSearchMemoryCache(string name, NameValueCollection config) {

            this.config = config;
			cache = new MemoryCache(name,config);

        }

        /// <summary>
        /// Tries the replace with cache request.
        /// </summary>
        /// <param name="request">Request.</param>
        public void TryReplaceWithCacheRequest(ref OpenSearchRequest request){

            Stopwatch watch = new Stopwatch();
            watch.Start();
            CacheItem it = cache.GetCacheItem(request.OpenSearchUrl.ToString());
          
            if (it == null) return;

            OpenSearchResponseCacheItem item = new OpenSearchResponseCacheItem(it);
            watch.Stop();

			request = new CachedOpenSearchRequest(item.OpenSearchUrl,item.OpenSearchResponse, request.OriginalParameters, watch.Elapsed);

		}

        /// <summary>
        /// Caches the response.
        /// </summary>
        /// <param name="request">Request.</param>
        /// <param name="response">Response.</param>
        public void CacheResponse(OpenSearchRequest request, ref IOpenSearchResponse response){

            /*if (response.GetType() != typeof(CachedOpenSearchResponse)) {
                response = new CachedOpenSearchResponse(response);
            }*/

            OpenSearchResponseCacheItem item = new OpenSearchResponseCacheItem(request.OpenSearchUrl, response);
            CacheItemPolicy policy = this.CreatePolicy(item);
            cache.Set(item, policy);

		}

        /// <summary>
        /// Creates the policy.
        /// </summary>
        /// <returns>The policy.</returns>
        /// <param name="item">Item.</param>
        protected CacheItemPolicy CreatePolicy (OpenSearchResponseCacheItem item) {
            CacheItemPolicy policy = new CacheItemPolicy();
            policy.SlidingExpiration = TimeSpan.FromSeconds(double.Parse(config["SlidingExpiration"]));
            return policy;
        }
    }

    /// <summary>
    /// Open search response cache item.
    /// </summary>
	public class OpenSearchResponseCacheItem : CacheItem {

        public OpenSearchResponseCacheItem(OpenSearchUrl url, IOpenSearchResponse response): base(url.ToString(), response.CloneForCache()){

		}

        internal OpenSearchResponseCacheItem(CacheItem item): base(item.Key, item.Value){

        }

		public OpenSearchUrl OpenSearchUrl{
			get {
				return new OpenSearchUrl(base.Key);
			}
		}

		public IOpenSearchResponse OpenSearchResponse{
			get {
				return (IOpenSearchResponse)base.Value;
			}
		}

	}

    /// <summary>
    /// Cached open search request.
    /// </summary>
	public class CachedOpenSearchRequest : OpenSearchRequest {

        private IOpenSearchResponse response;

		public CachedOpenSearchRequest(OpenSearchUrl url, IOpenSearchResponse response, NameValueCollection originalParameters, TimeSpan elapsed) :base(url) {
			base.OpenSearchUrl = url;
            this.response = response;
			base.OriginalParameters = originalParameters;
		}

		#region implemented abstract members of OpenSearchRequest
		public override IOpenSearchResponse GetResponse() {
			return response;
		}
		#endregion
	}

    /// <summary>
    /// Cached open search response.
    /// </summary>
    /*public class CachedOpenSearchResponse : IOpenSearchResponse {

        private object cachedResponse;
        TimeSpan requestTime;
        string contentType;
        readonly Type objectType;
        IOpenSearchable entity;

        public CachedOpenSearchResponse(IOpenSearchResponse response) : this(response, response.RequestTime) {
        }
            
        public CachedOpenSearchResponse(IOpenSearchResponse response, TimeSpan elapsed) {

            cachedResponse = response.GetResponseObject();
            requestTime = elapsed;
            contentType = response.ContentType;
            objectType = response.ObjectType;
            entity = response.Entity;

        }

        #region implemented abstract members of OpenSearchResponse

        public object GetResponseObject() {
            return cachedResponse;
        }

        public string ContentType {
            get {
                return contentType;
            }
        }

        public TimeSpan RequestTime {
            get {
                return requestTime;
            }
        }

        public Type ObjectType {
            get {
                return objectType;
            }
        }

        public IOpenSearchable Entity {
            get {
                return entity;
            }
            set {
                entity = value;
            }
        }
        #endregion
    }*/

}

