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

namespace Terradue.OpenSearch.Request {
    /// <summary>
    /// Base class to represent an OpenSearch request.
    /// </summary>
    public abstract class OpenSearchRequest {
        OpenSearchUrl url;

        NameValueCollection originalParameters;

        string contentType;

        /// <summary>
        /// Initializes a new instance of the <see cref="Terradue.OpenSearch.OpenSearchRequest"/> class.
        /// </summary>
        /// <param name="url">URL.</param>
        protected OpenSearchRequest(OpenSearchUrl url, string contentType) {
            this.url = url;
            this.contentType = contentType;
        }

        /// <summary>
        /// Gets the parameters of the request.
        /// </summary>
        /// <value>The parameters.</value>
        public NameValueCollection Parameters {
            get {
                return url.SearchAttributes;
            }
        }

        public NameValueCollection OriginalParameters {
            get {
                return originalParameters;
            }
			protected set {
				originalParameters = value;
			}
        }

        public string ContentType {
            get {
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
        public static OpenSearchRequest Create(IOpenSearchable entity, string mimeType, NameValueCollection parameters) {

            OpenSearchDescription osd = entity.GetOpenSearchDescription();

            OpenSearchDescriptionUrl url = OpenSearchFactory.GetOpenSearchUrlByType(osd, mimeType);

            if (url == null) throw new InvalidOperationException(string.Format("Could not find a URL template for entity {0} with type {1}", entity.Identifier, mimeType));

            if (url.Type == "application/x-parameters+json") {
                return new ParametersOpenSearchRequest(entity);
            }

            OpenSearchUrl queryUrl = OpenSearchFactory.BuildRequestUrlForTemplate(url, parameters, entity.GetOpenSearchParameters(mimeType));

            OpenSearchRequest request = null;

            switch (queryUrl.Scheme) {
                case "http":
                case "https":
                    request = new HttpOpenSearchRequest(queryUrl, mimeType);
                    break;
                case "file":
                    request = new FileOpenSearchRequest(queryUrl, mimeType);
                    break;
            }

            request.originalParameters = parameters;

            return request;

        }


        public static OpenSearchRequest Create(OpenSearchUrl queryUrl) {

            OpenSearchRequest request = null;

            switch (queryUrl.Scheme) {
                case "http":
                case "https":
                    request = new HttpOpenSearchRequest(queryUrl);
                    break;
                case "file":
                    request = new FileOpenSearchRequest(queryUrl, "");
                    break;
            }

            return request;

        }

        public virtual OpenSearchUrl OpenSearchUrl {
            get {
                return url;
            }
            protected set {
                url = value;
            }
        }

        public abstract IOpenSearchResponse GetResponse();
    }
}

