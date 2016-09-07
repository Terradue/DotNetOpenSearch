using System;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using NUnit.Framework;
using Terradue.OpenSearch;
using Terradue.OpenSearch.Schema;

namespace Terradue.OpenSearch.Test {

    [TestFixture]
    public class ParametersTest {

        public ParametersTest() {
        }

        [Test]
        public void TestTemplate() {
            OpenSearchParameterValueSet vs1 = OpenSearchParameterValueSet.FromOpenSearchDescription("http://mytest.com?bbox={geo:box?}&start={time:start}&end={time:stop}&test={test?}&format=atom&other=value");
            OpenSearchParameterValueSet vs2 = OpenSearchParameterValueSet.FromOpenSearchDescription("http://mytest.com?zone={geo:box?}&anfang={time:start}&ende={time:stop}&format=json");

            vs1.SetValuesByName("bbox", new string[] {"10,10,20,20", "30,30,40,40"});
            vs1.SetValueByName("start", "2010-01-01");
            vs1.SetValueByName("end", "2011-01-01");

            Assert.Throws<Exception>(delegate { vs1.SetValueByName("notfound", "bla"); }, "blabla");
            Assert.Throws<Exception>(delegate { vs1.SetValueByName("format", "json"); }, "blabla");

            Assert.AreEqual("bbox=10,10,20,20&bbox=30,30,40,40&start=2010-01-01&end=2011-01-01&test=&format=atom&other=value", vs1.GetQueryString(true));
            Assert.AreEqual("bbox=10,10,20,20&bbox=30,30,40,40&start=2010-01-01&end=2011-01-01&format=atom&other=value", vs1.GetQueryString(false));

            vs2.TranslateFrom(vs1);

            Assert.AreEqual("zone=10,10,20,20&zone=30,30,40,40&anfang=2010-01-01&ende=2011-01-01&format=json", vs2.GetQueryString(true));
            Assert.AreEqual("zone=10,10,20,20&zone=30,30,40,40&anfang=2010-01-01&ende=2011-01-01&format=json", vs2.GetQueryString(false));

            OpenSearchParameterValueSet vs3 = OpenSearchParameterValueSet.FromOpenSearchDescription("http://mytest.com?bbox={geo:box?}&start={time:start}&end={time:stop}&test={test?}&format=atom&other=value");
            System.Collections.Specialized.NameValueCollection nvc = new System.Collections.Specialized.NameValueCollection();
            nvc.Add("bbox", "1,1,2,2");
            nvc.Add("bbox", "3,3,4,4");
            nvc.Add("start", "2012-01-01");
            nvc.Add("end", "2013-01-01");
            nvc.Add("format", "error");

            vs3.SetValues(nvc);

            Assert.AreEqual("bbox=1,1,2,2&bbox=3,3,4,4&start=2012-01-01&end=2013-01-01&test=&format=atom&other=value", vs3.GetQueryString(true));
            Assert.AreEqual("bbox=1,1,2,2&bbox=3,3,4,4&start=2012-01-01&end=2013-01-01&format=atom&other=value", vs3.GetQueryString(false));
        }

        [Test]
        public void TestDeserializedObjects() {
            XmlSerializer ser = new XmlSerializer(typeof(OpenSearchDescription));
            OpenSearchDescription osd1 = (OpenSearchDescription)ser.Deserialize(XmlReader.Create("../Samples/ParametersTestOsd1.xml"));
            OpenSearchDescriptionUrl url1 = osd1.Url.SingleOrDefault(u => u.Type == "application/atom+xml");

            OpenSearchDescription osd2 = (OpenSearchDescription)ser.Deserialize(XmlReader.Create("../Samples/ParametersTestOsd2.xml"));
            OpenSearchDescriptionUrl url2 = osd1.Url.SingleOrDefault(u => u.Type == "application/atom+xml");

            //Fail
        }
    }
}

