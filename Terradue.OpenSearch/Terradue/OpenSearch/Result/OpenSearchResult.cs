//
//  OpenSearchResult.cs
//
//  Author:
//       Emmanuel Mathot <emmanuel.mathot@terradue.com>
//
//  Copyright (c) 2014 Terradue

using System;
using System.Collections.Specialized;
using Terradue.ServiceModel.Syndication;

namespace Terradue.OpenSearch.Result {

    /// <summary>
    /// Default class for reprenseting OpenSearchResult
    /// </summary>
    public partial class OpenSearchResult : IOpenSearchResult
    {

        // the Results
        private IOpenSearchResultCollection result;

        // The source Searchable entity
        private IOpenSearchable ose;

        // The source request
        private NameValueCollection searchParameters;

        public OpenSearchResult(IOpenSearchResultCollection result, NameValueCollection searchParameters) {
            Result = result;
            this.searchParameters = searchParameters;
        }

        #region IOpenSearchResult implementation

        public virtual IOpenSearchResultCollection Result {
            get {
                return result;
            }
            protected set {
                result = value;

            }
        }

        public IOpenSearchable OpenSearchableEntity {
            get {
                return ose;
            }
            set {
                ose = value;
            }
        }

        public string MimeType { 
            get {
                return result.ContentType;
            }
        }

        public NameValueCollection SearchParameters {
            get {
                return searchParameters;
            }
        }

       
        #endregion
    }
}

