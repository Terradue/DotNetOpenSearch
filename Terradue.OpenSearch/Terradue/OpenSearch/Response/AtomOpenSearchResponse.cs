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
using Terradue.OpenSearch.Benchmarking;
using System.Linq;

namespace Terradue.OpenSearch.Response
{
    public class AtomOpenSearchResponse : OpenSearchResponse<AtomFeed>
	{


        public AtomOpenSearchResponse(AtomFeed result) : base(result) {

		}

		#region implemented abstract members of OpenSearchResponse

        public override object GetResponseObject() {

            return payload;
		}

		public override IEnumerable<Metric> Metrics {
			get {
                var metrics = base.payload.ElementExtensions.ReadElementExtensions<Metrics>("Metrics", "http://www.terradue.com/metrics", MetricFactory.Serializer);
                if (metrics.Count() > 0)
                    return metrics.First().Metric;
                return new List<Metric>();
            }
		}

        public override string ContentType {
            get {
                return "application/atom+xml";
            }
        }

        public override IOpenSearchResponse CloneForCache() {
            AtomOpenSearchResponse aosr = new AtomOpenSearchResponse(new AtomFeed(payload, true));
            aosr.Entity = this.Entity;
            return aosr;
        }

        #endregion
	}
}

