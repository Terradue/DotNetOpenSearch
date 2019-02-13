using NUnit.Framework;
using System;
using System.Collections.Specialized;
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

    }
}
