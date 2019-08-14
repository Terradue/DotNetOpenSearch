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
using log4net;
using System.Reflection;
using Xunit;

namespace Terradue.OpenSearch.Test
{
    public class CacheTest : IClassFixture<TestFixture>
    {

        OpenSearchEngine ose;
        OpenSearchableFactorySettings settings;

        public CacheTest()
        {
            ose = new OpenSearchEngine();
            ose.LoadPlugins();

            settings = new OpenSearchableFactorySettings(ose);
            OpenSearchMemoryCache cache = new OpenSearchMemoryCache();

            ose.RegisterPreSearchFilter(cache.TryReplaceWithCacheRequest);
            ose.RegisterPostSearchFilter(cache.CacheResponse);
        }

        [Fact(DisplayName = "Cache Test #1")]
        [Trait("Category", "unit")]
        public void CacheTest1()
        {


            TestOpenSearchable entity1 = TestOpenSearchable.GenerateNumberedItomFeed("A", 100, new TimeSpan(0));

            ose.Query(entity1, new NameValueCollection());

            List<IOpenSearchable> entities = new List<IOpenSearchable>();
            entities.Add(entity1);

            entity1.Items.First().Identifier = "AA1";
            entity1.OnOpenSearchableChange(this, new OnOpenSearchableChangeEventArgs(entity1));

            ose.Query(entity1, new NameValueCollection());

            Thread.Sleep(1000);

            ose.Query(entity1, new NameValueCollection());

            IOpenSearchable multiEntity = new MultiGenericOpenSearchable(entities, settings, true);

        }
    }
}

