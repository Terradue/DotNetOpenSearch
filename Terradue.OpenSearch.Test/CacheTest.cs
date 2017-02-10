using NUnit.Framework;
using System;
using log4net.Config;
using System.IO;
using Terradue.OpenSearch.Engine;
using Terradue.OpenSearch.Filters;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace Terradue.OpenSearch.Test {
    [TestFixture()]
    public class CacheTest {

        OpenSearchEngine ose;

        [SetUp]
        public void RunBeforeTests()
        {
            XmlConfigurator.Configure(new FileInfo("../Log4Net.config"));

            ose = new OpenSearchEngine();
            ose.LoadPlugins();

            OpenSearchMemoryCache cache = new OpenSearchMemoryCache();

            ose.RegisterPreSearchFilter(cache.TryReplaceWithCacheRequest);
            ose.RegisterPostSearchFilter(cache.CacheResponse);
        }

        [Test()]
        public void CacheTest1() {
            

            TestOpenSearchable entity1 = TestOpenSearchable.GenerateNumberedItomFeed("A", 100, new TimeSpan(0));

            ose.Query(entity1, new NameValueCollection());

            List<IOpenSearchable> entities = new List<IOpenSearchable>();
            entities.Add(entity1);

            entity1.Items.First().Identifier = "AA1";
            entity1.OnOpenSearchableChange(this, new OnOpenSearchableChangeEventArgs(entity1));

            ose.Query(entity1, new NameValueCollection());

            Thread.Sleep(1000);

            ose.Query(entity1, new NameValueCollection());




            IOpenSearchable multiEntity = new MultiGenericOpenSearchable(entities, ose, true);

        }
    }
}

