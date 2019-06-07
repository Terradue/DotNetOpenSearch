using System;
using NUnit.Framework;
using Terradue.OpenSearch.Engine;
using System.Collections.Specialized;
using Terradue.OpenSearch.Schema;
using System.IO;
using System.Xml.Serialization;
using System.Web;
using System.Linq;
using Terradue.OpenSearch.Request;
using System.Xml;

namespace Terradue.OpenSearch.Test
{

    [TestFixture]
    public class TestOsddAttribute
    {

        [Test]
        public void TestFedeo()
        {

            XmlSerializer ser = new XmlSerializer(typeof(OpenSearchDescription));
            var osd = (OpenSearchDescription)ser.Deserialize(XmlReader.Create(new FileStream(Util.TestBaseDir + "/Samples/fedeo-osdd.xml", FileMode.Open, FileAccess.Read)));

            OpenSearchEngine ose = new OpenSearchEngine();
            ose.LoadPlugins();

			UrlBasedOpenSearchableFactory factory = new UrlBasedOpenSearchableFactory(new OpenSearchableFactorySettings(ose));

			IOpenSearchable os = factory.Create(osd);

            var url = new OpenSearchUrl("http://fedeo.esa.int/opensearch/request/?httpAccept=application/atom%2Bxml&parentIdentifier=urn:eop:DLR:EOWEB:Geohazard.Supersite.TerraSAR-X_SSC&startDate=2014-01-01T00:00:00Z&endDate=2015-04-20T00:00:00Z&recordSchema=om");
            NameValueCollection parameters = new NameValueCollection();
            parameters.Set("maximumRecords", "1");

            NameValueCollection nvc;
            if (url != null)
                nvc = HttpUtility.ParseQueryString(url.Query);
            else
                nvc = new NameValueCollection();
            
            parameters.AllKeys.SingleOrDefault(k =>
            {
                nvc.Set(k, parameters[k]);
                return false;
            });

            var request = OpenSearchRequest.Create(os, os.GetQuerySettings(ose), nvc);



        }
    }
}

