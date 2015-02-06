//
//  FeatureCollectionOpenSearchEngineExtension.cs
//
//  Author:
//       Emmanuel Mathot <emmanuel.mathot@terradue.com>
//
//  Copyright (c) 2014 Terradue

using System;
using Mono.Addins;
using Terradue.ServiceModel.Syndication;
using System.Collections.Generic;
using System.Xml;
using System.Net;
using System.Collections.Specialized;
using System.Linq;
using System.Collections.ObjectModel;
using Terradue.OpenSearch.Engine.Extensions;
using Terradue.OpenSearch.Response;
using Terradue.OpenSearch.Schema;
using Terradue.OpenSearch.Engine;
using Terradue.OpenSearch.Result;


namespace Terradue.OpenSearch.Response {

    public class ParametersOpenSearchResponse : OpenSearchResponse<ParametersResult> {

        ParametersResult parameters;

        internal ParametersOpenSearchResponse(ParametersResult parameters) {
            this.parameters = parameters;
        }
       
        public ParametersResult GetSuggestions() {

            return parameters;

        }

        #region implemented abstract members of OpenSearchResponse
        public override object GetResponseObject() {
            throw new NotImplementedException();
        }
        public override string ContentType {
            get {
                return "application/x-suggestions+json";
            }
        }
        public override TimeSpan RequestTime {
            get {
                return new TimeSpan(0);
            }
        }
        #endregion
    }
}
