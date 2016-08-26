using System;
using NUnit.Framework;
using System.Linq;
using System.IO;
using System.Xml.Serialization;
using Terradue.OpenSearch.Schema;
using System.Xml;
using FluentAssertions;
using Terradue.OpenSearch.Engine;
using Mono.Addins;

namespace Terradue.OpenSearch.Test {

    [TestFixture]
    public class OpenSearchDescriptionTests {

        [Test]
        public void TestOpenSearchDescriptionParameters(){


            IOpenSearchable entity1 = TestOpenSearchable.GenerateNumberedItomFeed("A", 100, new TimeSpan(0));

            var osd = entity1.GetOpenSearchDescription();

            Assert.That(osd.Url[0].Parameters.First(p => p.Name == "count").MaxInclusive == "100");


            var stream = new MemoryStream();
            var serializer = new XmlSerializer(typeof(OpenSearchDescription));

            using (var xw = XmlWriter.Create(stream)) {

                serializer.Serialize(xw, osd);
                xw.Flush();
            }

            stream.Seek(0, SeekOrigin.Begin);

            OpenSearchDescription osd2;

            using (var xr = XmlReader.Create(stream)) {

                osd2 = (OpenSearchDescription)serializer.Deserialize(xr);

            }

            osd2.ExtraNamespace = osd.ExtraNamespace;

            osd2.ShouldBeEquivalentTo(osd);

        }

    }
}

