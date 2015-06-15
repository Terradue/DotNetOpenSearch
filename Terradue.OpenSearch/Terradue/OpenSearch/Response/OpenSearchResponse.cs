//
//  OpenSearchResponse.cs
//
//  Author:
//       Emmanuel Mathot <emmanuel.mathot@terradue.com>
//
//  Copyright (c) 2014 Terradue
//

using System;
using System.IO;
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

        public OpenSearchResponse(){
        }

        public OpenSearchResponse(T obj){
            payload = obj;
        }

        public OpenSearchResponse(T obj, string contentType, TimeSpan requestTime){
            payload = obj;
            this.contentType = contentType;
            this.requestTime = requestTime;
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

        readonly TimeSpan requestTime;
        /// <summary>
        /// Gets the time interval spent for getting the response
        /// </summary>
        /// <value>A TimeSpan with the time interval</value>
        public virtual TimeSpan RequestTime {
            get {
                return requestTime;
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
            return null;
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
	}
}

