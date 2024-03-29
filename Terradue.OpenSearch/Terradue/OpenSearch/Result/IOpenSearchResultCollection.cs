//
//  IOpenSearchResultCollection.cs
//
//  Author:
//       Emmanuel Mathot <emmanuel.mathot@terradue.com>
//
//  Copyright (c) 2014 Terradue

using System;
using System.Linq;
using System.Xml;
using System.Collections.Generic;
using System.Collections.Specialized;
using Terradue.ServiceModel.Syndication;
using System.IO;
using System.Collections.ObjectModel;
using Terradue.OpenSearch.Request;

namespace Terradue.OpenSearch.Result {

    /// <summary>
    /// Interface to represent a collection of results item
    /// </summary>
    public interface IOpenSearchResultCollection : ICloneable{

        string Id { get; set; }

        IEnumerable<IOpenSearchResultItem> Items { get; set; }

        Collection<SyndicationLink> Links { get; set; }

        Collection<SyndicationCategory> Categories { get; }

        Collection<SyndicationPerson> Authors { get; }

        Collection<SyndicationPerson> Contributors { get; }

        TextSyndicationContent Copyright { get; set; }

        SyndicationElementExtensionCollection ElementExtensions { get; set; }

        TextSyndicationContent Title { get; set; }

        TextSyndicationContent Description { get; set; }

        DateTimeOffset LastUpdatedTime { get; set; }

        string Generator { get; set; }

        string Identifier { get; set; }

        long Count { get; }

        long TotalResults { get; set; }

        void SerializeToStream(Stream stream);

        string SerializeToString();

        string ContentType { get; }

        IOpenSearchable OpenSearchable { get; set; }

        NameValueCollection Parameters { get; set; }

        TimeSpan QueryTimeSpan { get; set; }
        
    }


}

