//
//  OpenSearchResponse.cs
//
//  Author:
//       Emmanuel Mathot <emmanuel.mathot@terradue.com>
//
//  Copyright (c) 2014 Terradue
//

using System;
using System.IO;

namespace Terradue.OpenSearch.Response
{

    /// <summary>
    /// Provides a base class for OpenSearch response
    /// </summary>
	public abstract class OpenSearchResponse
	{
        /// <summary>
        /// Get the MIME type of the response.
        /// </summary>
        /// <value>The type of the content.</value>
		public abstract string ContentType {
			get;
		}

        /// <summary>
        /// Gets the stream that is used to read the body of the response.
        /// </summary>
        /// <returns>A Stream containing the body of the response.</returns>
		public abstract Stream GetResponseStream();

        /// <summary>
        /// Gets the time interval spent for getting the response
        /// </summary>
        /// <value>A TimeSpan with the time interval</value>
		public abstract TimeSpan RequestTime {
			get;
		}

        /// <summary>
        /// Gets or sets the IOpenSearchable entity from which the response comes from.
        /// </summary>
        /// <value>The IOpenSearchable entity.</value>
		public IOpenSearchable Entity {
			get;
			set;
		}
	}
}

