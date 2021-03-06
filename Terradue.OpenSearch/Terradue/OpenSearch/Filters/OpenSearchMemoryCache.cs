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
using Terradue.OpenSearch.Request;
using Terradue.OpenSearch.Response;
using System.Text.RegularExpressions;
using Terradue.OpenSearch.Schema;
using System.Diagnostics;

namespace Terradue.OpenSearch.Filters
{

    /// <summary>
    /// Class that implements a cache for OpenSearch Request and Response.
    /// </summary>
    public class OpenSearchMemoryCache {

        private log4net.ILog log = log4net.LogManager.GetLogger
            (System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private MemoryCache cache;

        /// <summary>
        /// Initializes a new instance of the <see cref="Terradue.OpenSearch.Filters.OpenSearchMemoryCache"/> class.
        /// </summary>
        /// <param name="name">Name.</param>
        /// <param name="config">Config.</param>
        public OpenSearchMemoryCache(string name, NameValueCollection config = null) {

            cache = new MemoryCache(name, config);

        }

        public OpenSearchMemoryCache() {
            
            cache = new MemoryCache("opensearch");

        }

        /// <summary>
        /// Tries the replace with cache request.
        /// </summary>
        /// <param name="request">Request.</param>
        public void TryReplaceWithCacheRequest(ref OpenSearchRequest request) {

            Stopwatch watch = new Stopwatch();
            watch.Start();
            CacheItem it = cache.GetCacheItem(request.OpenSearchUrl.ToString());
          
            if (it == null) return;

            OpenSearchResponseCacheItem item = new OpenSearchResponseCacheItem(it);
            watch.Stop();

            log.DebugFormat("OpenSearch Cache [load] {0}", request.OpenSearchUrl);
			request = new CachedOpenSearchRequest(item.OpenSearchUrl, item.OpenSearchResponse, request.OriginalParameters, watch.Elapsed, item.OpenSearchDescription);

        }

        /// <summary>
        /// Caches the response.
        /// </summary>
        /// <param name="request">Request.</param>
        /// <param name="response">Response.</param>
        public void CacheResponse(OpenSearchRequest request, ref IOpenSearchResponse response) {

            CacheItem it = cache.GetCacheItem(request.OpenSearchUrl.ToString());

                if (it != null) return;

            if (response.Entity != null && !response.Entity.CanCache)
                return;

               
            var clonedResponse = response.CloneForCache();

            if (clonedResponse == null)
                throw new InvalidOperationException(string.Format("Response cannot be cached because it is null. Check the CloneForCache of the response [{0}] or the CanCache() method of the Opensearchable [{1}] requested", response.GetType(), response.Entity.GetType()));
            
			OpenSearchResponseCacheItem item = new OpenSearchResponseCacheItem(request.OpenSearchUrl, clonedResponse, request.OpenSearchDescription);
            CacheItemPolicy policy = this.CreatePolicy(item, request);
            log.DebugFormat("OpenSearch Cache [store] {0}", request.OpenSearchUrl);

            cache.Set(item, policy);

            log.DebugFormat("OpenSearch Cache [count] {0}", cache.GetCount());

        }

        /// <summary>
        /// Creates the policy.
        /// </summary>
        /// <returns>The policy.</returns>
        /// <param name="item">Item.</param>
        protected CacheItemPolicy CreatePolicy(OpenSearchResponseCacheItem item, OpenSearchRequest request) {
            CacheItemPolicy policy = new CacheItemPolicy();
            policy.AbsoluteExpiration = DateTime.UtcNow.Add(item.OpenSearchResponse.Validity);
            log.DebugFormat("OpenSearch Cache [prepare to store] AbsoluteExpiration {1} {0}", item.OpenSearchUrl, policy.AbsoluteExpiration);
            policy.RemovedCallback = new CacheEntryRemovedCallback(this.EntryRemovedCallBack);
            if (item.OpenSearchResponse.Entity is IMonitoredOpenSearchable) {
                IMonitoredOpenSearchable mos = (IMonitoredOpenSearchable)item.OpenSearchResponse.Entity;
                var monitor = new OpenSearchableChangeMonitor(mos, request);
                log.DebugFormat("OpenSearch Cache [prepare to store] Monitor {1} {0}  ", item.OpenSearchUrl, monitor.UniqueId);
                policy.ChangeMonitors.Add(monitor);
            }
            return policy;
        }

        public void EntryRemovedCallBack(CacheEntryRemovedArguments arguments){
            if ( arguments.CacheItem is OpenSearchResponseCacheItem ){
                OpenSearchResponseCacheItem item = new OpenSearchResponseCacheItem(arguments.CacheItem);
                log.DebugFormat("OpenSearch Cache [remove] reason {1} {0}", item.OpenSearchUrl, arguments.RemovedReason);
            }
        }

        public void ClearCache(string pattern, DateTime since)
		{
			Regex regex = new Regex(pattern);
			foreach (var k in cache.Where(i => regex.IsMatch(i.Key))) {
                var cacheItem = cache.GetCacheItem(k.Key);
                if (cacheItem != null)
                {
                    IOpenSearchResponse response = (IOpenSearchResponse)cacheItem.Value;
                    if (response.Created < since)
                        cache.Remove(k.Key);
                }
			}
		}

    }

    public delegate void OpenSearchableChangeEventHandler(object sender, OnOpenSearchableChangeEventArgs e);

    public interface IMonitoredOpenSearchable : IOpenSearchable {

        void OnOpenSearchableChange (object sender,  OnOpenSearchableChangeEventArgs data);

        event OpenSearchableChangeEventHandler OpenSearchableChange;

    }

    public class OnOpenSearchableChangeEventArgs: EventArgs {
        public object State { get; internal set; }

        public OnOpenSearchableChangeEventArgs(object state) {
            this.State = state;
        }
    }

}

