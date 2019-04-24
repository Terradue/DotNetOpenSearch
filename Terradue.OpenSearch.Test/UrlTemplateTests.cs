using NUnit.Framework;
using System;
using System.Collections.Specialized;
using System.Xml;
using Terradue.OpenSearch.Engine;
using Terradue.OpenSearch.Schema;

namespace Terradue.OpenSearch.Test
{
    [TestFixture()]
    public class UrlTemplateTests
    {
		[Test()]
        public void KeySearchParameters()
        {

            OpenSearchDescriptionUrl template = new OpenSearchDescriptionUrl("application/atom+xml",
             @"https://catalogue.nextgeoss.eu/opensearch/search.atom?collection_id=dataset&q={opensearch:searchTerms?}&rows={opensearch:count?}&page={opensearch:startPage?}&ext_bbox={geo:box?}&identifier={geo:uid?}&timerange_start={time:start?}&timerange_end={time:end?}&metadata_modified={eo:modificationDate?}&spatial_geom={geo:geometry?}&collection_id={custom:collection_id?}",
             "results",
             new System.Xml.Serialization.XmlSerializerNamespaces(new XmlQualifiedName[] {
              new XmlQualifiedName("om", "http://www.opengis.net/om/2.0"),
              new XmlQualifiedName("time", "http://a9.com/-/opensearch/extensions/time/1.0/"),
              new XmlQualifiedName("geo", "http://a9.com/-/opensearch/extensions/geo/1.0/"),
              new XmlQualifiedName("eo", "http://a9.com/-/opensearch/extensions/eo/1.0/"),
              new XmlQualifiedName("opensearch", "http://a9.com/-/spec/opensearch/1.1/"),
              new XmlQualifiedName("custom", "http://example.com/opensearchextensions/1.0/")
              }));

            NameValueCollection searchParameters = new NameValueCollection();

            searchParameters.Set("count", "20");
            searchParameters.Set("q", "test");
            searchParameters.Set("collection_id", "SENTINEL2_L1C");

            QuerySettings settings = new QuerySettings("application/atom+xml", null);
            settings.ParametersKeywordsTable.Add("uid", "{http://a9.com/-/opensearch/extensions/geo/1.0/}uid");
            settings.ParametersKeywordsTable.Add("collection_id", "{http://example.com/opensearchextensions/1.0/}collection_id");


            var result = OpenSearchFactory.BuildRequestUrlFromTemplate(template, searchParameters, settings);

            Assert.AreEqual(@"https://catalogue.nextgeoss.eu/opensearch/search.atom?collection_id=dataset&q=test&rows=20&page=&ext_bbox=&identifier=&timerange_start=&timerange_end=&metadata_modified=&spatial_geom=&collection_id=SENTINEL2_L1C",
                result.ToString()
            );

        }

        [Test()]
        public void FQDNSearchParameters()
        {

            OpenSearchDescriptionUrl template = new OpenSearchDescriptionUrl("application/atom+xml",
             @"https://catalogue.nextgeoss.eu/opensearch/search.atom?collection_id=dataset&q={opensearch:searchTerms?}&rows={opensearch:count?}&page={opensearch:startPage?}&ext_bbox={geo:box?}&identifier={geo:uid?}&timerange_start={time:start?}&timerange_end={time:end?}&metadata_modified={eo:modificationDate?}&spatial_geom={geo:geometry?}&collection_id={custom:collection_id?}",
             "results",
             new System.Xml.Serialization.XmlSerializerNamespaces(new XmlQualifiedName[] {
              new XmlQualifiedName("om", "http://www.opengis.net/om/2.0"),
              new XmlQualifiedName("time", "http://a9.com/-/opensearch/extensions/time/1.0/"),
              new XmlQualifiedName("geo", "http://a9.com/-/opensearch/extensions/geo/1.0/"),
              new XmlQualifiedName("eo", "http://a9.com/-/opensearch/extensions/eo/1.0/"),
              new XmlQualifiedName("opensearch", "http://a9.com/-/spec/opensearch/1.1/"),
              new XmlQualifiedName("custom", "http://example.com/opensearchextensions/1.0/")
              }));

            NameValueCollection searchParameters = new NameValueCollection();

            searchParameters.Set("{http://a9.com/-/opensearch/extensions/geo/1.0/}box", "-180,-90,180,90");
            searchParameters.Set("{http://a9.com/-/spec/opensearch/1.1/}searchTerms", "test");
            searchParameters.Set("{http://a9.com/-/spec/opensearch/1.1/}count", "20");
            searchParameters.Set("{http://example.com/opensearchextensions/1.0/}collection_id", "SENTINEL2_L1C");

            var result = OpenSearchFactory.BuildRequestUrlFromTemplate(template, searchParameters, new QuerySettings("application/atom+xml", null));

            Assert.AreEqual(@"https://catalogue.nextgeoss.eu/opensearch/search.atom?collection_id=dataset&q=test&rows=20&page=&ext_bbox=-180%2c-90%2c180%2c90&identifier=&timerange_start=&timerange_end=&metadata_modified=&spatial_geom=&collection_id=SENTINEL2_L1C",
                result.ToString()
            );

        }

