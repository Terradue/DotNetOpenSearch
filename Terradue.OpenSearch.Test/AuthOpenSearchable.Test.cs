using System;
using NUnit.Framework;
using Terradue.OpenSearch.Engine;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using log4net.Config;
using System.IO;

namespace Terradue.OpenSearch.Test {

    [TestFixture]
    public class AuthOpenSearchableTest {

        [SetUp]
        public void RunBeforeTests()
        {
            XmlConfigurator.Configure(new FileInfo("../Log4Net.config"));
        }


        [Test()]
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

