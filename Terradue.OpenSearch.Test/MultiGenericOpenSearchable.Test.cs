using System;
using Terradue.OpenSearch.Engine;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Xunit;
using Terradue.OpenSearch.Result;

namespace Terradue.OpenSearch.Test
{

    public class MultiGenericOpenSearchableTest {

        [Fact(DisplayName = "Generic Proxied OpenSearchable Test")]
        [Trait("Category", "unit")]
        public void GenericProxiedOpenSearchableTest() {

            OpenSearchEngine ose = new OpenSearchEngine();
            ose.LoadPlugins();

            var settings = new OpenSearchableFactorySettings(ose);

            IOpenSearchable entity1 = TestOpenSearchable.GenerateNumberedItomFeed("A", 100, new TimeSpan(0));

            List<IOpenSearchable> entities = new List<IOpenSearchable>();
            entities.Add(entity1);

            MultiGenericOpenSearchable multiEntity = new MultiGenericOpenSearchable(entities, settings, true);
            multiEntity.SortingComparer = new MethodSortingItemComparer();
            ((MethodSortingItemComparer)multiEntity.SortingComparer).SortMethods.Add((x,y) => x.PublishDate.CompareTo(y.PublishDate));

            NameValueCollection nvc = new NameValueCollection();

            var osr = ose.Query(multiEntity, nvc, "atom");

            Assert.Equal(100, osr.TotalResults);
            Assert.Equal(OpenSearchEngine.DEFAULT_COUNT, osr.Count);
            string totalResults = osr.ElementExtensions.ReadElementExtensions<string>("totalResults", "http://a9.com/-/spec/opensearch/1.1/")[0];
            Assert.Equal("100", totalResults);

            nvc.Set("count", "100");

            osr = ose.Query(multiEntity, nvc, "atom");

            Assert.Equal(100, osr.TotalResults);
            Assert.Equal(100, osr.Count);
            Assert.Equal("A1", osr.Items.First().Identifier);
            Assert.Equal("A100", osr.Items.Last().Identifier);

            nvc.Set("count", "6");
            nvc.Set("startPage", "1");

            osr = ose.Query(multiEntity, nvc, "atom");

            Assert.Equal(100, osr.TotalResults);
            Assert.Equal(6, osr.Count);
            Assert.Equal("A1", osr.Items.First().Identifier);
            Assert.Equal("A6", osr.Items.Last().Identifier);

            nvc.Set("count", "3");
            nvc.Set("startPage", "2");

            osr = ose.Query(multiEntity, nvc, "atom");

            Assert.Equal(100, osr.TotalResults);
            Assert.Equal(3, osr.Count);
            Assert.Equal("A4", osr.Items.First().Identifier);
            Assert.Equal("A6", osr.Items.Last().Identifier);

            nvc.Set("startIndex", "16");
            nvc.Set("startPage", "1");
            nvc.Set("count", "100");

            osr = ose.Query(multiEntity, nvc, "atom");

            Assert.Equal(85, osr.Count);
            Assert.Equal("A16", osr.Items.First().Identifier);
            Assert.Equal("A100", osr.Items.Last().Identifier);

            nvc.Set("count", "5");
            nvc.Set("startIndex", "5");
            nvc.Set("startPage", "1");

            osr = ose.Query(multiEntity, nvc, "atom");

            Assert.Equal(5, osr.Count);
            Assert.Equal("A5", osr.Items.First().Identifier);
            Assert.Equal("A9", osr.Items.Last().Identifier);

            nvc.Set("count", "5");
            nvc.Set("startIndex", "5");
            nvc.Set("startPage", "2");

            osr = ose.Query(multiEntity, nvc, "atom");

            Assert.Equal(5, osr.Count);
            Assert.Equal("A10", osr.Items.First().Identifier);
            Assert.Equal("A14", osr.Items.Last().Identifier);

            nvc.Set("count", "5");
            nvc.Set("startIndex", "5");
            nvc.Set("startPage", "4");

            osr = ose.Query(multiEntity, nvc, "atom");

            Assert.Equal(5, osr.Count);
            Assert.Equal("A20", osr.Items.First().Identifier);
            Assert.Equal("A24", osr.Items.Last().Identifier);

            nvc.Set("count", "5");
            nvc.Set("startIndex", "5");
            nvc.Set("startPage", "5");

            osr = ose.Query(multiEntity, nvc, "atom");

            Assert.Equal(5, osr.Count);
            Assert.Equal("A25", osr.Items.First().Identifier);
            Assert.Equal("A29", osr.Items.Last().Identifier);

            nvc.Set("count", "5");
            nvc.Set("startIndex", "1");
            nvc.Set("startPage", "1");

            osr = ose.Query(multiEntity, nvc, "atom");

            Assert.Equal(5, osr.Count);
            Assert.Equal("A1", osr.Items.First().Identifier);
            Assert.Equal("A5", osr.Items.Last().Identifier);

            nvc.Set("count", "5");
            nvc.Set("startIndex", "1");
            nvc.Set("startPage", "5");

            osr = ose.Query(multiEntity, nvc, "atom");

            Assert.Equal(5, osr.Count);
            Assert.Equal("A21", osr.Items.First().Identifier);
            Assert.Equal("A25", osr.Items.Last().Identifier);

            IOpenSearchable entity2 = TestOpenSearchable.GenerateNumberedItomFeed("B", 100, TimeSpan.FromMinutes(0.5));

            entities.Add(entity2);
            multiEntity = new MultiGenericOpenSearchable(entities, settings, true);
            multiEntity.SortingComparer = new MethodSortingItemComparer();
            ((MethodSortingItemComparer)multiEntity.SortingComparer).SortMethods.Add((x,y) => x.PublishDate.CompareTo(y.PublishDate));

            nvc = new NameValueCollection();
            nvc.Set("count", "10");

            osr = ose.Query(multiEntity, nvc, "atom");

            Assert.Equal(200, osr.TotalResults);
            Assert.Equal(10, osr.Count);
            Assert.Equal("A1", osr.Items.First().Identifier);
            Assert.Equal("B5", osr.Items.Last().Identifier);

            osr = ose.Query(multiEntity, nvc, "atom");

            Assert.Equal(200, osr.TotalResults);
            Assert.Equal(10, osr.Count);
            Assert.Equal("A1", osr.Items.First().Identifier);
            Assert.Equal("B5", osr.Items.Last().Identifier);

            nvc.Set("count", "4");
            nvc.Set("startIndex", "2");
            nvc.Set("startPage", "1");

            osr = ose.Query(multiEntity, nvc, "atom");

            Assert.Equal(4, osr.Count);
            Assert.Equal("B1", osr.Items.First().Identifier);
            Assert.Equal("A3", osr.Items.Last().Identifier);

            nvc.Set("count", "4");
            nvc.Set("startIndex", "4");
            nvc.Set("startPage", "1");

            osr = ose.Query(multiEntity, nvc, "atom");

            Assert.Equal(4, osr.Count);
            Assert.Equal("B2", osr.Items.First().Identifier);
            Assert.Equal("A4", osr.Items.Last().Identifier);

            nvc.Set("count", "4");
            nvc.Set("startIndex", "4");
            nvc.Set("startPage", "2");

            osr = ose.Query(multiEntity, nvc, "atom");

            Assert.Equal(4, osr.Count);
            Assert.Equal("B4", osr.Items.First().Identifier);
            Assert.Equal("A6", osr.Items.Last().Identifier);

        }