        [Test()]
        public void MixSearchParameters()
        {

            OpenSearchDescriptionUrl template = new OpenSearchDescriptionUrl("application/atom+xml",
             @"https://catalogue.nextgeoss.eu/opensearch/search.atom?collection_id=dataset&q={opensearch:searchTerms?}&rows={opensearch:count?}&page={opensearch:startPage?}&ext_bbox={geo:box?}&identifier={geo:uid?}&timerange_start={time:start?}&timerange_end={time:end?}&metadata_modified={eo:modificationDate?}&spatial_geom={geo:geometry?}&collection_id={custom:collection_id?}",
             "results",
             new System.Xml.Serialization.XmlSerializerNamespaces(new XmlQualifiedName[] {
              new XmlQualifiedName("om", "http://www.opengis.net/om/2.0"),
              new XmlQualifiedName("time", "http://a9.com/-/opensearch/extensions/time/1.0/"),
              new XmlQualifiedName("geo", "http://a9.com/-/opensearch/extensions/geo/1.0/"),
              new XmlQualifiedName("eo", "http://a9.com/-/opensearch/extensions/eo/1.0/"),
              new XmlQualifiedName("opensearch", "http://a9.com/-/spec/opensearch/1.1/"),
              new XmlQualifiedName("custom", "http://example.com/opensearchextensions/1.0/")
              }));

            NameValueCollection searchParameters = new NameValueCollection();

            searchParameters.Set("{http://a9.com/-/opensearch/extensions/geo/1.0/}box", "-180,-90,180,90");
            searchParameters.Set("{http://a9.com/-/spec/opensearch/1.1/}searchTerms", "test");
            searchParameters.Set("{http://a9.com/-/spec/opensearch/1.1/}count", "20");
            searchParameters.Set("{http://example.com/opensearchextensions/1.0/}collection_id", "SENTINEL2_L1C");
            searchParameters.Set("count", "20");
            searchParameters.Set("spatial_geom", "POINT(0 0)");
            searchParameters.Set("time:start", "20190101");

            QuerySettings settings = new QuerySettings("application/atom+xml", null);
            settings.ParametersKeywordsTable.Add("uid", "{http://a9.com/-/opensearch/extensions/geo/1.0/}uid");
            settings.ParametersKeywordsTable.Add("collection_id", "{http://example.com/opensearchextensions/1.0/}collection_id");
            settings.ParametersKeywordsTable.Add("spatial_geom", "{http://a9.com/-/opensearch/extensions/geo/1.0/}geometry");

            var result = OpenSearchFactory.BuildRequestUrlFromTemplate(template, searchParameters, settings);

            Assert.AreEqual(@"https://catalogue.nextgeoss.eu/opensearch/search.atom?collection_id=dataset&q=test&rows=20&page=&ext_bbox=-180%2c-90%2c180%2c90&identifier=&timerange_start=20190101&timerange_end=&metadata_modified=&spatial_geom=POINT(0+0)&collection_id=SENTINEL2_L1C",
                result.ToString()
            );

        }

    }
}
