//
//  MemoryOpenSearchResponse.cs
//
//  Author:
//       Emmanuel Mathot <emmanuel.mathot@terradue.com>
//
//  Copyright (c) 2014 Terradue

using System;
using System.Collections.Specialized;
using System.IO;
using System.Diagnostics;
using System.Text;
using System.Collections.Generic;
using Terradue.OpenSearch.Benchmarking;

namespace Terradue.OpenSearch.Response
{
    /// <summary>
    /// A memory buffer for storing an OpenSearch response
    /// </summary>
    public class MemoryOpenSearchResponse : OpenSearchResponse<byte[]>
    {

        protected string contentType;

        /// <summary>
        /// Initializes a new instance of the <see cref="Terradue.OpenSearch.MemoryOpenSearchResponse"/> class.
        /// </summary>
        /// <param name="input">Input Stream to be copied in memory</param>
        /// <param name="contentType">Content type of the stream to be put in memory</param>
        public MemoryOpenSearchResponse(byte[] input, string contentType)
        {
            payload = input;
            this.contentType = contentType;
        }

        public MemoryOpenSearchResponse(byte[] input, string contentType, IEnumerable<Metric> metrics)
        {
            payload = input;
            this.contentType = contentType;
            this.metrics = metrics;
        }

        protected MemoryOpenSearchResponse(string contentType)
        {
            this.contentType = contentType;
        }

        public MemoryOpenSearchResponse(string input, string contentType)
        {
            payload = System.Text.Encoding.Default.GetBytes(input);
            this.contentType = contentType;
        }

        #region implemented abstract members of OpenSearchResponse

        /// <summary>
        /// Gets the stream that is used to read the body of the response.
        /// </summary>
        /// <returns>A Stream containing the body of the response.</returns>
        public override object GetResponseObject()
        {
            return payload;
        }

        /// <summary>
        /// Get the MIME type of the response.
        /// </summary>
        /// <value>The type of the content.</value>
		public override string ContentType
        {
            get
            {
                return contentType;
            }
        }

        public override IOpenSearchResponse CloneForCache()
        {
            MemoryOpenSearchResponse mosr = new MemoryOpenSearchResponse((byte[])payload.Clone(), contentType);
            mosr.Validity = this.Validity;
            mosr.Entity = this.Entity;
            return mosr;
        }

        #endregion
    }
}


