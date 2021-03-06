//
//  OpenSearchEngineExtension.cs
//
//  Author:
//       Emmanuel Mathot <emmanuel.mathot@terradue.com>
//
//  Copyright (c) 2014 Terradue

using System;
using System.Collections.Generic;
using Terradue.ServiceModel.Syndication;
using Terradue.OpenSearch.Response;
using Terradue.OpenSearch.Result;

namespace Terradue.OpenSearch.Engine {
    /// <summary>
    /// Base class for implementing a typed OpenSearchEngine extension.
    /// </summary>
    public abstract class OpenSearchEngineExtension<T> : IOpenSearchEngineExtension {
        public abstract string Name { get; }

        public virtual Type GetTransformType() {
            return typeof(T);
        }

        public abstract IOpenSearchResultCollection ReadNative(IOpenSearchResponse response);

        public abstract IOpenSearchResultCollection CreateOpenSearchResultFromOpenSearchResult(IOpenSearchResultCollection results);

        public abstract OpenSearchUrl FindOpenSearchDescriptionUrlFromResponse(IOpenSearchResponse response);

        public abstract string DiscoveryContentType { get; }

        public abstract string Identifier { get; }
    }
}

