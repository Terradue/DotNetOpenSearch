using System;
using System.Collections.Specialized;
using Terradue.OpenSearch.Engine;

namespace Terradue.OpenSearch
{
    public class OpenSearchableFactorySettings : System.ICloneable
    {

        public OpenSearchableFactorySettings(OpenSearchEngine ose){
            OpenSearchEngine = ose;
            Soft = false;
        }

        public OpenSearchEngine OpenSearchEngine;

        public bool Soft;

        public System.Net.ICredentials Credentials;

        /// value indicating the number of retries if the query fails
        public int MaxRetries;
        
        public object Clone()
        {
            return new OpenSearchableFactorySettings(OpenSearchEngine)
            {
                Soft = this.Soft,
                Credentials = this.Credentials,
                MergeFilters = this.MergeFilters,
                MaxRetries = this.MaxRetries
            };
        }

        public Func<NameValueCollection, NameValueCollection, NameValueCollection> MergeFilters;
    }
}