//
//  OpenSearchException.cs
//
//  Author:
//       Emmanuel Mathot <emmanuel.mathot@terradue.com>
//
//  Copyright (c) 2014 Terradue

using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Web;
using System.Xml.Serialization;
using System.Xml;
using System.Text;
using System.IO;
using System.Security;
using Terradue.OpenSearch.Result;
using Terradue.ServiceModel.Syndication;
using Terradue.OpenSearch.Engine;
using Terradue.OpenSearch.Response;
using Terradue.OpenSearch.Schema;
using Terradue.OpenSearch.Request;

namespace Terradue.OpenSearch
{
	public class OpenSearchException : Exception
	{

        public OpenSearchException(string message) : base(message){}

	}
    
}
