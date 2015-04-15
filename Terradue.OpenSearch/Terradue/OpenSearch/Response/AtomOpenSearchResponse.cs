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
    public class AtomOpenSearchResponse : OpenSearchResponse<AtomFeed>
	{

        protected TimeSpan timeSpan;

        public AtomOpenSearchResponse(AtomFeed result, TimeSpan timeSpan) : base(result) {

            this.timeSpan = timeSpan;

		}

		#region implemented abstract members of OpenSearchResponse

        public override object GetResponseObject() {

            return payload;
		}

		public override TimeSpan RequestTime {
			get {
                return this.timeSpan;
			}
		}

        public override string ContentType {
            get {
                return "application/atom+xml";
            }
        }

        public override IOpenSearchResponse CloneForCache() {
            AtomOpenSearchResponse aosr = new AtomOpenSearchResponse(new AtomFeed(payload, true), RequestTime);
            aosr.Entity = this.Entity;
            return aosr;
        }

        #endregion
	}
}

