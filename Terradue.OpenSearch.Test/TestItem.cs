using System;
using Terradue.OpenSearch.Engine.Extensions;
using Terradue.OpenSearch.Request;
using System.Collections.Specialized;
using System.IO;
using Terradue.OpenSearch.Result;
using System.Collections.Generic;
using Terradue.ServiceModel.Syndication;
using System.Xml;
using System.Web;
using System.Linq;
using Terradue.OpenSearch.Schema;
using Terradue.OpenSearch.Filters;

namespace Terradue.OpenSearch.Test {

    public class TestItem {
        int i;

        public TestItem(int i) {
            this.i = i;
        }

        public string Id {
            get {

                return "file:///test/search?id=" + Identifier;

            }
        }

        public string Identifier {
            get;
            set;
        }

        public string Name {
            get;
            set;
        }

        public string TextContent {
            get;
            set;
        }

        public TimeSpan Shift {
            get;
            set;
        }

        public DateTimeOffset Date {
            get {
                return new DateTime(1900, 01, 01, 01, 01, 01).Add(Shift);
            }
        }
    }
}
