using System;
using NUnit.Framework;
using Mono.Addins;
using Terradue.OpenSearch.Engine;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace Terradue.OpenSearch.Test {

    [TestFixture]
    public class MultiGenericOpenSearchableTest {

        [Test()]
        public void GenericProxiedOpenSearchableTest() {

            AddinManager.Initialize();
            AddinManager.Registry.Update(null);

            OpenSearchEngine ose = new OpenSearchEngine();
            ose.LoadPlugins();

            OpenSearchUrl url = new OpenSearchUrl("http://catalogue.terradue.int/catalogue/search/MER_FRS_1P/rdf?startIndex=0&q=MER_FRS_1P&start=1992-01-01&stop=2014-10-24&bbox=-72,47,-57,58");
            IOpenSearchable entity = new GenericOpenSearchable(url, ose);

            List<IOpenSearchable> entities = new List<IOpenSearchable>();
            entities.Add(entity);

            IOpenSearchable multiEntity = new MultiGenericOpenSearchable(entities, ose, true);

            NameValueCollection nvc = new NameValueCollection();
            nvc.Set("count", "100");

            var osr = ose.Query(multiEntity, nvc, "rdf");

            Assert.That(osr.Result.Links.Count > 0);

            Console.Out.Write(osr.Result.SerializeToString());

        }
    }
}

