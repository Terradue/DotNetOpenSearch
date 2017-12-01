//
//  GenericOpenSearchable.cs
//
//  Author:
//       Emmanuel Mathot <emmanuel.mathot@terradue.com>
//
//  Copyright (c) 2014 Terradue

using System.Collections.Specialized;
using Terradue.OpenSearch.Request;
using Terradue.OpenSearch.Response;

namespace Terradue.OpenSearch
{
    internal class DummyRequest : OpenSearchRequest
    {
        public DummyRequest(OpenSearchUrl url, string contentType) : base(url, contentType)
        {
        }

        public override NameValueCollection OriginalParameters { get { return new NameValueCollection(); } set { } }

        public override IOpenSearchResponse GetResponse()
        {
            return new DummyResponse(new Result.AtomFeed(), new System.TimeSpan());
        }
    }
}