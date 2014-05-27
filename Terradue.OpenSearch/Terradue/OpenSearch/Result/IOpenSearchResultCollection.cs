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
using System.ServiceModel.Syndication;
using System.IO;

namespace Terradue.OpenSearch.Result
{

    /// <summary>
    /// Interface to represent a collection of results item
    /// </summary>
    public interface IOpenSearchResultCollection
	{

        List<IOpenSearchResultItem> Items { get; }

        List<SyndicationLink> Links { get; }

        SyndicationElementExtensionCollection ElementExtensions { get; }

        string Title { get; }

        DateTime Date { get; }

        string Identifier { get; }

        long Count { get; }

        void Serialize (Stream stream);

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

        List<SyndicationLink> Links { get; }

        SyndicationElementExtensionCollection ElementExtensions { get; }

    }
}

