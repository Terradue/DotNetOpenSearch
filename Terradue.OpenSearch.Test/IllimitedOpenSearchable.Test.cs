﻿using System;
using Terradue.OpenSearch.Engine;
using System.Collections.Specialized;
using System.Linq;
using Xunit;

namespace Terradue.OpenSearch.Test {

    public class IllimitedOpenSearchableTest {

        [Fact(DisplayName = "Illimited OpenSearch Test #1")]
        [Trait("Category", "unit")]
        public void IllimitedOpenSearchableTest1() {

            OpenSearchEngine ose = new OpenSearchEngine();
            ose.LoadPlugins();

            var settings = new OpenSearchableFactorySettings(ose);

            TestUnlimitedOpenSearchable entity = TestUnlimitedOpenSearchable.GenerateNumberedItomFeed("A", 100, new TimeSpan(0));
            IOpenSearchable entity1 = new IllimitedOpenSearchable(entity, settings);

            NameValueCollection nvc = new NameValueCollection();

            var osr = ose.Query(entity1, nvc, "atom");

            Assert.Equal(21, osr.TotalResults);
            Assert.Equal(OpenSearchEngine.DEFAULT_COUNT, osr.Count);
            string totalResults = osr.ElementExtensions.ReadElementExtensions<string>("totalResults", "http://a9.com/-/spec/opensearch/1.1/")[0];
            Assert.Equal("21", totalResults);

            nvc.Set("count", "10");
            nvc.Set("q", "11");

            osr = ose.Query(entity1, nvc, "atom");

            Assert.Equal(1, osr.TotalResults);
            Assert.Equal(1, osr.Count);
            Assert.Equal("A11", osr.Items.First().Identifier);
            Assert.Equal("A11", osr.Items.Last().Identifier);

            nvc.Set("count", "10");
            nvc.Set("q", "10");

            osr = ose.Query(entity1, nvc, "atom");

            Assert.Equal(2, osr.TotalResults);
            Assert.Equal(2, osr.Count);
            Assert.Equal("A10", osr.Items.First().Identifier);
            Assert.Equal("A100", osr.Items.Last().Identifier);

            nvc.Set("count", "3");
            nvc.Set("q", "3");
            nvc.Set("startPage", "2");

            osr = ose.Query(entity1, nvc, "atom");

            Assert.Equal(7, osr.TotalResults);
            Assert.Equal(3, osr.Count);
            Assert.Equal("A30", osr.Items.First().Identifier);
            Assert.Equal("A32", osr.Items.Last().Identifier);

            nvc.Set("startIndex", "16");
            nvc.Set("startPage", "1");
            nvc.Set("count", "3");

            osr = ose.Query(entity1, nvc, "atom");

            Assert.Equal(3, osr.Count);
            Assert.Equal("A63", osr.Items.First().Identifier);
            Assert.Equal("A83", osr.Items.Last().Identifier);

            nvc.Set("count", "5");
            nvc.Set("startIndex", "101");
            nvc.Set("startPage", "1");

            osr = ose.Query(entity1, nvc, "atom");

            Assert.Equal(0, osr.Count);

            /*nvc.Set("count", "5");
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
            Assert.AreEqual("A6", osr.Items.Last().Identifier);*/

        }


    }
}

