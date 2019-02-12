//
//  OpenSearchRequest.cs
//
//  Author:
//       Emmanuel Mathot <emmanuel.mathot@terradue.com>
//
//  Copyright (c) 2014 Terradue

using System;
using System.Collections.Specialized;
using System.Threading;
using System.Web;
using Terradue.OpenSearch.Response;
using Terradue.OpenSearch.Schema;

namespace Terradue.OpenSearch.Request
{
    /// <summary>
    /// Base class to represent an OpenSearch request.
    /// </summary>
    public abstract class OpenSearchRequest
    {
        OpenSearchUrl url;

        string contentType;

        /// <summary>
        /// Initializes a new instance of the <see cref="Terradue.OpenSearch.OpenSearchRequest"/> class.
        /// </summary>
        /// <param name="url">URL.</param>
        protected OpenSearchRequest(OpenSearchUrl url, string contentType)
        {
            this.url = url;
            this.contentType = contentType;
        }

        /// <summary>
        /// Gets the parameters of the request.
        /// </summary>
        /// <value>The parameters.</value>
        public NameValueCollection Parameters
        {
            get
            {
                return url.SearchAttributes;
            }
        }

        public abstract NameValueCollection OriginalParameters { get; set; }

        public string ContentType
        {
            get
            {
                return contentType;
            }
        }

        /// <summary>
        /// Create an OpenSearchRequest based on the IOpenSearchable entity, type and parameters.
        /// </summary>
        /// <description>
        /// This method is the basic default implementation for creating OpenSearchRequest. It supports only 2 schemes:
        /// 1) file:// that will create a FileOpenSearchRequest
        /// 2) http:// that will create an HttpOpenSearchRequest
        /// <param name="entity">IOpenSearchable Entity.</param>
        /// <param name="type">MimeType of the request.</param>
        /// <param name="parameters">Parameters of the request.</param>
        public static OpenSearchRequest Create(IOpenSearchable entity, QuerySettings querySettings, NameValueCollection parameters)
        {

            OpenSearchDescription osd = entity.GetOpenSearchDescription();

			OpenSearchDescriptionUrl templateUrl = OpenSearchFactory.GetOpenSearchUrlByType(osd, querySettings.PreferredContentType);

            if (templateUrl == null) throw new InvalidOperationException(string.Format("Could not find a URL template for entity {0} with type {1}", entity.Identifier, querySettings.PreferredContentType));

            OpenSearchUrl queryUrl = OpenSearchFactory.BuildRequestUrlFromTemplate(templateUrl, parameters, querySettings);

            OpenSearchRequest request = null;

            switch (queryUrl.Scheme)
            {
                case "http":
                case "https":
                    request = new HttpOpenSearchRequest(queryUrl, querySettings.PreferredContentType);
                    ((HttpOpenSearchRequest)request).TimeOut = 600000;
                    ((HttpOpenSearchRequest)request).Credentials = querySettings.Credentials;
                    break;
                case "file":
                    request = new FileOpenSearchRequest(queryUrl, querySettings.PreferredContentType);
                    break;
            }

            request.OriginalParameters = parameters;
			request.OpenSearchDescription = osd;

            return request;

        }


        public static OpenSearchRequest Create(OpenSearchUrl queryUrl, QuerySettings querySettings = null)
        {

            OpenSearchRequest request = null;

            switch (queryUrl.Scheme)
            {
                case "http":
                case "https":
                    request = new HttpOpenSearchRequest(queryUrl);
                    ((HttpOpenSearchRequest)request).TimeOut = 600000;
                    if (querySettings != null && querySettings.Credentials != null && querySettings.Credentials is System.Net.NetworkCredential)
                    {
                        ((HttpOpenSearchRequest)request).Credentials = querySettings.Credentials;
                        System.Net.NetworkCredential netcred = querySettings.Credentials as System.Net.NetworkCredential;
                        UriBuilder urib = new UriBuilder(request.url);
                        urib.UserName = netcred.UserName;
                        urib.Password = netcred.Password;
                        request.url = new OpenSearchUrl(urib.Uri);
                    }
                    break;
                case "file":
                    request = new FileOpenSearchRequest(queryUrl, "");
                    break;
            }

            return request;

        }

        public virtual OpenSearchUrl OpenSearchUrl
        {
            get
            {
                return url;
            }
            protected set
            {
                url = value;
            }
        }

		public OpenSearchDescription OpenSearchDescription
        {
			get; protected set;
        }

        public abstract IOpenSearchResponse GetResponse();
    }
}

