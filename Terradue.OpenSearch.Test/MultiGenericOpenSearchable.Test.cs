using System;
using NUnit.Framework;
using Mono.Addins;
using Terradue.OpenSearch.Engine;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace Terradue.OpenSearch.Test {

    [TestFixture]
    public class MultiGenericOpenSearchableTest {

        [Test()]
        public void GenericProxiedOpenSearchableTest() {

            AddinManager.Initialize();
            AddinManager.Registry.Update(null);

            OpenSearchEngine ose = new OpenSearchEngine();
            ose.LoadPlugins();

            IOpenSearchable entity1 = TestOpenSearchable.GenerateNumberedItomFeed("A");

            List<IOpenSearchable> entities = new List<IOpenSearchable>();
            entities.Add(entity1);

            IOpenSearchable multiEntity = new MultiGenericOpenSearchable(entities, ose, true);

            NameValueCollection nvc = new NameValueCollection();
            nvc.Set("count", "100");

            var osr = ose.Query(multiEntity, nvc, "atom");

            Assert.AreEqual(100, osr.Result.Count);
            Assert.AreEqual("A1", osr.Result.Items.First().Identifier);
            Assert.AreEqual("A100", osr.Result.Items.Last().Identifier);

            nvc.Set("startIndex", "16");

            osr = ose.Query(multiEntity, nvc, "atom");

            Assert.AreEqual(85, osr.Result.Count);
            Assert.AreEqual("A16", osr.Result.Items.First().Identifier);
            Assert.AreEqual("A100", osr.Result.Items.Last().Identifier);

            nvc.Set("count", "5");
            nvc.Set("startIndex", "5");
            nvc.Set("startPage", "4");

            osr = ose.Query(multiEntity, nvc, "atom");

            Assert.AreEqual(5, osr.Result.Count);
            Assert.AreEqual("A20", osr.Result.Items.First().Identifier);
            Assert.AreEqual("A24", osr.Result.Items.Last().Identifier);

            nvc.Set("count", "5");
            nvc.Set("startIndex", "5");
            nvc.Set("startPage", "5");

            osr = ose.Query(multiEntity, nvc, "atom");

            Assert.AreEqual(5, osr.Result.Count);
            Assert.AreEqual("A25", osr.Result.Items.First().Identifier);
            Assert.AreEqual("A29", osr.Result.Items.Last().Identifier);

            nvc.Set("count", "5");
            nvc.Set("startIndex", "1");
            nvc.Set("startPage", "1");

            osr = ose.Query(multiEntity, nvc, "atom");

            Assert.AreEqual(5, osr.Result.Count);
            Assert.AreEqual("A1", osr.Result.Items.First().Identifier);
            Assert.AreEqual("A5", osr.Result.Items.Last().Identifier);

            nvc.Set("count", "5");
            nvc.Set("startIndex", "1");
            nvc.Set("startPage", "5");

            osr = ose.Query(multiEntity, nvc, "atom");

            Assert.AreEqual(5, osr.Result.Count);
            Assert.AreEqual("A21", osr.Result.Items.First().Identifier);
            Assert.AreEqual("A25", osr.Result.Items.Last().Identifier);

            IOpenSearchable entity2 = TestOpenSearchable.GenerateNumberedItomFeed("B");

            entities.Add(entity2);
            multiEntity = new MultiGenericOpenSearchable(entities, ose, true);

            nvc = new NameValueCollection();
            nvc.Set("count", "10");

            osr = ose.Query(multiEntity, nvc, "atom");

            Assert.AreEqual(10, osr.Result.Count);
            Assert.AreEqual("A1", osr.Result.Items.First().Identifier);
            Assert.AreEqual("B5", osr.Result.Items.Last().Identifier);

            nvc.Set("count", "4");
            nvc.Set("startIndex", "2");
            nvc.Set("startPage", "1");

            osr = ose.Query(multiEntity, nvc, "atom");

            Assert.AreEqual(4, osr.Result.Count);
            Assert.AreEqual("B1", osr.Result.Items.First().Identifier);
            Assert.AreEqual("A3", osr.Result.Items.Last().Identifier);

            nvc.Set("count", "4");
            nvc.Set("startIndex", "4");
            nvc.Set("startPage", "1");

            osr = ose.Query(multiEntity, nvc, "atom");

            Assert.AreEqual(4, osr.Result.Count);
            Assert.AreEqual("B2", osr.Result.Items.First().Identifier);
            Assert.AreEqual("A4", osr.Result.Items.Last().Identifier);

            nvc.Set("count", "4");
            nvc.Set("startIndex", "4");
            nvc.Set("startPage", "2");

            osr = ose.Query(multiEntity, nvc, "atom");

            Assert.AreEqual(4, osr.Result.Count);
            Assert.AreEqual("B4", osr.Result.Items.First().Identifier);
            Assert.AreEqual("A6", osr.Result.Items.Last().Identifier);

        }
    }
}

