using NUnit.Framework;
using System;
using System.Collections.Specialized;
using System.Linq;
using Terradue.OpenSearch.Engine;

namespace Terradue.OpenSearch.Test
{
    [TestFixture()]
    public class QueryTests
    {
		[Test()]
        public void CountTest()
        {

            OpenSearchEngine ose = new OpenSearchEngine();
            ose.LoadPlugins();

            var settings = new OpenSearchableFactorySettings(ose);
            UrlBasedOpenSearchableFactory factory = new UrlBasedOpenSearchableFactory(settings);

			var os = OpenSearchFactory.FindOpenSearchable(settings, new Uri("https://catalog.terradue.com/sentinel1/description"));

            var parameters = new NameValueCollection();

            parameters.Set("{http://a9.com/-/spec/opensearch/1.1/}count", "2");

			var results = ose.Query(os, parameters);

            Assert.AreEqual(2, results.Count);

        }

        [Test()]
        public void UidTest()
        {

            OpenSearchEngine ose = new OpenSearchEngine();
            ose.LoadPlugins();

            var settings = new OpenSearchableFactorySettings(ose);
            UrlBasedOpenSearchableFactory factory = new UrlBasedOpenSearchableFactory(settings);

            var os = OpenSearchFactory.FindOpenSearchable(settings, new Uri("https://catalog.terradue.com//sentinel1/series/GRD/search?format=atom&uid=S1A_IW_GRDH_1SDV_20160719T181151_20160719T181219_012221_012F93_3E0B"), null);

            var parameters = new NameValueCollection();

            var results = ose.Query(os, parameters);

            Assert.AreEqual(1, results.Count);

            Assert.AreEqual("S1A_IW_GRDH_1SDV_20160719T181151_20160719T181219_012221_012F93_3E0B", results.Items.First().Identifier);

            results.ToString();

        }



    }
}
