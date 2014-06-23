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

namespace Terradue.OpenSearch.Result
{

    /// <summary>
    /// Interface to represent a collection of results item
    /// </summary>
    public interface IOpenSearchResultCollection
	{

        IEnumerable<IOpenSearchResultItem> Items { get; }

        Collection<SyndicationLink> Links { get; }

        SyndicationElementExtensionCollection ElementExtensions { get; }

        string Title { get; }

        DateTime Date { get; }

        string Identifier { get; }

        long Count { get; }

        void SerializeToStream (Stream stream);

        string SerializeToString ();

        string ContentType { get; }

        bool ShowNamespaces { get; set; }

	}


    /// <summary>
    /// Interface that represent a result item
    /// </summary>
    public interface IOpenSearchResultItem
    {
        string Id { get; }

        string Title { get; }

        DateTime Date { get; }

        string Identifier { get; }

        Collection<SyndicationLink> Links { get; }

        SyndicationElementExtensionCollection ElementExtensions { get; }

        bool ShowNamespaces { get; set; }

    }
}

