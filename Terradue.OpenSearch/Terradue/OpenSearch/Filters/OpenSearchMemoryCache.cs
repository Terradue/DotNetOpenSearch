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
            cache = new MemoryCache(name, config);

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

            request = new CachedOpenSearchRequest(item.OpenSearchUrl, item.OpenSearchResponse, request.OriginalParameters, watch.Elapsed);

        }

        /// <summary>
        /// Caches the response.
        /// </summary>
        /// <param name="request">Request.</param>
        /// <param name="response">Response.</param>
        public void CacheResponse(OpenSearchRequest request, ref IOpenSearchResponse response) {

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
        protected CacheItemPolicy CreatePolicy(OpenSearchResponseCacheItem item) {
            CacheItemPolicy policy = new CacheItemPolicy();
            policy.SlidingExpiration = TimeSpan.FromSeconds(double.Parse(config["SlidingExpiration"]));
            if (item.OpenSearchResponse.Entity is IMonitoredOpenSearchable) {
                IMonitoredOpenSearchable mos = (IMonitoredOpenSearchable)item.OpenSearchResponse.Entity;
                policy.ChangeMonitors.Add(new OpenSearchableChangeMonitor(mos));
            }
            return policy;
        }
    }

    /// <summary>
    /// Open search response cache item.
    /// </summary>
    public class OpenSearchResponseCacheItem : CacheItem {

        public OpenSearchResponseCacheItem(OpenSearchUrl url, IOpenSearchResponse response) : base(url.ToString(), response.CloneForCache()) {

        }

        internal OpenSearchResponseCacheItem(CacheItem item) : base(item.Key, item.Value) {

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

    }

    /// <summary>
    /// Cached open search request.
    /// </summary>
    public class CachedOpenSearchRequest : OpenSearchRequest {

        private IOpenSearchResponse response;

        public CachedOpenSearchRequest(OpenSearchUrl url, IOpenSearchResponse response, NameValueCollection originalParameters, TimeSpan elapsed) : base(url) {
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

    public class OpenSearchableChangeMonitor : ChangeMonitor {

        private String _uniqueId;
        private IMonitoredOpenSearchable entity;

        public OpenSearchableChangeMonitor(IMonitoredOpenSearchable entity) {
            this.entity = entity;
            InitDisposableMembers();
        }

        private void InitDisposableMembers() {
            bool dispose = true;
            try {
                string uniqueId = null;
                    
                uniqueId = entity.GetOpenSearchDescription().Url.FirstOrDefault(u => u.Relation == "self").Template;
                entity.OpenSearchableChange += new OpenSearchableChangeEventHandler(OnOpenSearchableChanged);

                _uniqueId = uniqueId;
                dispose = false;
            } finally {
                InitializationComplete();
                if (dispose) {
                    Dispose();
                }
            }
        }

        private void OnOpenSearchableChanged(Object sender, OnOpenSearchableChangeEventArgs e) {
            OnChanged(e.State);
        }

        #region implemented abstract members of ChangeMonitor
        protected override void Dispose(bool disposing) {}

        public override string UniqueId {
            get {
                return _uniqueId;
            }
        }
        #endregion
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

