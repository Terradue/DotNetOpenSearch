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

namespace Terradue.OpenSearch.Result {

    /// <summary>
    /// Interface that represent a result item
    /// </summary>
    public interface IOpenSearchResultItem {

        string Id { get; set; }

        TextSyndicationContent Title { get; set; }

        DateTime LastUpdatedTime { get; set; }

        DateTime PublishDate { get; set; }

        TextSyndicationContent Summary { get; set; }

        string Identifier { get; set; }

        Collection<SyndicationLink> Links { get; set; }

        Collection<SyndicationCategory> Categories { get; }

        Collection<SyndicationPerson> Contributors { get; }

        Collection<SyndicationPerson> Authors { get; }

        TextSyndicationContent Copyright { get; set; }

        SyndicationElementExtensionCollection ElementExtensions { get; set; }

        SyndicationContent Content { get; set; }

        string SortKey { get; }

        bool ShowNamespaces { get; set; }

    }
}
