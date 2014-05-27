//
//  OpenSearchEngineExtension.cs
//
//  Author:
//       Emmanuel Mathot <emmanuel.mathot@terradue.com>
//
//  Copyright (c) 2014 Terradue

using System;
using Mono.Addins;
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

        public abstract string[] GetInputFormatTransformPath();

        public Type GetTransformType() {
            return typeof(T);
        }

        public abstract IOpenSearchResultCollection TransformResponse(OpenSearchResponse response);

        public abstract OpenSearchUrl FindOpenSearchDescriptionUrlFromResponse(OpenSearchResponse response);

        public abstract string DiscoveryContentType { get; }

        public abstract SyndicationLink[] GetEnclosures(IOpenSearchResult result);

        public abstract string Identifier { get; }
    }
}

