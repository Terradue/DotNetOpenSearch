//
//  OpenSearchMemoryCache.cs
//
//  Author:
//       Emmanuel Mathot <emmanuel.mathot@terradue.com>
//
//  Copyright (c) 2014 Terradue

using System;
using System.Collections.Specialized;
using Terradue.OpenSearch.Request;
using Terradue.OpenSearch.Response;
using Terradue.OpenSearch.Schema;

namespace Terradue.OpenSearch.Filters
{
    /// <summary>
    /// Cached open search request.
    /// </summary>
    public class CachedOpenSearchRequest : OpenSearchRequest
    {

        private IOpenSearchResponse response;

        public CachedOpenSearchRequest(OpenSearchUrl url, IOpenSearchResponse response, NameValueCollection originalParameters, TimeSpan elapsed, OpenSearchDescription osd) : base(url, response.ContentType)
        {
            base.OpenSearchUrl = url;
            this.response = response;
            this.OriginalParameters = originalParameters;
			this.OpenSearchDescription = osd;
        }

        #region implemented abstract members of OpenSearchRequest

        public override IOpenSearchResponse GetResponse()
        {
            return response;
        }

        NameValueCollection originalParameters;
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

        #endregion
    }

}

