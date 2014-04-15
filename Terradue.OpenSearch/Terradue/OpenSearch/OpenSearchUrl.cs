//
//  OpenSearchUrl.cs
//
//  Author:
//       Emmanuel Mathot <emmanuel.mathot@terradue.com>
//
//  Copyright (c) 2014 Terradue

using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Web;
using System.Xml.Serialization;
using System.Xml;
using System.Text;
using Terradue.OpenSearch.Schema;

namespace Terradue.OpenSearch
{
    /// <summary>
    /// Represents an OpenSearch Url
    /// </summary>
	public class OpenSearchUrl : Uri
	{

		private int pageOffset = 1;

		private int indexOffset = 1;

        /// <summary>
        /// Initializes a new instance of the <see cref="Terradue.OpenSearch.OpenSearchUrl"/> class.
        /// </summary>
        /// <param name="uriString">URI string.</param>
		public OpenSearchUrl (string uriString) : base (uriString)
		{
		}

        /// <summary>
        /// Initializes a new instance of the <see cref="Terradue.OpenSearch.OpenSearchUrl"/> class.
        /// </summary>
        /// <param name="osdu">Osdu.</param>
		public OpenSearchUrl (OpenSearchDescriptionUrl osdu) : base (osdu.Template)
		{
			this.indexOffset = osdu.IndexOffset;
			this.pageOffset = osdu.PageOffset;
		}

        /// <summary>
        /// Initializes a new instance of the <see cref="Terradue.OpenSearch.OpenSearchUrl"/> class.
        /// </summary>
        /// <param name="url">URL.</param>
        public OpenSearchUrl (Uri url) : base (url.ToString())
        {
        }

        /// <summary>
        /// Gets the search attributes.
        /// </summary>
        /// <value>The search attributes.</value>
		public NameValueCollection SearchAttributes {
			get {

				UriBuilder ub = new UriBuilder (this.AbsoluteUri);
				NameValueCollection nvc = HttpUtility.ParseQueryString (ub.Query);

				return nvc;

			}
		}

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents the current <see cref="Terradue.OpenSearch.OpenSearchUrl"/>.
        /// </summary>
        /// <returns>A <see cref="System.String"/> that represents the current <see cref="Terradue.OpenSearch.OpenSearchUrl"/>.</returns>
		public string  ToString() {
			return base.AbsoluteUri;
		}

        /// <summary>
        /// Gets the page offset.
        /// </summary>
        /// <value>The page offset.</value>
		public int PageOffset {
			get {
				return pageOffset;
			}
		}

        /// <summary>
        /// Gets the index offset.
        /// </summary>
        /// <value>The index offset.</value>
		public int IndexOffset {
			get {
				return indexOffset;
			}
		}
	}

}
