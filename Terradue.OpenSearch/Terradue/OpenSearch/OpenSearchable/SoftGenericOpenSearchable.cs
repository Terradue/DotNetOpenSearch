//
//  GenericOpenSearchable.cs
//
//  Author:
//       Emmanuel Mathot <emmanuel.mathot@terradue.com>
//
//  Copyright (c) 2014 Terradue

using System;
using System.Web;
using System.Net;
using Terradue.OpenSearch.Engine.Extensions;
using System.Collections.Specialized;
using Terradue.OpenSearch.Request;
using Terradue.OpenSearch.Engine;
using Terradue.OpenSearch.Response;
using Terradue.OpenSearch.Result;
using Terradue.OpenSearch.Schema;
using System.Linq;

namespace Terradue.OpenSearch {
    /// <summary>
    /// Generic class to represent any OpenSearchable entity
    /// </summary>
    public class SoftGenericOpenSearchable : GenericOpenSearchable {

        /// <summary>
        /// Initializes a new instance of the <see cref="Terradue.OpenSearch.GenericOpenSearchable"/> class from a quaery Url
        /// </summary>
        /// <param name="url">The query URL</param>
        /// <param name="ose">An OpenSearchEngine instance, preferably with registered extensions able to read the query url</param>
        public SoftGenericOpenSearchable(OpenSearchUrl url, OpenSearchEngine ose) : base(url, ose) {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Terradue.OpenSearch.GenericOpenSearchable"/> class from an OpenSearchDescription.
        /// </summary>
        /// <param name="osd">The OpenSearchDescription describing the OpenSearchable entity to represent</param>
        /// <param name="ose">An OpenSearchEngine instance, preferably with registered extensions able to read the query url</param>
        public SoftGenericOpenSearchable(OpenSearchDescription osd, OpenSearchEngine ose) : base(osd, ose) {
        }

        #region IOpenSearchable implementation

        public override QuerySettings GetQuerySettings(OpenSearchEngine ose) {
            var querySettings = base.GetQuerySettings(ose);
            querySettings.ForceUnspecifiedParameters = true;
            return querySettings;
        }

        #endregion

    }
}

