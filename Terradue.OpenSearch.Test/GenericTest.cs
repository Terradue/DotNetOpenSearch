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
            OpenSearchUrl url = new OpenSearchUrl("http://catalogue.terradue.int/catalogue/search/MER_FRS_1P/rdf?startIndex=0&q=MER_FRS_1P&start=1992-01-01&stop=2014-10-24&bbox=-72,47,-57,58");
            IOpenSearchable entity = new GenericOpenSearchable(url, ose);

            var osr = ose.Query(entity, new System.Collections.Specialized.NameValueCollection(), "rdf");

            Assert.That(osr.Result.Links.Count > 0);

        }
    }
}

