//
//  IAtomizable.cs
//
//  Author:
//       Emmanuel Mathot <emmanuel.mathot@terradue.com>
//
//  Copyright (c) 2014 Terradue

using System;
using Terradue.OpenSearch.Result;
using System.Collections.Specialized;

namespace Terradue.OpenSearch {

    /// <summary>
    /// Interface to implement a class as an item in a generic or heterogeneous OpenSearchable entity.
    /// </summary>
    public interface IAtomizable {

        /// <summary>
        /// Transform the objec in its Atom item representation.
        /// </summary>
        /// <returns>The atom item. null if does not correspond to the parameters</returns>
        /// <param name="parameters">Parameters to filter the items the search</param>
        AtomItem ToAtomItem(NameValueCollection parameters);

        /// <summary>
        /// Get the OpenSearch parameters applicable to the item.
        /// </summary>
        /// It should be ued by the generic OpenSearchable entity that use the object implementing IAtomizable
        /// to build all the OpenSearch parameters of the search template. Due to the atom nature
        /// of the item, the content type is not a parameter and should be considered as "application/atom+xml"
        /// <returns>The open search parameters.</returns>
        NameValueCollection GetOpenSearchParameters();

    }
}

