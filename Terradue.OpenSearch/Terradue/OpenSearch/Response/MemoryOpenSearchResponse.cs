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

namespace Terradue.OpenSearch.Response
{
    /// <summary>
    /// A memory buffer for storing an OpenSearch response
    /// </summary>
    public class MemoryOpenSearchResponse : OpenSearchResponse<byte[]>
	{

        protected byte[] response;

        protected string contentType;

        protected TimeSpan timeSpan;

        /// <summary>
        /// Initializes a new instance of the <see cref="Terradue.OpenSearch.MemoryOpenSearchResponse"/> class.
        /// </summary>
        /// <param name="input">Input Stream to be copied in memory</param>
        /// <param name="contentType">Content type of the stream to be put in memory</param>
        public MemoryOpenSearchResponse(byte[] input, string contentType){
			Stopwatch sw = new Stopwatch();
			sw.Start();
            response = input;
			this.contentType = contentType;
			sw.Start();
			timeSpan = sw.Elapsed;
		}

        protected MemoryOpenSearchResponse(string contentType){
            Stopwatch sw = new Stopwatch();
            sw.Start();
            this.contentType = contentType;
            sw.Start();
            timeSpan = sw.Elapsed;
        }

        public MemoryOpenSearchResponse(string input, string contentType){
            Stopwatch sw = new Stopwatch();
            sw.Start();
            response = System.Text.Encoding.Default.GetBytes(input);
            this.contentType = contentType;
            sw.Start();
            timeSpan = sw.Elapsed;
        }

		#region implemented abstract members of OpenSearchResponse

        /// <summary>
        /// Gets the stream that is used to read the body of the response.
        /// </summary>
        /// <returns>A Stream containing the body of the response.</returns>
        public override object GetResponseObject() {
			return response;
		}

        /// <summary>
        /// Get the MIME type of the response.
        /// </summary>
        /// <value>The type of the content.</value>
		public override string ContentType {
			get {
				return contentType;
			}
		}

        /// <summary>
        /// Gets the time interval spent for getting the response
        /// </summary>
        /// <value>A TimeSpan with the time interval</value>
		public override TimeSpan RequestTime {
			get {
				return timeSpan;
			}
		}

		#endregion
	}
}


