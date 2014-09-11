using System;
using Terradue.OpenSearch.Schema;
using System.Collections.Specialized;
using System.Web;
using System.Linq;


namespace Terradue.OpenSearch.Schema
{
	public partial class OpenSearchDescription
	{
		
        OpenSearchDescriptionUrl defaultUrl = null;
        public OpenSearchDescriptionUrl DefaultUrl {
            get {
                if (defaultUrl == null) {
                    if (this.Url.Length > 0)
                        return this.Url[0];
                }
                return defaultUrl;
            }
            set {
                defaultUrl = value;
            }
        }

        public string[] ContentTypes {
            get {
                return Url.Select<OpenSearchDescriptionUrl, string>(u => u.Type).ToArray(); 
            }
        }

	}

    public partial class OpenSearchDescriptionUrl
    {

        public NameValueCollection GetIdentifierDictionary() {

            var nvc = HttpUtility.ParseQueryString(new Uri(this.Template).Query);
            return OpenSearchFactory.ReverseTemplateOpenSearchParameters(nvc);

        }

    }
}

