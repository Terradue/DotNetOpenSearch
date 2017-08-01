//
//  IProxiedOpenSearchable.cs
//
//  Author:
//       Emmanuel Mathot <emmanuel.mathot@terradue.com>
//
//  Copyright (c) 2014 Terradue
//

using System;
using System.Collections.Specialized;
using System.Collections.Generic;
using Terradue.OpenSearch.Schema;

namespace Terradue.OpenSearch {

	/// <summary>
	/// IOpenSearchable.
	/// </summary>
    /// <description>Internal interface that enables OpenSearch mechanism on a class that proxy another OpenSearchable entity.
    /// For instance, a remote OpenSearch interface could be proxied by a class implementeing IOpenSearchable
    /// with a local Url.
    /// </description>
    public partial interface IProxiedOpenSearchable : IOpenSearchable {

        /// <summary>
        /// Gets the OpenSearchDescription that represents the entity proxied locally.
        /// </summary>
        /// <returns>OpenSearchDescription with local references</returns>
        OpenSearchDescription GetProxyOpenSearchDescription();

	}
        
}