        [Fact(DisplayName = "Multi OpenSearchable Limit 2 Test")]
        [Trait("Category", "unit")]
        public void MultiLimitTwoTest() {

            OpenSearchEngine ose = new OpenSearchEngine();
            ose.LoadPlugins();

            var settings = new OpenSearchableFactorySettings(ose);

            IOpenSearchable entity1 = TestOpenSearchable.GenerateNumberedItomFeed("A", 1, new TimeSpan(0));
            IOpenSearchable entity2 = TestOpenSearchable.GenerateNumberedItomFeed("B", 1, TimeSpan.FromHours(-1));

            List<IOpenSearchable> entities = new List<IOpenSearchable>();
            entities.Add(entity1);
            entities.Add(entity2);

            MultiGenericOpenSearchable multiEntity = new MultiGenericOpenSearchable(entities, settings, true);
            multiEntity.SortingComparer = new MethodSortingItemComparer();
            ((MethodSortingItemComparer)multiEntity.SortingComparer).SortMethods.Add((x,y) => x.PublishDate.CompareTo(y.PublishDate));

            NameValueCollection nvc = new NameValueCollection();

            var osr = ose.Query(multiEntity, nvc, "atom");

            Assert.Equal(2, osr.Count);
        }

        [Fact(DisplayName = "Paginated Multi OpenSearchable Test")]
        [Trait("Category", "unit")]
        public void PaginationMultiOpenSearchableTest() {

            OpenSearchEngine ose = new OpenSearchEngine();
            ose.LoadPlugins();

            var settings = new OpenSearchableFactorySettings(ose);

            IOpenSearchable entity1 = TestOpenSearchable.GenerateNumberedItomFeed("A", 5, new TimeSpan(0));
            IOpenSearchable entity2 = TestOpenSearchable.GenerateNumberedItomFeed("B", 10, TimeSpan.FromDays(2000));

            List<IOpenSearchable> entities = new List<IOpenSearchable>();
            entities.Add(entity1);
            entities.Add(entity2);

            MultiGenericOpenSearchable multiEntity = new MultiGenericOpenSearchable(entities, settings, true);
            multiEntity.SortingComparer = new MethodSortingItemComparer();
            ((MethodSortingItemComparer)multiEntity.SortingComparer).SortMethods.Add((x,y) => x.PublishDate.CompareTo(y.PublishDate));
            

            NameValueCollection nvc = new NameValueCollection();

            var osr = ose.Query(multiEntity, nvc, "atom");

            nvc = new NameValueCollection();
            nvc.Set("count", "10");

            osr = ose.Query(multiEntity, nvc, "atom");

            Assert.Equal(15, osr.TotalResults);
            Assert.Equal(10, osr.Count);
            Assert.Equal("A1", osr.Items.First().Identifier);
            Assert.Equal("B5", osr.Items.Last().Identifier);

            nvc.Set("count", "10");
            nvc.Set("startPage", "1");

            osr = ose.Query(multiEntity, nvc, "atom");

            Assert.Equal(15, osr.TotalResults);
            Assert.Equal(10, osr.Count);
            Assert.Equal("A1", osr.Items.First().Identifier);
            Assert.Equal("B5", osr.Items.Last().Identifier);

            nvc.Set("count", "1");
            nvc.Set("startPage", "2");

            osr = ose.Query(multiEntity, nvc, "atom");

            Assert.Equal(15, osr.TotalResults);
            Assert.Equal(1, osr.Count);
            Assert.Equal("A2", osr.Items.First().Identifier);

            nvc.Set("count", "1");
            nvc.Set("startPage", "1");
            nvc.Set("startIndex", "20");

            osr = ose.Query(multiEntity, nvc, "atom");

            Assert.Equal(15, osr.TotalResults);
            Assert.Equal(0, osr.Count);

        }

