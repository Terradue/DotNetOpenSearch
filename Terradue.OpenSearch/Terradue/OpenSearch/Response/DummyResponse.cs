//
//  GenericOpenSearchable.cs
//
//  Author:
//       Emmanuel Mathot <emmanuel.mathot@terradue.com>
//
//  Copyright (c) 2014 Terradue

using System;
using System.Collections.Generic;
using Terradue.OpenSearch.Benchmarking;
using Terradue.OpenSearch.Response;
using Terradue.OpenSearch.Result;

namespace Terradue.OpenSearch
{
    internal class DummyResponse : OpenSearchResponse<AtomFeed>
    {

        public DummyResponse(AtomFeed result) : base(result) {

        }

        #region implemented abstract members of OpenSearchResponse

        public override object GetResponseObject()
        {

            return payload;
        }

        public override IEnumerable<Metric> Metrics
        {
            get
            {
                return new List<Metric>();
            }
        }

        public override string ContentType
        {
            get
            {
                return "application/atom+xml";
            }
        }

        public override IOpenSearchResponse CloneForCache()
        {
            AtomOpenSearchResponse aosr = new AtomOpenSearchResponse(new AtomFeed(payload, true));
            aosr.Entity = this.Entity;
            return aosr;
        }

        #endregion
    }
}