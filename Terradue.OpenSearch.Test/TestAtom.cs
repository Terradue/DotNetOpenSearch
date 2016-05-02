using System;
using NUnit.Framework;
using Mono.Addins;
using Terradue.OpenSearch.Engine;
using System.Collections.Specialized;

namespace Terradue.OpenSearch.Test {

    [TestFixture]
    public class TestAtom {

//        [Test]
        public void TestAtom1() {


            AddinManager.Initialize();
            AddinManager.Registry.Update(null);

            OpenSearchEngine ose = new OpenSearchEngine();
            ose.LoadPlugins();

            GenericOpenSearchable entity = new GenericOpenSearchable(new OpenSearchUrl("https://ngeopro.magellium.fr/remote/S1_SAR_IW_SINGLE_POL/description"), ose);
            NameValueCollection nvc = new NameValueCollection();

            var osr = ose.Query(entity, nvc, "atom");

        }
    }
}

