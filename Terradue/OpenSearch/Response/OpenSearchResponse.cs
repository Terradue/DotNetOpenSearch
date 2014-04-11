//
//  OpenSearchResponse.cs
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
using System.IO;

namespace Terradue.OpenSearch
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

