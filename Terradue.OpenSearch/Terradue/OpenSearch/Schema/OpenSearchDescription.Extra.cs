using System;
using Terradue.OpenSearch.Schema;
using System.Collections.Specialized;
using System.Web;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml.Serialization;


namespace Terradue.OpenSearch.Schema
{
    public partial class OpenSearchDescription
	{
        OpenSearchDescriptionUrl defaultUrl = null;

        public OpenSearchDescription(){
            ExtraNamespace.Add("os", "http://a9.com/-/spec/opensearch/1.1/");
        }

        [System.Xml.Serialization.XmlIgnore]
        public OpenSearchDescriptionUrl DefaultUrl {
            get {
                if (defaultUrl == null && Url.Count() > 0)
                    return Url[0];
                return defaultUrl;
            }
            set {
                defaultUrl = value;
            }
        }

        [System.Xml.Serialization.XmlIgnore]
        public string[] ContentTypes {
            get {
                return Url.Select<OpenSearchDescriptionUrl, string>(u => u.Type).ToArray(); 
            }
        }

        [System.Xml.Serialization.XmlIgnore]
        public OpenSearchUrl Originator { get; set; }

	}

    public partial class OpenSearchDescriptionUrl
    {

        public NameValueCollection GetIdentifierDictionary() {

            var nvc = HttpUtility.ParseQueryString(new Uri(this.Template).Query);
            return OpenSearchFactory.ReverseTemplateOpenSearchParameters(nvc);

        }

    }
}

