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
using Terradue.OpenSearch.Benchmarking;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace Terradue.OpenSearch.Request
{

    /// <summary>
    /// Implements an OpenSearch request over HTTP
    /// </summary>
    public class HttpOpenSearchRequest : OpenSearchRequest
    {

        private log4net.ILog log = log4net.LogManager.GetLogger
            (System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        string contentType;

        int timeOut = 10000;

        /// <summary>
        /// Initializes a new instance of the <see cref="Terradue.OpenSearch.HttpOpenSearchRequest"/> class.
        /// </summary>
        /// <param name="url">the HTTP URL.</param>
        internal HttpOpenSearchRequest(OpenSearchUrl url, string contentType = null) : base(url, contentType)
        {
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
        public int TimeOut
        {
            get
            {
                return timeOut;
            }
            set
            {
                timeOut = value;
            }
        }

        #region implemented abstract members of OpenSearchRequest

        /// <summary>
        /// Gets the HTTP response.
        /// </summary>
        /// <returns>The response.</returns>
        public override IOpenSearchResponse GetResponse()
        {

            int retry = RetryNumber;

            while (retry >= 0)
            {
                try
                {

                    byte[] data;
                    MemoryOpenSearchResponse response;

                    HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(this.OpenSearchUrl);

                    if (SkipCertificateVerification)
                    {
                        httpWebRequest.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
                    }

                    if (contentType != null)
                    {
                        ((HttpWebRequest)httpWebRequest).Accept = contentType;
                    }
                    httpWebRequest.Timeout = timeOut;
                    httpWebRequest.Proxy = null;
                    httpWebRequest.Credentials = Credentials;
                    httpWebRequest.PreAuthenticate = true;
                    httpWebRequest.AllowAutoRedirect = true;
                    SetBasicAuthHeader(httpWebRequest, (NetworkCredential)Credentials);

                    log.DebugFormat("Querying (Try={1}) {0}", this.OpenSearchUrl, retry);

                    Stopwatch sw2 = new Stopwatch();
                    Stopwatch sw = Stopwatch.StartNew();
                    DateTime beginGetResponseTime = DateTime.UtcNow;
                    DateTime endGetResponseTime = DateTime.UtcNow;

                    return Task.Factory.FromAsync((asyncCallback, state) => {
                         var asyncResult = httpWebRequest.BeginGetResponse(asyncCallback, state);
                         log.DebugFormat("Connected to {0}", this.OpenSearchUrl.Host);
                         beginGetResponseTime = DateTime.UtcNow;
                         sw2.Start();
                         return asyncResult;
                        }, httpWebRequest.EndGetResponse, null)
                    .ContinueWith(resp =>
                    {
                        sw2.Stop();
                        endGetResponseTime = DateTime.UtcNow;
                        log.DebugFormat("Reply from {0}", this.OpenSearchUrl.Host);
                        Metric responseTime = new LongMetric("responseTime", sw.ElapsedMilliseconds, "ms", "Response time of the remote server to answer the query");
                        Metric beginGetResponseTimeMetric = new LongMetric("beginGetResponseTime", beginGetResponseTime.Ticks, "ticks", "Begin time of the get response from remote server to answer the query");
                        Metric endGetResponseTimeMetric = new LongMetric("endGetResponseTime", endGetResponseTime.Ticks, "ticks", "End time of the get response from remote server to answer the query");

                        using (HttpWebResponse webResponse = (HttpWebResponse)resp.Result)
                        {
                            using (var ms = new MemoryStream())
                            {
                                webResponse.GetResponseStream().CopyTo(ms);
                                ms.Flush();
                                data = ms.ToArray();
                            }
                            sw.Stop();
                            Metric requestTime = new LongMetric("requestTime", sw.ElapsedMilliseconds, "ms", "Request time for retrieveing the query");
                            Metric retryNumber = new LongMetric("retryNumber", RetryNumber - retry, "#", "Number of retry to have the query completed");
                            response = new MemoryOpenSearchResponse(data, webResponse.ContentType, new List<Metric>() { responseTime, requestTime, retryNumber });
                        }

                        return response;
                    }).Result;

                }
                catch (AggregateException ae)
                {
                    log.DebugFormat("Error during query at {0} : {1}.", this.OpenSearchUrl, ae.InnerException.Message);
                    if ( ae.InnerException is WebException && ((WebException)ae.InnerException).Response is HttpWebResponse ){
                        var resp = ((WebException)ae.InnerException).Response as HttpWebResponse ;
                        if ( resp.StatusCode == HttpStatusCode.BadRequest ||
                            resp.StatusCode == HttpStatusCode.Unauthorized ||
                            resp.StatusCode == HttpStatusCode.Forbidden ||
                            resp.StatusCode == HttpStatusCode.NotFound ||
                            resp.StatusCode == HttpStatusCode.MethodNotAllowed
                        )
                            throw ae.InnerException;
                    }
                    retry--;
                    if (retry > 0)
                    {
                        log.DebugFormat("Retrying in 3 seconds...");
                        Thread.Sleep(3000);
                        continue;
                    }
                    throw ae.InnerException;
                }
            }

            throw new Exception("Unknown error during query at " + this.OpenSearchUrl);
        }

        public void SetBasicAuthHeader(WebRequest request, NetworkCredential creds)
        {
            if (creds == null) return;
            string authInfo = creds.UserName + ":" + creds.Password;
            authInfo = Convert.ToBase64String(Encoding.Default.GetBytes(authInfo));
            request.Headers["Authorization"] = "Basic " + authInfo;
        }

        NameValueCollection originalParameters = new NameValueCollection();

        public override NameValueCollection OriginalParameters
        {
            get
            {
                return originalParameters;
            }
            set
            {
                originalParameters = value;
            }
        }

        public ICredentials Credentials { get; set; }

        #endregion


        private bool skipCertificateVerification = false;
        public bool SkipCertificateVerification
        {
            get
            {
                return skipCertificateVerification;
            }
            set
            {
                skipCertificateVerification = value;

            }
        }

        public int RetryNumber { get; internal set; }
    }
}

