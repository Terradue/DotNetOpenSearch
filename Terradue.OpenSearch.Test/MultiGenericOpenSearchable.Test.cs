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
    public class MultiGenericOpenSearchableTest {

        [SetUp]
        public void RunBeforeTests()
        {
            XmlConfigurator.Configure(new FileInfo("../Log4Net.config"));
        }

        [Test()]
        public void GenericProxiedOpenSearchableTest() {

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

            nvc.Set("count", "1");
            nvc.Set("startPage", "1");
            nvc.Set("startIndex", "20");

            osr = ose.Query(multiEntity, nvc, "atom");

            Assert.AreEqual(15, osr.TotalResults);
            Assert.AreEqual(0, osr.Count);

        }

        [Test()]
        public void SameMultiOpenSearchableTest() {

            OpenSearchEngine ose = new OpenSearchEngine();
            ose.LoadPlugins();

            IOpenSearchable entity1 = TestOpenSearchable.GenerateNumberedItomFeed("A", 5, new TimeSpan(0));
            IOpenSearchable entity2 = TestOpenSearchable.GenerateNumberedItomFeed("A", 5, new TimeSpan(0));

            List<IOpenSearchable> entities = new List<IOpenSearchable>();
            entities.Add(entity1);
            entities.Add(entity2);

            IOpenSearchable multiEntity = new MultiGenericOpenSearchable(entities, ose, true);

            NameValueCollection nvc = new NameValueCollection();

            var osr = ose.Query(multiEntity, nvc, "atom");

            nvc = new NameValueCollection();
            nvc.Set("count", "10");
            nvc.Set("q", "1");

            osr = ose.Query(multiEntity, nvc, "atom");

            Assert.AreEqual(1, osr.TotalResults);
            Assert.AreEqual(1, osr.Count);
            Assert.AreEqual("A1", osr.Items.First().Identifier);
            Assert.AreEqual("A1", osr.Items.Last().Identifier);



        }

		[Test()]
		public void StartIndexMultiOpenSearchableTest()
		{

			OpenSearchEngine ose = new OpenSearchEngine();
			ose.LoadPlugins();

			IOpenSearchable entity1 = TestOpenSearchable.GenerateNumberedItomFeed("A", 4, new TimeSpan(0));

			List<IOpenSearchable> entities = new List<IOpenSearchable>();
			entities.Add(entity1);

			IOpenSearchable multiEntity = new MultiGenericOpenSearchable(entities, ose, true);

			NameValueCollection nvc = new NameValueCollection();

			nvc = new NameValueCollection();
			nvc.Set("count", "2");
			nvc.Set("startIndex", "5");

			var osr = ose.Query(multiEntity, nvc, "atom");

			Assert.AreEqual(4, osr.TotalResults);
			Assert.AreEqual(0, osr.Count);


			nvc.Set("count", "2");
			nvc.Set("startIndex", "4");

			osr = ose.Query(multiEntity, nvc, "atom");

			Assert.AreEqual(4, osr.TotalResults);
			Assert.AreEqual(1, osr.Count);
			Assert.AreEqual("A4", osr.Items.First().Identifier);
			Assert.AreEqual("A4", osr.Items.Last().Identifier);

			nvc.Set("count", "2");
			nvc.Set("startIndex", "3");

			osr = ose.Query(multiEntity, nvc, "atom");

			Assert.AreEqual(4, osr.TotalResults);
			Assert.AreEqual(2, osr.Count);
			Assert.AreEqual("A3", osr.Items.First().Identifier);
			Assert.AreEqual("A4", osr.Items.Last().Identifier);

		}

        [Test()]
        public void CatalogueMultiOpenSearchableTest()
        {

            OpenSearchEngine ose = new OpenSearchEngine();
            ose.LoadPlugins();

            IOpenSearchable entity1 = ose.Create(new OpenSearchUrl("https://catalog.terradue.com/hydro-co-floodmonitoring-apps/search?uid=floodmonitoring"));
            IOpenSearchable entity2 = ose.Create(new OpenSearchUrl("https://catalog.terradue.com/hydro-co-floodmonitoring-apps/search"));
            IOpenSearchable entity3 = ose.Create(new OpenSearchUrl("https://catalog.terradue.com/hydro-co-smallwaterbody-apps/search"));

            List<IOpenSearchable> entities = new List<IOpenSearchable>();
            entities.Add(entity1);
            entities.Add(entity2);
            entities.Add(entity3);

            IOpenSearchable multiEntity = new MultiGenericOpenSearchable(entities, ose, true);

            NameValueCollection nvc = new NameValueCollection();

            var osr = ose.Query(multiEntity, nvc, "atom");

            nvc = new NameValueCollection();
            //nvc.Set("count", "10");
            nvc.Set("q", "1");

            osr = ose.Query(multiEntity, nvc, "atom");


            Assert.AreEqual(2, osr.Count);
            Assert.AreEqual(2, osr.TotalResults);



        }

        [Test()]
        public void CatalogueMultiOpenSearchableTest2()
        {

            OpenSearchEngine ose = new OpenSearchEngine();
            ose.LoadPlugins();

            IOpenSearchable entity1 = ose.Create(new OpenSearchUrl("https://catalog.terradue.com:443//sentinel1/series/GRD/search?format=atom&uid=S1A_IW_GRDH_1SDV_20160719T181151_20160719T181219_012221_012F93_3E0B"));
            IOpenSearchable entity2 = ose.Create(new OpenSearchUrl("https://catalog.terradue.com:443//sentinel1/series/GRD/search?format=atom&uid=S1A_IW_GRDH_1SDV_20160731T181152_20160731T181219_012396_013552_7CC6"));
            //IOpenSearchable entity3 = new GenericOpenSearchable(new OpenSearchUrl("https://catalog.terradue.com/hydro-co-smallwaterbody-apps/search"), ose);

            List<IOpenSearchable> entities = new List<IOpenSearchable>();
            entities.Add(entity1);
            entities.Add(entity2);
            //entities.Add(entity3);

            IOpenSearchable multiEntity = new MultiGenericOpenSearchable(entities, ose, true);

            NameValueCollection nvc = new NameValueCollection();

            var osr = ose.Query(multiEntity, nvc, "atom");

            nvc = new NameValueCollection();
            //nvc.Set("count", "10");
            //nvc.Set("q", "1");
            nvc.Set("uid", "");

            osr = ose.Query(multiEntity, nvc, "atom");

            Assert.AreEqual(2, osr.TotalResults);
            Assert.AreEqual(2, osr.Count);




        }


    }
}

