using System;
using NUnit.Framework;
using Terradue.OpenSearch.Engine;
using Mono.Addins;
using Terradue.OpenSearch.Result;
using Terradue.OpenSearch.Engine;

namespace Terradue.OpenSearch.RdfEO.Test {

    [TestFixture()]
    public class GenericTest {

        [Test()]
        public void GenericOpenSearchableTest() {

            AddinManager.Initialize();
            AddinManager.Registry.Update(null);

            OpenSearchEngine ose = new OpenSearchEngine();

            ose.LoadPlugins();
            OpenSearchUrl url = new OpenSearchUrl("https://challenges.esa.int/eceo/datapackage/FRSPAR/description?key=495f181f-47d3-4668-b717-d36d4a560837");
            IOpenSearchable entity = new GenericOpenSearchable(url, ose);

            var osr = ose.Query(entity, new System.Collections.Specialized.NameValueCollection(), "atom");

            OpenSearchFactory.ReplaceOpenSearchDescriptionLinks(entity, osr.Result);

            Assert.That(osr.Result.Links.Count > 0);

            Console.Out.Write(osr.Result.SerializeToString());

        }

        /*[Test()]
        public void GenericProxiedOpenSearchableTest() {

            AddinManager.Initialize();
            AddinManager.Registry.Update(null);

            OpenSearchEngine ose = new OpenSearchEngine();

            ose.LoadPlugins();
            OpenSearchUrl url = new OpenSearchUrl("http://127.0.0.1:8082/eceo/datapackage/DP_01/search?key=1bc95928-0387-4a55-a1a3-829e5bfe5947");
            IOpenSearchable entity = new GenericOpenSearchable(url, ose);

            var osr = ose.Query(entity, new System.Collections.Specialized.NameValueCollection(), "rdf");

            Assert.That(osr.Result.Links.Count > 0);

            Console.Out.Write(osr.Result.SerializeToString());

        }*/
    }
}

