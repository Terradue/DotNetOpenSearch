using System;
using System.Collections.Specialized;
using System.Threading;
using System.ServiceModel.Syndication;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Diagnostics;

namespace Terradue.OpenSearch
{
    public class AtomOpenSearchResponse : MemoryOpenSearchResponse
	{

		SyndicationFeed result;

        public AtomOpenSearchResponse(SyndicationFeed result) : base(new MemoryStream(),"application/atom+xml") {

            this.result = result;

            Stopwatch sw = new Stopwatch();
            sw.Start();
            response = new MemoryStream();
            var writer = XmlWriter.Create(response);
            Atom10FeedFormatter atomFormatter = new Atom10FeedFormatter(result);
            atomFormatter.WriteTo(writer);
            writer.Flush();
            writer.Close();
            response.Seek(0, SeekOrigin.Begin);
            sw.Start();
            timeSpan = sw.Elapsed;

		}

		#region implemented abstract members of OpenSearchResponse

		public override System.IO.Stream GetResponseStream() {

            return response;
		}

		public override TimeSpan RequestTime {
			get {
				return new TimeSpan(0);
			}
		}

		#endregion
	}
}

