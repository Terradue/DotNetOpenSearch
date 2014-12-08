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
using System.Runtime.Serialization;

namespace Terradue.OpenSearch.Result
{

    [DataContract]
    public class ParameterDescription : IOpenSearchResultItem
	{

        public ParameterDescription(){
            template = new XmlQualifiedName();
        }

        XmlQualifiedName template;
        [DataMember]
        public XmlQualifiedName Template {
            get {
                return template;
            }
            set {
                template = value;
            }
        }

        List<string> options;
        [DataMember]
        public List<string> Options {
            get {
                return options;
            }
            set {
                options = value;
            }
        }

        string type;
        [DataMember]
        public string Type {
            get {
                return type;
            }
            set {
                type = value;
            }
        }

        [DataMember]
        public string Abstract {
            get {
                return summary.Text;
            }
            set {
                summary = new TextSyndicationContent(value);
            }
        }

        #region IOpenSearchResultItem implementation

        string id;
        [DataMember]
        public string Id {
            get {
                return id;
            }
            set {
                id = value;
            }
        }

        TextSyndicationContent title;
        [DataMember]
        public TextSyndicationContent Title {
            get {
                return title;
            }
            set {
                title = value;
            }
        }

        [IgnoreDataMember]
        public DateTime Date {
            get {
                throw new NotImplementedException();
            }
            set {
                throw new NotImplementedException();
            }
        }

        TextSyndicationContent summary;
        [IgnoreDataMember]
        public TextSyndicationContent Summary {
            get {
                return summary;
            }
            set {
                summary = value;
            }
        }

        [DataMember]
        public string Identifier {
            get {
                return template.Name;
            }
            set {

            }
        }

        [DataMember]
        public string Namespace {
            get {
                return template.Namespace;
            }
            set {

            }
        }

        [IgnoreDataMember]
        public Collection<SyndicationLink> Links {
            get {
                throw new NotImplementedException();
            }
            set {
                throw new NotImplementedException();
            }
        }

        [IgnoreDataMember]
        public Collection<SyndicationCategory> Categories {
            get {
                throw new NotImplementedException();
            }
        }

        [IgnoreDataMember]
        public Collection<SyndicationPerson> Contributors {
            get {
                throw new NotImplementedException();
            }
        }

        [IgnoreDataMember]
        public Collection<SyndicationPerson> Authors {
            get {
                throw new NotImplementedException();
            }
        }

        [IgnoreDataMember]
        public TextSyndicationContent Copyright {
            get {
                throw new NotImplementedException();
            }
            set {
                throw new NotImplementedException();
            }
        }

        SyndicationElementExtensionCollection elementExtensions;
        [IgnoreDataMember]
        public SyndicationElementExtensionCollection ElementExtensions {
            get {
                return elementExtensions;
            }
            set {
                elementExtensions = value;
            }
        }

        [IgnoreDataMember]
        public SyndicationContent Content {
            get {
                throw new NotImplementedException();
            }
            set {
                throw new NotImplementedException();
            }
        }

        [IgnoreDataMember]
        public bool ShowNamespaces {
            get {
                throw new NotImplementedException();
            }
            set {
                throw new NotImplementedException();
            }
        }
        #endregion
	}


}

