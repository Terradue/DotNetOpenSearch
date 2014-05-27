//
//  IOpenSearchResult.cs
//
//  Author:
//       Emmanuel Mathot <emmanuel.mathot@terradue.com>
//
//  Copyright (c) 2014 Terradue

using System;
using System.Collections.Specialized;
using Terradue.ServiceModel.Syndication;

namespace Terradue.OpenSearch.Result
{
    /// <summary>
    /// Interface for OpenSearch Results
    /// </summary>
	public interface IOpenSearchResult {

        /// <summary>
        /// Gets the result.
        /// </summary>
        /// <value>The result.</value>
        IOpenSearchResultCollection Result { get; }

        /// <summary>
        /// Gets or sets the IOpenSearchable entity of the result.
        /// </summary>
        /// <value>The open searchable entity.</value>
        IOpenSearchable OpenSearchableEntity { get; set; }

        /// <summary>
        /// Declarative mimeType of the results
        /// </summary>
        /// <value>The type of the MIME.</value>
        string MimeType { get; }

        /// <summary>
        /// Gets the search parameters of the result.
        /// </summary>
        /// <value>The search parameters.</value>
        NameValueCollection SearchParameters { get; }


	}
}

