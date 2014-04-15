//
//  GeoSpatialTemporalOpenSearchExtension.cs
//
//  Author:
//       Emmanuel Mathot <emmanuel.mathot@terradue.com>
//
//  Copyright (c) 2014 Terradue

using System;
using System.ServiceModel.Syndication;
using System.Collections.Generic;
using System.Xml;
using Terradue.OpenSearch.Result;

namespace Terradue.OpenSearch.Engine.Extensions {

    /// <summary>
    /// Helper class that implements rules of Geo and Time OpenSearch extensions
    /// </summary>
    public static class GeoSpatialTemporalOpenSearchExtension {

        /// <summary>
        /// Gets the local identifier.
        /// </summary>
        /// <returns>The local identifier.</returns>
        /// <param name="item">Item.</param>
        public static string GetLocalIdentifier(SyndicationItem item){
            var elem = item.ElementExtensions.ReadElementExtensions<string>("identifier", "http://purl.org/dc/elements/1.1/");
            if (elem.Count == 0) return null;
            return elem[0];
        }

        /// <summary>
        /// Gets the local identifier.
        /// </summary>
        /// <returns>The local identifier.</returns>
        /// <param name="item">Item.</param>
        public static string GetLocalIdentifier(IOpenSearchResultItem item){
            return item.Identifier;
        }

    }
}

