//
//  FileOpenSearchRequest.cs
//
//  Author:
//       Emmanuel Mathot <emmanuel.mathot@terradue.com>
//
//  Copyright (c) 2014 Terradue

using System;

namespace Terradue.OpenSearch {
    public class ImpossibleSearchException : Exception {
        public ImpossibleSearchException(string message) : base(message) {
        }
    }
}

