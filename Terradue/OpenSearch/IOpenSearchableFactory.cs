//
//  IOpenSearchableFactory.cs
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

namespace Terradue.OpenSearch {

    /// <summary>
    /// Represents a factory able to create IOpenSearchable object
    /// </summary>
    public interface IOpenSearchableFactory {

        /// <summary>
        /// Create an IOpenSearchable from an OpenSearchUrl
        /// </summary>
        /// <param name="url">URL to either a search or a description</param>
        IOpenSearchable Create(OpenSearchUrl url);

        /// <summary>
        /// Create an IOpenSearchable from an OpenSearchDescription
        /// </summary>
        /// <param name="osd">OPenSearchDescription</param>
        IOpenSearchable Create(OpenSearchDescription osd);

    }
}

