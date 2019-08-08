using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Terradue.OpenSearch.Benchmarking;
using Terradue.OpenSearch.Engine;

namespace Terradue.OpenSearch.Test
{
    [TestFixture()]
    public class QueryTests
    {
		[Test()]
        public void CountTest()
        {

            OpenSearchEngine ose = new OpenSearchEngine();
            ose.LoadPlugins();

            var settings = new OpenSearchableFactorySettings(ose);
            UrlBasedOpenSearchableFactory factory = new UrlBasedOpenSearchableFactory(settings);

			var os = OpenSearchFactory.FindOpenSearchable(settings, new Uri("https://catalog.terradue.com/sentinel1/description"));

            var parameters = new NameValueCollection();

            parameters.Set("{http://a9.com/-/spec/opensearch/1.1/}count", "2");

			var results = ose.Query(os, parameters);

            Assert.AreEqual(2, results.Count);

        }

        [Test()]
        public void UidTest()
        {

            OpenSearchEngine ose = new OpenSearchEngine();
            ose.LoadPlugins();

            ose.RegisterPostSearchFilter(MetricFactory.GenerateBasicMetrics);

            var settings = new OpenSearchableFactorySettings(ose);
            settings.ParametersKeywordsTable.Add("uid", "{http://a9.com/-/opensearch/extensions/geo/1.0/}uid");
            settings.ReportMetrics = true;

            UrlBasedOpenSearchableFactory factory = new UrlBasedOpenSearchableFactory(settings);

            var os = OpenSearchFactory.FindOpenSearchable(settings, new Uri("https://catalog.terradue.com//sentinel1/series/GRD/search?format=atom&uid=S1A_IW_GRDH_1SDV_20160719T181151_20160719T181219_012221_012F93_3E0B"), null);

            var parameters = new NameValueCollection();

            var results = ose.Query(os, parameters);

            Assert.AreEqual(1, results.Count);

            Assert.AreEqual("S1A_IW_GRDH_1SDV_20160719T181151_20160719T181219_012221_012F93_3E0B", results.Items.First().Identifier);

            var test = results.SerializeToString();

            Assert.IsTrue(results.SerializeToString().Contains("<t2m:Metrics xmlns:t2m=\"http://www.terradue.com/metrics\">"));

            var metricsArray = results.ElementExtensions.ReadElementExtensions<Metrics>("Metrics", "http://www.terradue.com/metrics", MetricFactory.Serializer);
            Assert.IsTrue(metricsArray.Count == 1);

            Assert.AreEqual(3, metricsArray.First().Metric.Count());

        }

        [Test()]
        public void LoopTest()
        {

            OpenSearchEngine ose = new OpenSearchEngine();
            ose.LoadPlugins();

            ose.RegisterPostSearchFilter(MetricFactory.GenerateBasicMetrics);

            var settings = new OpenSearchableFactorySettings(ose);

            settings.ReportMetrics = true;

            UrlBasedOpenSearchableFactory factory = new UrlBasedOpenSearchableFactory(settings);

            var os = OpenSearchFactory.FindOpenSearchable(settings, new Uri("https://catalog.terradue.com/sentinel3/series/SR_2_LAN/description"), null);

            var parameters = new NameValueCollection();
            parameters.Set("bbox", "-0.526617333123,7.94325361219,-0.00986752860266,8.49324877028");
            parameters.Set("track", "57");
            parameters.Set("time:start", "2016-01-01");
            parameters.Set("time:end", "2019-04-01");
            parameters.Set("startIndex", "21");

            var results = ose.Query(os, parameters);

            Assert.AreEqual(20, results.Count);
            Assert.Greater(results.TotalResults, 2000);

            //parameters.Set("startIndex", "21");
            //results = ose.Query(os, parameters);
            //Assert.AreEqual(20, results.Count);
        }

        [Test()]
        public void Uid2Test()
        {

            OpenSearchEngine ose = new OpenSearchEngine();
            ose.LoadPlugins();

            ose.RegisterPostSearchFilter(MetricFactory.GenerateBasicMetrics);

            var settings = new OpenSearchableFactorySettings(ose);
            
            settings.ReportMetrics = true;
            InitializeParametersKeywordsTable(settings.ParametersKeywordsTable);

            UrlBasedOpenSearchableFactory factory = new UrlBasedOpenSearchableFactory(settings);

            var os = OpenSearchFactory.FindOpenSearchable(settings, new Uri("https://catalog.terradue.com/esar/description"), null);

            var parameters = new NameValueCollection();
            parameters.Set("uid", "ASA_IMS_1PNESA20061204_024611_000000152053_00290_24893_0000");

            var results = ose.Query(os, parameters);

            Assert.AreEqual(1, results.Count);
            Assert.AreEqual(results.TotalResults, 1);

        }

