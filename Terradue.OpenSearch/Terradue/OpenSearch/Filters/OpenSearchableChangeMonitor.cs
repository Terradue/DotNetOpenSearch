//
//  OpenSearchMemoryCache.cs
//
//  Author:
//       Emmanuel Mathot <emmanuel.mathot@terradue.com>
//
//  Copyright (c) 2014 Terradue

using System;
using System.Linq;
using System.Collections.Specialized;
using System.Runtime.Caching;
using Terradue.OpenSearch.Request;

namespace Terradue.OpenSearch.Filters
{
    public class OpenSearchableChangeMonitor : ChangeMonitor {

        private string _uniqueId;
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

}

