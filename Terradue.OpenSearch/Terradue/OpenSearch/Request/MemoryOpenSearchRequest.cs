//
//  FileOpenSearchRequest.cs
//
//  Author:
//       Emmanuel Mathot <emmanuel.mathot@terradue.com>
//
//  Copyright (c) 2014 Terradue

using System;
using System.Collections.Specialized;
using System.IO;
using System.Web;
using Terradue.OpenSearch.Response;

namespace Terradue.OpenSearch.Request
{
    /// <summary>
    /// OpenSearchRequest in memory that doesnot actually performs a request but is a container
    /// for the response
    /// </summary>
    public class MemoryOpenSearchRequest : OpenSearchRequest
	{

		NameValueCollection parameters;
		MemoryStream memStream;

		string contentType;

        /// <summary>
        /// Initializes a new instance of the <see cref="Terradue.OpenSearch.Request.MemoryOpenSearchRequest"/> class.
        /// </summary>
        /// <param name="url">URL.</param>
        /// <param name="contentType">Content type.</param>
        public MemoryOpenSearchRequest(OpenSearchUrl url, string contentType) : base(url){
			this.contentType = contentType;
            this.parameters = HttpUtility.ParseQueryString(url.Query);
			memStream = new MemoryStream();
		}

        /// <summary>
        /// Gets the MemoryInputStream to write the result into.
        /// </summary>
        /// <value>The memory input stream.</value>
		public Stream MemoryInputStream {
			get {
				return memStream;
			}
		}

        public TimeSpan ResponseValidity {
            get ;
            set ;
        }

		#region implemented abstract members of OpenSearchRequest

		public override IOpenSearchResponse GetResponse() {
			memStream.Seek(0, SeekOrigin.Begin);
            MemoryOpenSearchResponse mosr = new MemoryOpenSearchResponse(memStream.ToArray(), contentType);
            mosr.Validity = this.ResponseValidity.Ticks == 0 ? mosr.Validity : this.ResponseValidity;
            return mosr;

		}

        public override OpenSearchUrl OpenSearchUrl {
            get {
                return base.OpenSearchUrl;
            }
        }

		#endregion

	}
}


