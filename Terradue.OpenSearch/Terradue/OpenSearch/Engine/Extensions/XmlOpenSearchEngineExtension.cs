//
//  AtomOpenSearchEngineExtension.cs
//
//  Author:
//       Emmanuel Mathot <emmanuel.mathot@terradue.com>
//
//  Copyright (c) 2014 Terradue

using System;
using System.Linq;
using Mono.Addins;
using Terradue.ServiceModel.Syndication;
using System.Collections.Generic;
using System.Xml;
using System.Net;
using System.Diagnostics;
using System.Collections.Specialized;
using Terradue.OpenSearch.Response;
using Terradue.OpenSearch.Result;
using Terradue.OpenSearch.Schema;

namespace Terradue.OpenSearch.Engine.Extensions {
    /// <summary>
    /// Atom open search engine extension.
    /// </summary>
    [Extension(typeof(IOpenSearchEngineExtension))]
    [ExtensionNode("Xml", "Atom (Xml) native query")]
    public class XmlOpenSearchEngineExtension : AtomOpenSearchEngineExtension {
        public XmlOpenSearchEngineExtension() {
        }

        #region implemented abstract members of OpenSearchEngineExtension

        public override string DiscoveryContentType {
            get {
                return "application/xml";
            }
        }

        #endregion

    }
}

