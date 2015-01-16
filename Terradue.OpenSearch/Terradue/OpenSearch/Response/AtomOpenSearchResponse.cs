//
//  AtomOpenSearchResponse.cs
//
//  Author:
//       Emmanuel Mathot <emmanuel.mathot@terradue.com>
//
//  Copyright (c) 2014 Terradue

using System;
using System.Collections.Specialized;
using System.Threading;
using Terradue.ServiceModel.Syndication;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Diagnostics;
using Terradue.OpenSearch.Result;

namespace Terradue.OpenSearch.Response
{
    public class AtomOpenSearchResponse : MemoryOpenSearchResponse
	{

        AtomFeed result;

        public AtomOpenSearchResponse(AtomFeed result, TimeSpan timeSpan) : base("application/atom+xml") {

            this.timeSpan = timeSpan;
            this.result = result;

            Stopwatch sw = new Stopwatch();
            sw.Start();
            MemoryStream responseStream = new MemoryStream();
            var writer = XmlWriter.Create(responseStream);
            Atom10FeedFormatter atomFormatter = new Atom10FeedFormatter(result.Feed);
            atomFormatter.WriteTo(writer);
            writer.Flush();
            writer.Close();
            responseStream.Seek(0, SeekOrigin.Begin);
            response = responseStream.ToArray();
            sw.Start();
            this.timeSpan += sw.Elapsed;

		}

		#region implemented abstract members of OpenSearchResponse

        public override object GetResponseObject() {

            return response;
		}

		public override TimeSpan RequestTime {
			get {
                return this.timeSpan;
			}
		}

		#endregion
	}
}

