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
using System.Runtime.Serialization.Json;
using System.IO;

namespace Terradue.OpenSearch.Result
{

    [DataContract]
    public class ParametersResult : IOpenSearchResultCollection
	{


        List<ParameterDescription> parameters;

        public ParametersResult(){
            parameters = new List<ParameterDescription>();
        }

        [DataMember]
        public List<ParameterDescription> ParameterItems {
            get {
                return parameters;
            }
        }

        #region IOpenSearchResultCollection implementation
        public void SerializeToStream(System.IO.Stream stream) {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(ParametersResult));
            serializer.WriteObject(stream, this);
        }
        public string SerializeToString() {
            MemoryStream ms = new MemoryStream();
            SerializeToStream(ms);
            ms.Seek(0, SeekOrigin.Begin);
            StreamReader sr = new StreamReader(ms);
            return sr.ReadToEnd();
        }

        [IgnoreDataMember]
        public string Id {
            get {
                throw new NotImplementedException();
            }
            set {
                throw new NotImplementedException();
            }
        }
        [IgnoreDataMember]
        public IEnumerable<IOpenSearchResultItem> Items {
            get {
                return parameters.Cast<IOpenSearchResultItem>().ToList();
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
        public Collection<SyndicationPerson> Authors {
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
        public TextSyndicationContent Copyright {
            get {
                throw new NotImplementedException();
            }
            set {
                throw new NotImplementedException();
            }
        }
        [IgnoreDataMember]
        public SyndicationElementExtensionCollection ElementExtensions {
            get {
                throw new NotImplementedException();
            }
            set {
                throw new NotImplementedException();
            }
        }
        [IgnoreDataMember]
        public TextSyndicationContent Title {
            get {
                throw new NotImplementedException();
            }
            set {
                throw new NotImplementedException();
            }
        }
        [IgnoreDataMember]
        public TextSyndicationContent Description {
            get {
                throw new NotImplementedException();
            }
            set {
                throw new NotImplementedException();
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
        [IgnoreDataMember]
        public string Generator {
            get {
                throw new NotImplementedException();
            }
            set {
                throw new NotImplementedException();
            }
        }
        [IgnoreDataMember]
        public string Identifier {
            get {
                throw new NotImplementedException();
            }
            set {
                throw new NotImplementedException();
            }
        }
        [IgnoreDataMember]
        public long Count {
            get {
                throw new NotImplementedException();
            }
        }
        [IgnoreDataMember]
        public long TotalResults {
            get {
                throw new NotImplementedException();
            }
            set {
                throw new NotImplementedException();
            }
        }
        [IgnoreDataMember]
        public string ContentType {
            get {
                return "application/x-parameters+json";
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

        public IOpenSearchable OpenSearchable {
            get {
                throw new NotImplementedException();
            }
            set {
                throw new NotImplementedException();
            }
        }

        public NameValueCollection Parameters {
            get {
                throw new NotImplementedException();
            }
            set {
                throw new NotImplementedException();
            }
        }

        public TimeSpan Duration {
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

