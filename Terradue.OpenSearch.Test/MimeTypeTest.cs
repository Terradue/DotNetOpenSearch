using NUnit.Framework;
using System;
using log4net.Config;
using System.IO;
using Terradue.OpenSearch.Engine;
using Terradue.OpenSearch.Filters;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace Terradue.OpenSearch.Test {
    [TestFixture()]
    public class MimeTypeTest {


        [Test()]
        public void MimeTypeTest1() {


			MimeType type = MimeType.CreateFromContentType("application/atom+xml");
			MimeType type2 = MimeType.CreateFromContentType("application/atom+xml; charset=UTF-8");

			Assert.IsTrue(MimeType.Equals(type, type2));
        }

        [Test()]
        public void MimeTypeTest2()
        {
            OpenSearchEngine ose = new OpenSearchEngine();
            ose.LoadPlugins();

            var settings = new OpenSearchableFactorySettings(ose);
            var url = new Uri("https://catalog.terradue.com/sentinel1/search");
            var e = OpenSearchFactory.FindOpenSearchable(settings, url, "application/atom+xml; profile=http://earth.esa.int/eop/2.1");

            Assert.Greater(e.GetOpenSearchDescription().Url[0].ExtraNamespace.Count, 1);

            Assert.AreEqual("application/atom+xml; profile=http://earth.esa.int/eop/2.1", e.DefaultMimeType);
        }
    }
}