        [Fact(DisplayName = "Same Multi OpenSearchable Test")]
        [Trait("Category", "unit")]
        public void SameMultiOpenSearchableTest() {

            OpenSearchEngine ose = new OpenSearchEngine();
            ose.LoadPlugins();

            var settings = new OpenSearchableFactorySettings(ose);

            IOpenSearchable entity1 = TestOpenSearchable.GenerateNumberedItomFeed("A", 5, new TimeSpan(0));
            IOpenSearchable entity2 = TestOpenSearchable.GenerateNumberedItomFeed("A", 5, new TimeSpan(0));

            List<IOpenSearchable> entities = new List<IOpenSearchable>();
            entities.Add(entity1);
            entities.Add(entity2);

            MultiGenericOpenSearchable multiEntity = new MultiGenericOpenSearchable(entities, settings, true);
            multiEntity.SortingComparer = new MethodSortingItemComparer();
            ((MethodSortingItemComparer)multiEntity.SortingComparer).SortMethods.Add((x,y) => x.PublishDate.CompareTo(y.PublishDate));

            NameValueCollection nvc = new NameValueCollection();

            var osr = ose.Query(multiEntity, nvc, "atom");

            nvc = new NameValueCollection();
            nvc.Set("count", "10");
            nvc.Set("q", "1");

            osr = ose.Query(multiEntity, nvc, "atom");

            Assert.Equal(1, osr.TotalResults);
            Assert.Equal(1, osr.Count);
            Assert.Equal("A1", osr.Items.First().Identifier);
            Assert.Equal("A1", osr.Items.Last().Identifier);

        }

		[Fact(DisplayName = "Start Index Multi OpenSearchable Test")]
        [Trait("Category", "unit")]
		public void StartIndexMultiOpenSearchableTest()
		{

			OpenSearchEngine ose = new OpenSearchEngine();
			ose.LoadPlugins();

            var settings = new OpenSearchableFactorySettings(ose);

			IOpenSearchable entity1 = TestOpenSearchable.GenerateNumberedItomFeed("A", 4, new TimeSpan(0));

			List<IOpenSearchable> entities = new List<IOpenSearchable>();
			entities.Add(entity1);

            MultiGenericOpenSearchable multiEntity = new MultiGenericOpenSearchable(entities, settings, true);
            multiEntity.SortingComparer = new MethodSortingItemComparer();
            ((MethodSortingItemComparer)multiEntity.SortingComparer).SortMethods.Add((x,y) => x.PublishDate.CompareTo(y.PublishDate));

			NameValueCollection nvc = new NameValueCollection();

			nvc = new NameValueCollection();
			nvc.Set("count", "2");
			nvc.Set("startIndex", "5");

			var osr = ose.Query(multiEntity, nvc, "atom");

			Assert.Equal(4, osr.TotalResults);
			Assert.Equal(0, osr.Count);


			nvc.Set("count", "2");
			nvc.Set("startIndex", "4");

			osr = ose.Query(multiEntity, nvc, "atom");

			Assert.Equal(4, osr.TotalResults);
			Assert.Equal(1, osr.Count);
			Assert.Equal("A4", osr.Items.First().Identifier);
			Assert.Equal("A4", osr.Items.Last().Identifier);

			nvc.Set("count", "2");
			nvc.Set("startIndex", "3");

			osr = ose.Query(multiEntity, nvc, "atom");

			Assert.Equal(4, osr.TotalResults);
			Assert.Equal(2, osr.Count);
			Assert.Equal("A3", osr.Items.First().Identifier);
			Assert.Equal("A4", osr.Items.Last().Identifier);

		}

