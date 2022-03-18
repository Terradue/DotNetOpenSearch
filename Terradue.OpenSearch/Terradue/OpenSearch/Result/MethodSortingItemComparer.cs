//
//  AtomFeed.cs
//
//  Author:
//       Emmanuel Mathot <emmanuel.mathot@terradue.com>
//
//  Copyright (c) 2014 Terradue

using System;
using System.Collections.Generic;

namespace Terradue.OpenSearch.Result
{
    public class MethodSortingItemComparer : IComparer<IOpenSearchResultItem>
    {
        private static OpenSearchResultItemComparer openSearchResultItemComparer = new OpenSearchResultItemComparer();

        public List<Func<IOpenSearchResultItem, IOpenSearchResultItem, int>> SortMethods { get; protected set; }

        public MethodSortingItemComparer()
        {
            SortMethods = new List<Func<IOpenSearchResultItem, IOpenSearchResultItem, int>>();
        }

        public int Compare(IOpenSearchResultItem x, IOpenSearchResultItem y)
        {
            foreach (var method in SortMethods)
            {
                int res = method(x, y);
                if ( res == 0 ) continue;
                return res;
            }
            return 0;
        }
    }
}