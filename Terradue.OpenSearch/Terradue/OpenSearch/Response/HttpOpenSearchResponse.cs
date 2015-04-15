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
using System.IO;

namespace Terradue.OpenSearch.Response {
    /// <summary>
    /// OpenSearchResponse from a HTTP request
    /// </summary>
    public class HttpOpenSearchResponse : OpenSearchResponse<byte[]> {

        HttpWebResponse webResponse;
        TimeSpan requestTime;

        /// <summary>
        /// Initializes a new instance of the <see cref="Terradue.OpenSearch.HttpOpenSearchResponse"/> class.
        /// </summary>
        /// <param name="resp">Resp.</param>
        /// <param name="time">Time.</param>
        internal HttpOpenSearchResponse(HttpWebResponse resp, TimeSpan time){
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

        public override object GetResponseObject() {
            if (payload == null) {
                using (var ms = new MemoryStream()) {
                    webResponse.GetResponseStream().CopyTo(ms);
                    payload = ms.ToArray();
                }
            }
            return payload;
        }

        public override IOpenSearchResponse CloneForCache() {
            GetResponseObject();
            MemoryOpenSearchResponse mosr = new MemoryOpenSearchResponse((byte[])payload.Clone(), ContentType);
            mosr.Entity = this.Entity;
            return mosr;
        }

        #endregion
    }
}

