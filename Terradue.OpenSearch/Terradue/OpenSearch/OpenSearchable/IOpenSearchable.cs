//
//  IOpenSearchable.cs
//
//  Author:
//       Emmanuel Mathot <emmanuel.mathot@terradue.com>
//
//  Copyright (c) 2014 Terradue
//

using System;
using System.Collections.Specialized;
using System.Collections.Generic;
using Terradue.OpenSearch.Request;
using Terradue.OpenSearch.Engine;
using Terradue.OpenSearch.Response;
using Terradue.OpenSearch.Result;
using Terradue.OpenSearch.Schema;

namespace Terradue.OpenSearch {
    /// <summary>
    /// IOpenSearchable.
    /// </summary>
    /// <description>Internal interface that enables OpenSearch mechanism on its class</description>
    public partial interface IOpenSearchable {

        /// <summary>
        /// Get the transform function according to the type of the result requested. OpenSearch is passed as argument
        /// to be used with the default functions in OpenSearchEngine and OpenSearchFactory
        /// </summary>
        /// <seealso cref="OpenSearchEngine.GetExtension"/>
        /// <seealso cref="OpenSearchFactory.BestTransformFunctionByNumberOfParam"/> 
        /// <returns>A tuple with the transform function and the mime-type that will be read as input</returns>
        /// <param name="ose">OpenSearchEngine instance</param>
        /// <param name="resultType">Result type</param>
        Tuple<string, Func<OpenSearchResponse, IOpenSearchResultCollection>> GetTransformFunction(OpenSearchEngine ose, Type resultType);

        /// <summary>
        /// Get the transform function according to the entity. OpenSearch is passed as argument
        /// to be used with the default functions in OpenSearchEngine and OpenSearchFactory
        /// </summary>
        /// <seealso cref="OpenSearchEngine.GetExtension"/>
        /// <seealso cref="OpenSearchFactory.BestTransformFunctionByNumberOfParam"/> 
        /// <returns>A tuple with the transform function and the mime-type that will be read as input</returns>
        /// <param name="ose">OpenSearchEngine instance</param>
        Tuple<string, Func<OpenSearchResponse, IOpenSearchResultCollection>> GetTransformFunction(OpenSearchEngine ose);

        /// <summary>
        /// Create the OpenSearch Request for the requested mime-type the specified type and parameters.
        /// </summary>
        /// <param name="mimetype">Mime-Type requested to the OpenSearchable entity</param>
        /// <param name="parameters">Parameters of the request</param>
        OpenSearchRequest Create(string mimetype, NameValueCollection parameters);

        /// <summary>
        /// Get the local identifier.
        /// </summary>
        /// <value>The local identifier of the OpenSearchable entity.</value>
        string Identifier { get; }

        /// <summary>
        /// Get the entity's OpenSearchDescription.
        /// </summary>
        /// <returns>The OpenSearchDescription describing the IOpenSearchable.</returns>
        OpenSearchDescription GetOpenSearchDescription();

        /// <summary>
        /// Gets the OpenSearch parameters for a given Mime-Type.
        /// </summary>
        /// <returns>OpenSearch parameters NameValueCollection.</returns>
        /// <param name="mimeType">MIME type for the requested parameters</param>
        NameValueCollection GetOpenSearchParameters(string mimeType);

        /// <summary>
        /// Get the total of possible results for the OpenSearchable entity
        /// </summary>
        /// <returns>a unsigned long number representing the number of items searchable</returns>
        ulong TotalResults();

        /// <summary>
        /// Optional function that apply to the result after the search and before the result is returned by OpenSearchEngine.
        /// </summary>
        /// <param name="osr">IOpenSearchResult cotnaing the result of the a search</param>
        void ApplyResultFilters(ref IOpenSearchResult osr);

        /// <summary>
        /// Gets the default MIME-type that the entity can be searched for
        /// </summary>
        /// <value>The default MIME-type.</value>
        string DefaultMimeType { get; }

    }

    /// <summary>
    /// OpenSearchable comparer.
    /// </summary>
    public class OpenSearchableComparer : IEqualityComparer<IOpenSearchable> {
        #region IEqualityComparer implementation

        public bool Equals(IOpenSearchable x, IOpenSearchable y) {
            if (x.Identifier == y.Identifier && x.GetType() == y.GetType()) return true;
            else return false;
        }

        public int GetHashCode(IOpenSearchable obj) {
            return obj.GetType().Name.GetHashCode() + obj.Identifier.GetHashCode();
        }

        #endregion
    }
}

