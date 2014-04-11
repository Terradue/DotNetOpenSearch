//
//  IOpenSearchable.cs
//
//  Author:
//       Terradue <info@terradue.com>
//
//  Copyright (c) 2014 Terradue
//
//  This program is free software; you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation; either version 2 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program; if not, write to the Free Software
//  Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
//

using System;
using System.Collections.Specialized;
using System.Collections.Generic;

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
        Tuple<string, Func<OpenSearchResponse, object>> GetTransformFunction(OpenSearchEngine ose, Type resultType);

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
        /// Gets the search base URL.
        /// </summary>
        /// <returns>The search base URL.</returns>
        /// <param name="mimeType">MIME type.</param>
        OpenSearchUrl GetSearchBaseUrl(string mimeType);

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

