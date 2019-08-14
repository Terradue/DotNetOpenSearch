using System;
using System.Collections.Specialized;
using Terradue.OpenSearch.Engine;
using Xunit;

namespace Terradue.OpenSearch.Test
{
    public class DiscoveryTests
    {
		[Fact(DisplayName = "EOCLOUD finder")]
        [Trait("Category", "unit")]
        public void FinderEoCloud()
        {

            OpenSearchEngine ose = new OpenSearchEngine();
            ose.LoadPlugins();

            var settings = new OpenSearchableFactorySettings(ose);
            UrlBasedOpenSearchableFactory factory = new UrlBasedOpenSearchableFactory(settings);

			var os = OpenSearchFactory.FindOpenSearchable(settings, new Uri("http://finder.eocloud.eu/resto/api/collections/describe.xml"));

			//var results = ose.Query(os, new NameValueCollection());

			//results.SerializeToString();

        }

        //[Test()]
        //public void CKANOSDD()
        //{

        //    OpenSearchEngine ose = new OpenSearchEngine();
        //    ose.LoadPlugins();

        //    var settings = new OpenSearchableFactorySettings(ose);
        //    UrlBasedOpenSearchableFactory factory = new UrlBasedOpenSearchableFactory(settings);

        //    var os = OpenSearchFactory.FindOpenSearchable(settings, new Uri("https://catalogue.nextgeoss.eu/opensearch/description.xml?osdd=dataset"), "application/atom+xml");

        //    //var results = ose.Query(os, new NameValueCollection());

        //    //results.SerializeToString();

        //}
    }
}
