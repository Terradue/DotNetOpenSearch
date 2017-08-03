using Terradue.OpenSearch.Engine;

namespace Terradue.OpenSearch
{
    public class OpenSearchableFactorySettings
    {

        public OpenSearchableFactorySettings(OpenSearchEngine ose){
            OpenSearchEngine = ose;
            Soft = false;
        }

        public OpenSearchEngine OpenSearchEngine;

        public bool Soft;

        public System.Net.ICredentials Credentials;

    }
}