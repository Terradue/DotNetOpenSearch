using System;
using NUnit.Framework;
using Terradue.OpenSearch.Engine;
using System.Collections.Specialized;

namespace Terradue.OpenSearch.Test {

    [TestFixture]
    public class TestAtom {

//        [Test]
        public void TestAtom1() {


            OpenSearchEngine ose = new OpenSearchEngine();
            ose.LoadPlugins();

            IOpenSearchable entity = ose.Create(new OpenSearchUrl("https://ngeopro.magellium.fr/remote/S1_SAR_IW_SINGLE_POL/description"));
            NameValueCollection nvc = new NameValueCollection();

            var osr = ose.Query(entity, nvc, "atom");

        }
    }
}

