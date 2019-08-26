using System;
using Terradue.OpenSearch.Engine;
using System.Collections.Specialized;
using Xunit;

namespace Terradue.OpenSearch.Test
{
    public class AuthOpenSearchableTest : IClassFixture<TestFixture>
    {

        [Fact(DisplayName = "HTTP Network Credentials test")]
        [Trait("Category", "unit")]
        public void NetworkCredentialHttp()
        {

            OpenSearchEngine ose = new OpenSearchEngine();
            ose.LoadPlugins();

            var settings = new OpenSearchableFactorySettings(ose);
            settings.Credentials = new System.Net.NetworkCredential("demo1", "Demo199++");
            UrlBasedOpenSearchableFactory factory = new UrlBasedOpenSearchableFactory(settings);

            IOpenSearchable entity1 = factory.Create(new OpenSearchUrl("https://catalog.terradue.com/demo1/search"));

            NameValueCollection nvc = new NameValueCollection();

            var osr = ose.Query(entity1, nvc, "atom");

            nvc = new NameValueCollection();
            nvc.Set("count", "10");

            osr = ose.Query(entity1, nvc, "atom");

            OpenSearchFactory.FindOpenSearchable(settings, new Uri("https://catalog.terradue.com/demo1/search"), null);


        }

        [Fact(DisplayName = "HTTP Network Credentials with special character test")]
        public void SpecialCharCredentialHttp()
        {

            OpenSearchEngine ose = new OpenSearchEngine();
            ose.LoadPlugins();

            var settings = new OpenSearchableFactorySettings(ose);
            settings.Credentials = new System.Net.NetworkCredential("eod.exp@gmail.com", "fred1960");
            UrlBasedOpenSearchableFactory factory = new UrlBasedOpenSearchableFactory(settings);

            OpenSearchFactory.FindOpenSearchable(settings, new Uri("https://finder.creodias.eu/resto/api/collections/describe.xml"), null);

        }

        public void NetworkCredentialRedirectedHttp()
        {

            OpenSearchEngine ose = new OpenSearchEngine();
            ose.LoadPlugins();

            var settings = new OpenSearchableFactorySettings(ose);
            settings.Credentials = new System.Net.NetworkCredential("demo1", "Demo199++");
            UrlBasedOpenSearchableFactory factory = new UrlBasedOpenSearchableFactory(settings);

            IOpenSearchable entity1 = factory.Create(new OpenSearchUrl("https://catalog.terradue.com/demo1/search"));

            NameValueCollection nvc = new NameValueCollection();

            var osr = ose.Query(entity1, nvc, "atom");

            nvc = new NameValueCollection();
            nvc.Set("count", "10");

            osr = ose.Query(entity1, nvc, "atom");

            OpenSearchFactory.FindOpenSearchable(settings, new Uri("https://catalog.terradue.com/demo1/search"), null);

        }



    }
}