        [Fact(DisplayName = "Catalogue Multi OpenSearchable Test #1")]
        [Trait("Category", "unit")]
        public void CatalogueMultiOpenSearchableTest()
        {

            OpenSearchEngine ose = new OpenSearchEngine();
            ose.LoadPlugins();

            var settings = new OpenSearchableFactorySettings(ose);

            UrlBasedOpenSearchableFactory factory = new UrlBasedOpenSearchableFactory(settings);

            IOpenSearchable entity1 = factory.Create(new OpenSearchUrl("https://catalog.terradue.com/hydro-co-floodmonitoring-apps/search?uid=floodmonitoring"));
            IOpenSearchable entity2 = factory.Create(new OpenSearchUrl("https://catalog.terradue.com/hydro-co-floodmonitoring-apps/search"));
            IOpenSearchable entity3 = factory.Create(new OpenSearchUrl("https://catalog.terradue.com/hydro-co-smallwaterbody-apps/search"));

            List<IOpenSearchable> entities = new List<IOpenSearchable>();
            entities.Add(entity1);
            entities.Add(entity2);
            entities.Add(entity3);

            MultiGenericOpenSearchable multiEntity = new MultiGenericOpenSearchable(entities, settings, true);
            multiEntity.SortingComparer = new MethodSortingItemComparer();
            ((MethodSortingItemComparer)multiEntity.SortingComparer).SortMethods.Add((x,y) => x.PublishDate.CompareTo(y.PublishDate));

            NameValueCollection nvc = new NameValueCollection();

            var osr = ose.Query(multiEntity, nvc, "atom");

            nvc = new NameValueCollection();
            //nvc.Set("count", "10");
            nvc.Set("q", "1");

            osr = ose.Query(multiEntity, nvc, "atom");

            Assert.Equal(2, osr.Count);
            Assert.Equal(2, osr.TotalResults);
        }

        [Fact(DisplayName = "Catalogue Multi OpenSearchable Test #2")]
        [Trait("Category", "unit")]
        public void CatalogueMultiOpenSearchableTest2()
        {

            OpenSearchEngine ose = new OpenSearchEngine();
            ose.LoadPlugins();

            var settings = new OpenSearchableFactorySettings(ose);
            settings.ParametersKeywordsTable.Add("uid", "{http://a9.com/-/opensearch/extensions/geo/1.0/}uid");
			UrlBasedOpenSearchableFactory factory = new UrlBasedOpenSearchableFactory(settings);

			IOpenSearchable entity1 = factory.Create(new OpenSearchUrl("https://catalog.terradue.com:443//sentinel1/series/GRD/search?format=atom&uid=S1A_IW_GRDH_1SDV_20160719T181151_20160719T181219_012221_012F93_3E0B"));
            IOpenSearchable entity2 = factory.Create(new OpenSearchUrl("https://catalog.terradue.com:443//sentinel1/series/GRD/search?format=atom&uid=S1A_IW_GRDH_1SDV_20160731T181152_20160731T181219_012396_013552_7CC6"));
            //IOpenSearchable entity3 = new GenericOpenSearchable(new OpenSearchUrl("https://catalog.terradue.com/hydro-co-smallwaterbody-apps/search"), ose);

            List<IOpenSearchable> entities = new List<IOpenSearchable>();
            entities.Add(entity1);
            entities.Add(entity2);
            //entities.Add(entity3);

            MultiGenericOpenSearchable multiEntity = new MultiGenericOpenSearchable(entities, settings, true);
            multiEntity.SortingComparer = new MethodSortingItemComparer();
            ((MethodSortingItemComparer)multiEntity.SortingComparer).SortMethods.Add((x,y) => x.PublishDate.CompareTo(y.PublishDate));

            NameValueCollection nvc = new NameValueCollection();

            var osr = ose.Query(multiEntity, nvc, "atom");

            nvc = new NameValueCollection();
            nvc.Set("uid", "");

            osr = ose.Query(multiEntity, nvc, "atom");

            Assert.Equal(2, osr.TotalResults);
            Assert.Equal(2, osr.Count);
        }
    }
}

