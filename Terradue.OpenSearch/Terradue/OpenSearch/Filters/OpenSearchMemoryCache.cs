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
            request = new CachedOpenSearchRequest(item.OpenSearchUrl, item.OpenSearchResponse, request.OriginalParameters, watch.Elapsed);

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
            
            OpenSearchResponseCacheItem item = new OpenSearchResponseCacheItem(request.OpenSearchUrl, clonedResponse);
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
    }

    /// <summary>
    /// Open search response cache item.
    /// </summary>
    public class OpenSearchResponseCacheItem : CacheItem {

        public OpenSearchResponseCacheItem(OpenSearchUrl url, IOpenSearchResponse clonedResponse) : base(url.ToString(),clonedResponse) {

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

        public CachedOpenSearchRequest(OpenSearchUrl url, IOpenSearchResponse response, NameValueCollection originalParameters, TimeSpan elapsed) : base(url, response.ContentType) {
            base.OpenSearchUrl = url;
            this.response = response;
            this.OriginalParameters = originalParameters;
        }

        #region implemented abstract members of OpenSearchRequest

        public override IOpenSearchResponse GetResponse() {
            return response;
        }

        NameValueCollection originalParameters;
        public override NameValueCollection OriginalParameters {
            get {
                return originalParameters;
            }
            set {
                originalParameters = value;
            }
        }

        #endregion
    }

    public class OpenSearchableChangeMonitor : ChangeMonitor {

        private String _uniqueId;
        private IMonitoredOpenSearchable entity;

        NameValueCollection parameters;

        string contentType;

        public OpenSearchableChangeMonitor(IMonitoredOpenSearchable entity, OpenSearchRequest request) {
            this.contentType = request.ContentType;
            this.parameters = request.OriginalParameters;
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

