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

        //---------------------------------------------------------------------------------------------------------------------

        [Test]
        public void TestTemplate() {

            // Definitions of value sets by template URL; the second one is equivalent to the first but with parameter names in German
            OpenSearchParameterValueSet vs1 = OpenSearchParameterValueSet.FromOpenSearchDescription("http://mytest.com?bbox={geo:box?}&start={time:start}&end={time:stop}&test={test?}&format=atom&other=value");
            OpenSearchParameterValueSet vs2 = OpenSearchParameterValueSet.FromOpenSearchDescription("http://mytest.com?rechteck={geo:box?}&anfang={time:start}&ende={time:stop}&format=json");

            // Set parameters
            vs1.SetValuesByName("bbox", new string[] {"10,10,20,20", "30,30,40,40"});
            vs1.SetValueByName("start", "2010-01-01");
            vs1.SetValueByName("end", "2011-01-01");

            // Make sure that non-existing or fixed-value parameters cannot be set
            Assert.Throws<OpenSearchException>(delegate { vs1.SetValueByName("notfound", "bla"); });
            Assert.Throws<OpenSearchException>(delegate { vs1.SetValueByName("format", "json"); });

            // Add non-existing parameter and set it
            vs1.AddExtraParameter("notfound");
            vs1.SetValueByName("notfound", "bla");

            // Verify search query string correctness
            Assert.AreEqual("bbox=10,10,20,20&bbox=30,30,40,40&start=2010-01-01&end=2011-01-01&test=&format=atom&other=value&notfound=bla", vs1.GetQueryString(true));
            Assert.AreEqual("bbox=10,10,20,20&bbox=30,30,40,40&start=2010-01-01&end=2011-01-01&format=atom&other=value&notfound=bla", vs1.GetQueryString(false));

            // Translate value set by identifier matching
            vs2.TranslateFrom(vs1);
            Assert.AreEqual("rechteck=10,10,20,20&rechteck=30,30,40,40&anfang=2010-01-01&ende=2011-01-01&format=json", vs2.GetQueryString(true));
            Assert.AreEqual("rechteck=10,10,20,20&rechteck=30,30,40,40&anfang=2010-01-01&ende=2011-01-01&format=json", vs2.GetQueryString(false));

            // Create new value set and fill it with values from NameValueCollection
            OpenSearchParameterValueSet vs3 = OpenSearchParameterValueSet.FromOpenSearchDescription("http://mytest.com?bbox={geo:box?}&start={time:start}&end={time:stop}&test={test?}&format=atom&other=value");
            System.Collections.Specialized.NameValueCollection nvc = new System.Collections.Specialized.NameValueCollection();
            nvc.Add("bbox", "1,1,2,2");
            nvc.Add("bbox", "3,3,4,4");
            nvc.Add("start", "2012-01-01");
            nvc.Add("end", "2013-01-01");
            nvc.Add("format", "error");
            vs3.SetValues(nvc);

            // Verify search query string correctness
            Assert.AreEqual("bbox=1,1,2,2&bbox=3,3,4,4&start=2012-01-01&end=2013-01-01&test=&format=atom&other=value", vs3.GetQueryString(true));
            Assert.AreEqual("bbox=1,1,2,2&bbox=3,3,4,4&start=2012-01-01&end=2013-01-01&format=atom&other=value", vs3.GetQueryString(false));
        }

        //---------------------------------------------------------------------------------------------------------------------

        [Test]
        public void TestDeserializedObjects() {
            XmlSerializer ser = new XmlSerializer(typeof(OpenSearchDescription));

            // Base OpenSearch description document
            OpenSearchDescription osd1 = (OpenSearchDescription)ser.Deserialize(XmlReader.Create(Util.TestBaseDir + "/Samples/ParametersTest1.osd.xml"));
            OpenSearchParameterValueSet vs1 = OpenSearchParameterValueSet.FromOpenSearchDescription(osd1, "application/atom+xml");

            // Equivalent OpenSearch description document where parameter names are changed
            OpenSearchDescription osd2 = (OpenSearchDescription)ser.Deserialize(XmlReader.Create(Util.TestBaseDir + "/Samples/ParametersTest2.osd.xml"));
            OpenSearchParameterValueSet vs2 = OpenSearchParameterValueSet.FromOpenSearchDescription(osd2, "application/atom+xml");

            // Equivalent OpenSearch description document where parameter names and namespace prefixes are changed
            OpenSearchDescription osd3 = (OpenSearchDescription)ser.Deserialize(XmlReader.Create(Util.TestBaseDir + "/Samples/ParametersTest3.osd.xml"));
            OpenSearchParameterValueSet vs3 = OpenSearchParameterValueSet.FromOpenSearchDescription(osd3, "application/atom+xml");

            // Set values
            vs1.SetValueByIdentifier("http://a9.com/-/opensearch/extensions/geo/1.0/", "box", "5,5,6,6");
            vs1.SetValueByIdentifier("searchTerms", "test search");

            // Verify search query string correctness
            Assert.AreEqual("format=atom&count=&startPage=&startIndex=&q=test search&start=&stop=&trel=&bbox=5,5,6,6&uid=", vs1.GetQueryString(true));
            Assert.AreEqual("format=atom&q=test search&bbox=5,5,6,6", vs1.GetQueryString(false));

            // Translate value set by basic identifier matching
            vs2.TranslateFrom(vs1);
            Assert.AreEqual("format=atom&count2=&startPage2=&startIndex2=&q2=test search&start2=&stop2=&trel2=&bbox2=5,5,6,6&uid2=", vs2.GetQueryString(true));
            Assert.AreEqual("format=atom&q2=test search&bbox2=5,5,6,6", vs2.GetQueryString(false));

            // Translate value set by advanced identifier matching (verify namespace URIs)
            vs3.TranslateFrom(vs1, true);
            Assert.AreEqual("format=atom&count3=&startPage3=&startIndex3=&q3=test search&start3=&stop3=&trel3=&bbox3=5,5,6,6&uid3=", vs3.GetQueryString(true));
            Assert.AreEqual("format=atom&q3=test search&bbox3=5,5,6,6", vs3.GetQueryString(false));

        }
    }
}

