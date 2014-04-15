//
//  HttpOpenSearchResponse.cs
//
//  Author:
//       Emmanuel Mathot <emmanuel.mathot@terradue.com>
//
//  Copyright (c) 2014 Terradue

using System;
using Mono.Addins;
using System.Collections.Generic;
using System.Net;
using System.Xml;
using System.Diagnostics;
using Terradue.OpenSearch.Result;

namespace Terradue.OpenSearch.Response {
    /// <summary>
    /// OpenSearchResponse from a HTTP request
    /// </summary>
    public class HttpOpenSearchResponse : OpenSearchResponse {
        HttpWebResponse webResponse;
        TimeSpan requestTime;

        /// <summary>
        /// Initializes a new instance of the <see cref="Terradue.OpenSearch.HttpOpenSearchResponse"/> class.
        /// </summary>
        /// <param name="resp">Resp.</param>
        /// <param name="time">Time.</param>
        internal HttpOpenSearchResponse(HttpWebResponse resp, TimeSpan time) {
            webResponse = resp;
            requestTime = time;
        }

        #region implemented abstract members of OpenSearchResponse

        public override string ContentType {
            get {
                if (webResponse == null) return null;
                return webResponse.ContentType.Split(';')[0];
            }
        }

        public override TimeSpan RequestTime {
            get {
                return requestTime;
            }
        }

        public override System.IO.Stream GetResponseStream() {
            return webResponse.GetResponseStream();
        }

        #endregion
    }
}

