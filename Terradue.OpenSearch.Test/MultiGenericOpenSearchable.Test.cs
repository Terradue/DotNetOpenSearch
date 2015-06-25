using System;
using NUnit.Framework;
using Mono.Addins;
using Terradue.OpenSearch.Engine;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using log4net.Config;
using System.IO;

namespace Terradue.OpenSearch.Test {

    [TestFixture]
    public class MultiGenericOpenSearchableTest {

        [SetUp]
        public void RunBeforeTests()
        {
            XmlConfigurator.Configure(new FileInfo("../Log4Net.config"));
        }

        [Test()]
        public void GenericProxiedOpenSearchableTest() {

            AddinManager.Initialize();
            AddinManager.Registry.Update(null);

            OpenSearchEngine ose = new OpenSearchEngine();
            ose.LoadPlugins();

            IOpenSearchable entity1 = TestOpenSearchable.GenerateNumberedItomFeed("A", 100, new TimeSpan(0));

            List<IOpenSearchable> entities = new List<IOpenSearchable>();
            entities.Add(entity1);

            IOpenSearchable multiEntity = new MultiGenericOpenSearchable(entities, ose, true);

            NameValueCollection nvc = new NameValueCollection();

            var osr = ose.Query(multiEntity, nvc, "atom");

            Assert.AreEqual(100, osr.TotalResults);
            Assert.AreEqual(OpenSearchEngine.DEFAULT_COUNT, osr.Count);
            string totalResults = osr.ElementExtensions.ReadElementExtensions<string>("totalResults", "http://a9.com/-/spec/opensearch/1.1/")[0];
            Assert.AreEqual("100", totalResults);

            nvc.Set("count", "100");

            osr = ose.Query(multiEntity, nvc, "atom");

            Assert.AreEqual(100, osr.TotalResults);
            Assert.AreEqual(100, osr.Count);
            Assert.AreEqual("A1", osr.Items.First().Identifier);
            Assert.AreEqual("A100", osr.Items.Last().Identifier);

            nvc.Set("count", "6");
            nvc.Set("startPage", "1");

            osr = ose.Query(multiEntity, nvc, "atom");

            Assert.AreEqual(100, osr.TotalResults);
            Assert.AreEqual(6, osr.Count);
            Assert.AreEqual("A1", osr.Items.First().Identifier);
            Assert.AreEqual("A6", osr.Items.Last().Identifier);

            nvc.Set("count", "3");
            nvc.Set("startPage", "2");

            osr = ose.Query(multiEntity, nvc, "atom");

            Assert.AreEqual(100, osr.TotalResults);
            Assert.AreEqual(3, osr.Count);
            Assert.AreEqual("A4", osr.Items.First().Identifier);
            Assert.AreEqual("A6", osr.Items.Last().Identifier);

            nvc.Set("startIndex", "16");
            nvc.Set("startPage", "1");
            nvc.Set("count", "100");

            osr = ose.Query(multiEntity, nvc, "atom");

            Assert.AreEqual(85, osr.Count);
            Assert.AreEqual("A16", osr.Items.First().Identifier);
            Assert.AreEqual("A100", osr.Items.Last().Identifier);

            nvc.Set("count", "5");
            nvc.Set("startIndex", "5");
            nvc.Set("startPage", "1");

            osr = ose.Query(multiEntity, nvc, "atom");

            Assert.AreEqual(5, osr.Count);
            Assert.AreEqual("A5", osr.Items.First().Identifier);
            Assert.AreEqual("A9", osr.Items.Last().Identifier);

            nvc.Set("count", "5");
            nvc.Set("startIndex", "5");
            nvc.Set("startPage", "2");

            osr = ose.Query(multiEntity, nvc, "atom");

            Assert.AreEqual(5, osr.Count);
            Assert.AreEqual("A10", osr.Items.First().Identifier);
            Assert.AreEqual("A14", osr.Items.Last().Identifier);

            nvc.Set("count", "5");
            nvc.Set("startIndex", "5");
            nvc.Set("startPage", "4");

            osr = ose.Query(multiEntity, nvc, "atom");

            Assert.AreEqual(5, osr.Count);
            Assert.AreEqual("A20", osr.Items.First().Identifier);
            Assert.AreEqual("A24", osr.Items.Last().Identifier);

            nvc.Set("count", "5");
            nvc.Set("startIndex", "5");
            nvc.Set("startPage", "5");

            osr = ose.Query(multiEntity, nvc, "atom");

            Assert.AreEqual(5, osr.Count);
            Assert.AreEqual("A25", osr.Items.First().Identifier);
            Assert.AreEqual("A29", osr.Items.Last().Identifier);

            nvc.Set("count", "5");
            nvc.Set("startIndex", "1");
            nvc.Set("startPage", "1");

            osr = ose.Query(multiEntity, nvc, "atom");

            Assert.AreEqual(5, osr.Count);
            Assert.AreEqual("A1", osr.Items.First().Identifier);
            Assert.AreEqual("A5", osr.Items.Last().Identifier);

            nvc.Set("count", "5");
            nvc.Set("startIndex", "1");
            nvc.Set("startPage", "5");

            osr = ose.Query(multiEntity, nvc, "atom");

            Assert.AreEqual(5, osr.Count);
            Assert.AreEqual("A21", osr.Items.First().Identifier);
            Assert.AreEqual("A25", osr.Items.Last().Identifier);

            IOpenSearchable entity2 = TestOpenSearchable.GenerateNumberedItomFeed("B", 100, TimeSpan.FromHours(-1));

            entities.Add(entity2);
            multiEntity = new MultiGenericOpenSearchable(entities, ose, true);

            nvc = new NameValueCollection();
            nvc.Set("count", "10");

            osr = ose.Query(multiEntity, nvc, "atom");

            Assert.AreEqual(200, osr.TotalResults);
            Assert.AreEqual(10, osr.Count);
            Assert.AreEqual("A1", osr.Items.First().Identifier);
            Assert.AreEqual("B5", osr.Items.Last().Identifier);

            osr = ose.Query(multiEntity, nvc, "atom");

            Assert.AreEqual(200, osr.TotalResults);
            Assert.AreEqual(10, osr.Count);
            Assert.AreEqual("A1", osr.Items.First().Identifier);
            Assert.AreEqual("B5", osr.Items.Last().Identifier);

            nvc.Set("count", "4");
            nvc.Set("startIndex", "2");
            nvc.Set("startPage", "1");

            osr = ose.Query(multiEntity, nvc, "atom");

            Assert.AreEqual(4, osr.Count);
            Assert.AreEqual("B1", osr.Items.First().Identifier);
            Assert.AreEqual("A3", osr.Items.Last().Identifier);

            nvc.Set("count", "4");
            nvc.Set("startIndex", "4");
            nvc.Set("startPage", "1");

            osr = ose.Query(multiEntity, nvc, "atom");

            Assert.AreEqual(4, osr.Count);
            Assert.AreEqual("B2", osr.Items.First().Identifier);
            Assert.AreEqual("A4", osr.Items.Last().Identifier);

            nvc.Set("count", "4");
            nvc.Set("startIndex", "4");
            nvc.Set("startPage", "2");

            osr = ose.Query(multiEntity, nvc, "atom");

            Assert.AreEqual(4, osr.Count);
            Assert.AreEqual("B4", osr.Items.First().Identifier);
            Assert.AreEqual("A6", osr.Items.Last().Identifier);

        }

