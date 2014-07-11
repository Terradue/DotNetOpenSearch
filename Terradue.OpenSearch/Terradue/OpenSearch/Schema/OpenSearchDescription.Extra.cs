using System;
using Terradue.OpenSearch.Schema;
using System.Collections.Specialized;
using System.Web;


namespace Terradue.OpenSearch.Schema
{
	public partial class OpenSearchDescription
	{
		
        public OpenSearchDescriptionUrl DefaultUrl { get; set; }

	}

    public partial class OpenSearchDescriptionUrl
    {

        public NameValueCollection GetIdentifierDictionary() {

            var nvc = HttpUtility.ParseQueryString(new Uri(this.Template).Query);
            return OpenSearchFactory.ReverseTemplateOpenSearchParameters(nvc);

        }

    }
}

