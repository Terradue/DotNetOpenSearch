//
//  OpenSearchResponse.cs
//
//  Author:
//       Emmanuel Mathot <emmanuel.mathot@terradue.com>
//
//  Copyright (c) 2014 Terradue
//

using System;
using System.Collections.Generic;
using System.IO;
using Terradue.OpenSearch.Benchmarking;
using Terradue.OpenSearch.Engine;
using Terradue.OpenSearch.Result;

namespace Terradue.OpenSearch.Response
{

    /// <summary>
    /// Provides a base class for OpenSearch response
    /// </summary>
    public class OpenSearchResponse<T> : IOpenSearchResponse
	{

        protected T payload;
        DateTime created;

        public OpenSearchResponse(){
            created = DateTime.UtcNow;
        }

        public OpenSearchResponse(T obj){
            payload = obj;
            created = DateTime.UtcNow;
        }

        public OpenSearchResponse(T obj, string contentType, IEnumerable<Metric> metrics){
            payload = obj;
            this.contentType = contentType;
            this.metrics = metrics;
            created = DateTime.UtcNow;
        }

        readonly string contentType;
        /// <summary>
        /// Get the MIME type of the response.
        /// </summary>
        /// <value>The type of the content.</value>
        public virtual string ContentType {
            get {
                return contentType;
            }
        }

        /// <summary>
        /// Get the MIME type of the response.
        /// </summary>
        /// <value>The type of the content.</value>
        public virtual Type ObjectType {
            get {
                return typeof(T);
            }
        }

        /// <summary>
        /// Gets the object that represents the response.
        /// </summary>
        /// <returns>A object of type containing the response.</returns>
        public virtual object GetResponseObject() {
            return payload;
        }

        protected IEnumerable<Metric> metrics = new List<Metric>();
        /// <summary>
        /// Gets the time interval spent for getting the response
        /// </summary>
        /// <value>A TimeSpan with the time interval</value>
        public virtual IEnumerable<Metric> Metrics
        {
            get {
                return metrics;
            }
            set
            {
                metrics = value;
            }
        }

        /// <summary>
        /// Gets or sets the IOpenSearchable entity from which the response comes from.
        /// </summary>
        /// <value>The IOpenSearchable entity.</value>
		public IOpenSearchable Entity {
			get;
			set;
		}

        public virtual IOpenSearchResponse CloneForCache(){
            return new OpenSearchResponse<T>(payload, contentType, metrics);
        }

        TimeSpan validity = TimeSpan.FromSeconds(OpenSearchEngine.DEFAULT_VALIDITY);
        public virtual TimeSpan Validity {
            get {
                return validity;
            }
            set {
                validity = value;
            }
        }

        public DateTime Created
        {
            get
            {
                return created;
            }
        }
    }
}