        private void InitializeParametersKeywordsTable(Dictionary<string, string> table) {
            table["update"] = "{http://purl.org/dc/terms/}modified";
            table["updated"] = "{http://purl.org/dc/terms/}modified";
            table["modified"] = "{http://purl.org/dc/terms/}modified";
            table["do"] = "{http://www.terradue.com/opensearch}downloadOrigin";
            table["from"] = "{http://a9.com/-/opensearch/extensions/eo/1.0/}accessedFrom";
            table["start"] = "{http://a9.com/-/opensearch/extensions/time/1.0/}start";
            table["stop"] = "{http://a9.com/-/opensearch/extensions/time/1.0/}end";
            table["end"] = "{http://a9.com/-/opensearch/extensions/time/1.0/}end";
            table["trel"] = "{http://a9.com/-/opensearch/extensions/time/1.0/}relation";
            table["box"] = "{http://a9.com/-/opensearch/extensions/geo/1.0/}box";
            table["bbox"] = "{http://a9.com/-/opensearch/extensions/geo/1.0/}box";
            table["geom"] = "{http://a9.com/-/opensearch/extensions/geo/1.0/}geometry";
            table["geometry"] = "{http://a9.com/-/opensearch/extensions/geo/1.0/}geometry";
            table["uid"] = "{http://a9.com/-/opensearch/extensions/geo/1.0/}uid";
            table["id"] = "{http://purl.org/dc/terms/}identifier";
            table["rel"] = "{http://a9.com/-/opensearch/extensions/geo/1.0/}relation";
            table["cat"] = "{http://purl.org/dc/terms/}subject";
            table["pt"] = "{http://a9.com/-/opensearch/extensions/eo/1.0/}productType";
            table["psn"] = "{http://a9.com/-/opensearch/extensions/eo/1.0/}platform";
            table["psi"] = "{http://a9.com/-/opensearch/extensions/eo/1.0/}platformSerialIdentifier";
            table["isn"] = "{http://a9.com/-/opensearch/extensions/eo/1.0/}instrument";
            table["sensor"] = "{http://a9.com/-/opensearch/extensions/eo/1.0/}sensorType";
            table["st"] = "{http://a9.com/-/opensearch/extensions/eo/1.0/}sensorType";
            table["od"] = "{http://a9.com/-/opensearch/extensions/eo/1.0/}orbitDirection";
            table["ot"] = "{http://a9.com/-/opensearch/extensions/eo/1.0/}orbitType";
            table["title"] = "{http://a9.com/-/opensearch/extensions/eo/1.0/}title";
            table["track"] = "{http://a9.com/-/opensearch/extensions/eo/1.0/}track";
            table["frame"] = "{http://a9.com/-/opensearch/extensions/eo/1.0/}frame";
            table["swath"] = "{http://a9.com/-/opensearch/extensions/eo/1.0/}swathIdentifier";
            table["cc"] = "{http://a9.com/-/opensearch/extensions/eo/1.0/}cloudCover";
            table["lc"] = "{http://www.terradue.com/opensearch}landCover";
            table["dcg"] = "{http://www.terradue.com/opensearch}doubleCheckGeometry";
        }


        //[Test()]
        //public void NextGeossTest()
        //{

        //    OpenSearchEngine ose = new OpenSearchEngine();
        //    ose.LoadPlugins();

        //    var settings = new OpenSearchableFactorySettings(ose);
        //    settings.SkipCertificateVerification = true;
        //    UrlBasedOpenSearchableFactory factory = new UrlBasedOpenSearchableFactory(settings);

        //    var os = OpenSearchFactory.FindOpenSearchable(settings, new Uri("https://147.228.242.207/opensearch/description.xml?osdd=SENTINEL1_L1_SLC"));

        //    var parameters = new NameValueCollection();

        //    parameters.Set("{http://a9.com/-/spec/opensearch/1.1/}count", "2");

        //    var results = ose.Query(os, parameters);

        //    Assert.AreEqual(2, results.Count);

        //}

    }
}
