//
//  HttpOpenSearchRequest.cs
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
using Terradue.OpenSearch.Response;
using Terradue.OpenSearch.Engine;
using System.IO;
using System.Threading.Tasks;

namespace Terradue.OpenSearch.Request {

    /// <summary>
    /// Implements an OpenSearch request over HTTP
    /// </summary>
    public class HttpOpenSearchRequest : OpenSearchRequest {

        private log4net.ILog log = log4net.LogManager.GetLogger
            (System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        string contentType;

        int timeOut = 10000;

        /// <summary>
        /// Initializes a new instance of the <see cref="Terradue.OpenSearch.HttpOpenSearchRequest"/> class.
        /// </summary>
        /// <param name="url">the HTTP URL.</param>
        internal HttpOpenSearchRequest(OpenSearchUrl url, string contentType = null) : base(url, contentType) {
            this.contentType = contentType;
            if (!url.Scheme.StartsWith("http"))
                throw new InvalidOperationException("A http scheme is expected for this kind of request");
            this.OpenSearchUrl = url;
            this.originalParameters = HttpUtility.ParseQueryString(url.Query);

        }

        /// <summary>
        /// Gets or sets the HTTP requesttime out.
        /// </summary>
        /// <value>The time out.</value>
        public int TimeOut {
            get {
                return timeOut;
            }
            set {
                timeOut = value;
            }
        }

        #region implemented abstract members of OpenSearchRequest

        /// <summary>
        /// Gets the HTTP response.
        /// </summary>
        /// <returns>The response.</returns>
        public override IOpenSearchResponse GetResponse() {

            int retry = 2;

            while (retry >= 0) {
                try {
                    Stopwatch sw = Stopwatch.StartNew();
                    byte[] data;
                    MemoryOpenSearchResponse response;

                    HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(this.OpenSearchUrl);

                    if (contentType != null) {
                        ((HttpWebRequest)httpWebRequest).Accept = contentType;
                    }
                    httpWebRequest.Timeout = timeOut;
                    httpWebRequest.Proxy = null;

                    log.DebugFormat("Querying {0}", this.OpenSearchUrl);

                    using (HttpWebResponse webResponse = (HttpWebResponse)httpWebRequest.GetResponse())
                    {

                        using (var ms = new MemoryStream())
                        {
                            webResponse.GetResponseStream().CopyTo(ms);
                            ms.Flush();
                            data = ms.ToArray();
                        }

                        response = new MemoryOpenSearchResponse(data, webResponse.ContentType, sw.Elapsed);
                    }


                    sw.Stop();
                    return response;

                } catch (WebException e) {
                    if (e.Status == WebExceptionStatus.Timeout)
                        throw new TimeoutException(String.Format("Search Request {0} has timed out", this.OpenSearchUrl), e);
                    retry--;
                    if (retry > 0) {
                        Thread.Sleep(1000);
                        continue;
                    }
                    throw e;
                } catch (Exception e) {
                    throw new Exception("Unknown error during query at " + this.OpenSearchUrl, e);
                }
            }

            throw new Exception("Unknown error during query at " + this.OpenSearchUrl);
        }

        NameValueCollection originalParameters = new NameValueCollection();

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

