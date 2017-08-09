using System;
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

        public object Clone()
        {
            return new OpenSearchableFactorySettings(OpenSearchEngine)
            {
                Soft = this.Soft,
                Credentials = this.Credentials
            };
        }
    }
}