        [Test()]
        public void MultiLimitTwoTest() {

            AddinManager.Initialize();
            AddinManager.Registry.Update(null);

            OpenSearchEngine ose = new OpenSearchEngine();
            ose.LoadPlugins();

            IOpenSearchable entity1 = TestOpenSearchable.GenerateNumberedItomFeed("A", 1, new TimeSpan(0));
            IOpenSearchable entity2 = TestOpenSearchable.GenerateNumberedItomFeed("B", 1, TimeSpan.FromHours(-1));

            List<IOpenSearchable> entities = new List<IOpenSearchable>();
            entities.Add(entity1);
            entities.Add(entity2);

            IOpenSearchable multiEntity = new MultiGenericOpenSearchable(entities, ose, true);

            NameValueCollection nvc = new NameValueCollection();

            var osr = ose.Query(multiEntity, nvc, "atom");

            Assert.AreEqual(2, osr.Count);
        }

        [Test()]
        public void PaginationMultiOpenSearchableTest() {

            AddinManager.Initialize();
            AddinManager.Registry.Update(null);

            OpenSearchEngine ose = new OpenSearchEngine();
            ose.LoadPlugins();

            IOpenSearchable entity1 = TestOpenSearchable.GenerateNumberedItomFeed("A", 5, new TimeSpan(0));
            IOpenSearchable entity2 = TestOpenSearchable.GenerateNumberedItomFeed("B", 10, TimeSpan.FromDays(-2000));

            List<IOpenSearchable> entities = new List<IOpenSearchable>();
            entities.Add(entity1);
            entities.Add(entity2);

            IOpenSearchable multiEntity = new MultiGenericOpenSearchable(entities, ose, true);

            NameValueCollection nvc = new NameValueCollection();

            var osr = ose.Query(multiEntity, nvc, "atom");

            nvc = new NameValueCollection();
            nvc.Set("count", "10");

            osr = ose.Query(multiEntity, nvc, "atom");

            Assert.AreEqual(15, osr.TotalResults);
            Assert.AreEqual(10, osr.Count);
            Assert.AreEqual("A1", osr.Items.First().Identifier);
            Assert.AreEqual("B5", osr.Items.Last().Identifier);

            nvc.Set("count", "10");
            nvc.Set("startPage", "1");

            osr = ose.Query(multiEntity, nvc, "atom");

            Assert.AreEqual(15, osr.TotalResults);
            Assert.AreEqual(10, osr.Count);
            Assert.AreEqual("A1", osr.Items.First().Identifier);
            Assert.AreEqual("B5", osr.Items.Last().Identifier);

            nvc.Set("count", "1");
            nvc.Set("startPage", "2");

            osr = ose.Query(multiEntity, nvc, "atom");

            Assert.AreEqual(15, osr.TotalResults);
            Assert.AreEqual(1, osr.Count);
            Assert.AreEqual("A2", osr.Items.First().Identifier);

        }

    }
}

