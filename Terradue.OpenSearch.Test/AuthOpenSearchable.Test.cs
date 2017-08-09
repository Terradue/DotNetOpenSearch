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

			UrlBasedOpenSearchableFactory factory = new UrlBasedOpenSearchableFactory(new OpenSearchableFactorySettings(ose));

            factory.Settings.Credentials = new System.Net.NetworkCredential("demo1", "Demo199++");

            IOpenSearchable entity1 = factory.Create(new OpenSearchUrl("https://catalog.terradue.com/demo1/search"));

            NameValueCollection nvc = new NameValueCollection();

            var osr = ose.Query(entity1, nvc, "atom");

            nvc = new NameValueCollection();
            nvc.Set("count", "10");

            osr = ose.Query(entity1, nvc, "atom");


        }


    }
}

