//
//  ParametersOpenSearchEngineExtension.cs
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

namespace Terradue.OpenSearch.Engine.Extensions {

    /// <summary>
    /// Atom open search engine extension.
    /// </summary>
    [Extension(typeof(IOpenSearchEngineExtension))]
    [ExtensionNode("Parameters", "Parameters descriptor")]
    public class ParametersOpenSearchEngineExtension : OpenSearchEngineExtension<ParametersResult> {

        public ParametersOpenSearchEngineExtension() {
        }

        #region OpenSearchEngineExtension implementation

        public override string Identifier {
            get {
                return "params";
            }
        }

        public override string Name {
            get {
                return "Parameters descriptor";
            }
        }

        public override IOpenSearchResultCollection ReadNative(IOpenSearchResponse response) {
            if (response.ContentType == "application/x-parameters+json")
                return TransformJsonResponseToSuggestions((ParametersOpenSearchResponse)response);

            throw new NotSupportedException("Parameters extension does not transform OpenSearch response of contentType " + response.ContentType);
        }

        public override string DiscoveryContentType {
            get {
                return "application/x-parameters+json";
            }
        }

        public override OpenSearchUrl FindOpenSearchDescriptionUrlFromResponse(IOpenSearchResponse response) {

            if (response.ContentType == "application/x-parameters+json") {
                // TODO
                throw new NotImplementedException();
            }

            return null;
        }

        public override IOpenSearchResultCollection CreateOpenSearchResultFromOpenSearchResult(IOpenSearchResultCollection results) {
            if (results is ParametersResult)
                return results;

            throw new NotSupportedException("Paraneters extension is not a result transformator.");
        }

        #endregion

        public static ParametersResult TransformJsonResponseToSuggestions(ParametersOpenSearchResponse response) {

            return response.GetSuggestions();

        }

        //---------------------------------------------------------------------------------------------------------------------

    }
}

