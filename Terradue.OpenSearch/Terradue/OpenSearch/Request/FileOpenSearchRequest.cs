//
//  FileOpenSearchRequest.cs
//
//  Author:
//       Emmanuel Mathot <emmanuel.mathot@terradue.com>
//
//  Copyright (c) 2014 Terradue

using System;
using System.Collections.Specialized;
using Terradue.ServiceModel.Syndication;
using System.Net;
using System.Web;
using System.Diagnostics;
using System.Threading;
using System.IO;
using Terradue.OpenSearch.Response;

namespace Terradue.OpenSearch.Request {
    /// <summary>
    /// Implements an OpenSearch request over HTTP
    /// </summary>
    public class FileOpenSearchRequest : OpenSearchRequest {

        /// <summary>
        /// Initializes a new instance of the <see cref="Terradue.OpenSearch.HttpOpenSearchRequest"/> class.
        /// </summary>
        /// <param name="url">the HTTP URL.</param>
        internal FileOpenSearchRequest(OpenSearchUrl url, string mimeType) : base(url, mimeType) {
            if (!url.Scheme.StartsWith("file")) throw new InvalidOperationException("A file scheme is expected for this kind of request");
            this.OpenSearchUrl = url;

        }

        #region implemented abstract members of OpenSearchRequest

        /// <summary>
        /// Gets the HTTP response.
        /// </summary>
        /// <returns>The response.</returns>
        public override IOpenSearchResponse GetResponse() {

            try {
                Stopwatch sw = Stopwatch.StartNew();
                FileStream fs = new FileStream(this.OpenSearchUrl.ToString().Replace("file://",""),FileMode.Open);
                byte[] fileBytes= new byte[fs.Length];
                fs.Read(fileBytes, 0, fileBytes.Length);
                fs.Close();
                MemoryOpenSearchResponse response = new MemoryOpenSearchResponse(fileBytes, ContentType);
                sw.Stop();

                return response;

            } catch (Exception e) {
                throw new Exception("Unknown error during query at " + this.OpenSearchUrl, e);
            }
        }

        NameValueCollection originalParameters= new NameValueCollection();
        public override NameValueCollection OriginalParameters {
            get {
                return originalParameters;
            }
            set {
                originalParameters = value;
            }
        }

        #endregion
    }
}

