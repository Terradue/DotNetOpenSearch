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
using Terradue.OpenSearch.Engine;

namespace Terradue.OpenSearch
{
    /// <summary>
    /// Represents an OpenSearch Url
    /// </summary>
	public class OpenSearchUrl : Uri
	{

        private int osdPageOffset = 1;

        private int osdIndexOffset = 1;

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
			this.osdIndexOffset = osdu.IndexOffset;
			this.osdPageOffset = osdu.PageOffset;
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

				NameValueCollection nvc = HttpUtility.ParseQueryString (this.Query);

				return nvc;

			}
		}

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents the current <see cref="Terradue.OpenSearch.OpenSearchUrl"/>.
        /// </summary>
        /// <returns>A <see cref="System.String"/> that represents the current <see cref="Terradue.OpenSearch.OpenSearchUrl"/>.</returns>
        public new string ToString() {
			return base.AbsoluteUri;
		}

        /// <summary>
        /// Gets the page offset.
        /// </summary>
        /// <value>The page offset.</value>
		public int PageOffset {
			get {
                var po = SearchAttributes["startPage"];
                return po == null ? osdPageOffset : int.Parse(po);
			}
		}

        /// <summary>
        /// Gets the index offset.
        /// </summary>
        /// <value>The index offset.</value>
		public int IndexOffset {
			get {
                var si = SearchAttributes["startIndex"];
                return si == null ? osdIndexOffset : int.Parse(si);
			}
		}

        /// <summary>
        /// Gets the index offset.
        /// </summary>
        /// <value>The index offset.</value>
        public int Count {
            get {
                var count = SearchAttributes["count"];
                return count == null ? OpenSearchEngine.DEFAULT_COUNT : int.Parse(count);
            }
        }
	}

}
