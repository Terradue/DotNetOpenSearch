//
//  IOpenSearchEngineExtension.cs
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

[assembly:AddinRoot("OpenSearchEngine", "1.0")]
[assembly:AddinDescription("OpenSearch Engine")]
namespace Terradue.OpenSearch {
    /// <summary>
    /// Set of methods and properties for implementing an OpenSearchEngineExtension.
    /// </summary>
    /// <description>
    /// This is also a TypeExtensionPoint for declaring extensions and discovering them
    /// with mono.addins
    /// </description>
    [TypeExtensionPoint()]
    public interface IOpenSearchEngineExtension {

        /// <summary>
        /// Gets the main Type of object that the extension generates
        /// </summary>
        /// <returns>The transform type.</returns>
        Type GetTransformType();

        /// <summary>
        /// Identifier of the extension
        /// </summary>
        /// <value>The identifier.</value>
        string Identifier { get; }

        /// <summary>
        /// Main function to transform an OpenSearchResponse into the declared type
        /// </summary>
        /// <returns>The response transformed into the declared Type.</returns>
        /// <param name="response">OpenSearchResponse.</param>
        IOpenSearchResultCollection ReadNative(OpenSearchResponse response);

        IOpenSearchResultCollection CreateOpenSearchResultFromOpenSearchResult(IOpenSearchResultCollection results);

        /// <summary>
        /// Discover the OpenSearchDescription Url from an OpenSearchResponse
        /// </summary>
        /// <returns>The OpenSearchDescription URL from OpenSearchResponse.</returns>
        /// <param name="response">OpenSearchResponse.</param>
        OpenSearchUrl FindOpenSearchDescriptionUrlFromResponse(OpenSearchResponse response);

        /// <summary>
        /// Gets the contentType that the extension is able to read to discover the OpenSearchDescription.
        /// </summary>
        /// <value>The type of the discovery content.</value>
        string DiscoveryContentType { get; }

        /// <summary>
        /// Gets the enclosures of an OpenSearch result 
        /// </summary>
        /// <returns>The enclosures in the format of SyndicationLink.</returns>
        /// <param name="result">OpenSearchResult.</param>
        SyndicationLink[] GetEnclosures(IOpenSearchResult result);

        /// <summary>
        /// Gets the name of the extension.
        /// </summary>
        /// <value>The name.</value>
        string Name { get; }
    }
}

