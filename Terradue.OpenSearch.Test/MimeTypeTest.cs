using System;
using Terradue.OpenSearch.Engine;
using Xunit;

namespace Terradue.OpenSearch.Test
{
    public class MimeTypeTest {


        [Fact(DisplayName = "MimeType Test #1")]
        [Trait("Category", "unit")]
        public void MimeTypeTest1() {


			MimeType type = MimeType.CreateFromContentType("application/atom+xml");
			MimeType type2 = MimeType.CreateFromContentType("application/atom+xml; charset=UTF-8");

			Assert.True(MimeType.Equals(type, type2));
        }

        [Fact(DisplayName = "MimeType Test #2")]
        [Trait("Category", "unit")]
        public void MimeTypeTest2()
        {
            OpenSearchEngine ose = new OpenSearchEngine();
            ose.LoadPlugins();

            var settings = new OpenSearchableFactorySettings(ose);
            var url = new Uri("https://catalog.terradue.com/sentinel1/search");
            var e = OpenSearchFactory.FindOpenSearchable(settings, url, "application/atom+xml; profile=http://earth.esa.int/eop/2.1");

            Assert.True(e.GetOpenSearchDescription().Url[0].ExtraNamespace.Count > 1, "More than 1 extra namespace");

            Assert.Equal("application/atom+xml; profile=http://earth.esa.int/eop/2.1", e.DefaultMimeType);
        }
    }
}

