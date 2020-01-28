using System;
using System.Linq;
using System.IO;
using System.Xml.Serialization;
using Terradue.OpenSearch.Schema;
using System.Xml;
using Xunit;

namespace Terradue.OpenSearch.Test
{

    public class OpenSearchDescriptionTests
    {

        [Fact(DisplayName = "Test OpenSearch Description Parameters")]
        [Trait("Category", "unit")]
        public void TestOpenSearchDescriptionParameters()
        {
            IOpenSearchable entity1 = TestOpenSearchable.GenerateNumberedItomFeed("A", 100, new TimeSpan(0));

            var osd = entity1.GetOpenSearchDescription();

            Assert.True(osd.Url[0].Parameters.First(p => p.Name == "count").MaxInclusive == "100");

            var stream = new MemoryStream();
            var serializer = new XmlSerializer(typeof(OpenSearchDescription));

            using (var xw = XmlWriter.Create(stream))
            {
                serializer.Serialize(xw, osd);
                xw.Flush();
            }

            stream.Seek(0, SeekOrigin.Begin);

            OpenSearchDescription osd2;

            using (var xr = XmlReader.Create(stream))
            {
                osd2 = (OpenSearchDescription)serializer.Deserialize(xr);
            }

            osd2.ExtraNamespace = osd.ExtraNamespace;
            for (int i = 0; i < osd.Url.Count(); i++)
            {
                osd2.Url[i].ExtraNamespace = osd.Url[i].ExtraNamespace;
            }

        }

        [Fact(DisplayName = "Test OpenSearch Description Parameters Deserialization")]
        [Trait("Category", "unit")]
        public void TestOpenSearchDescriptionParametersDeser()
        {

            var serializer = new XmlSerializer(typeof(OpenSearchDescription));

            OpenSearchDescription osd;

            using (var xr = XmlReader.Create(new FileStream(TestFixture.TestBaseDir + "/Samples/AUX_Dynamic_Open.xml", FileMode.Open, FileAccess.Read)))
            {
                osd = (OpenSearchDescription)serializer.Deserialize(xr);
            }

            Assert.Equal("intersects", osd.Url.FirstOrDefault(u => u.Type == "application/atom+xml").Parameters.First(p => p.Name == "trel").Options.FirstOrDefault(o => o.Label == "intersects").Value);

            var stream = new MemoryStream();

            using (var xw = XmlWriter.Create(stream))
            {
                serializer.Serialize(xw, osd);
                xw.Flush();
            }

            stream.Seek(0, SeekOrigin.Begin);

            OpenSearchDescription osd2;

            using (var xr = XmlReader.Create(stream))
            {
                osd2 = (OpenSearchDescription)serializer.Deserialize(xr);
            }

            osd2.ExtraNamespace = osd.ExtraNamespace;
            for (int i = 0; i < osd.Url.Count(); i++)
            {
                osd2.Url[i].ExtraNamespace = osd.Url[i].ExtraNamespace;
            }

            using (var xw = XmlWriter.Create(new FileStream(TestFixture.TestBaseDir + "/out/TestOpenSearchDescriptionParametersDeser.xml", FileMode.Create, FileAccess.Write)))
            {

                serializer.Serialize(xw, osd);
                xw.Flush();
            }
        }
    }
}

