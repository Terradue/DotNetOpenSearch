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
    }
}

