//
//  ExceptionMessage.cs
//
//  Author:
//       Emmanuel Mathot <emmanuel.mathot@terradue.com>
//
//  Copyright (c) 2014 Terradue

using System;
using System.Linq;
using System.Collections.Specialized;
using Terradue.ServiceModel.Syndication;
using System.Collections.Generic;
using System.Threading;
using System.Web;
using Terradue.OpenSearch.Engine;
using Terradue.OpenSearch.Response;
using System.Diagnostics;
using Terradue.OpenSearch.Result;

namespace Terradue.OpenSearch
{
	public class ExceptionMessage
	{

        public string Message {get; set;}

        public string Source {get; set;}

        public string HelpLink {get; set;}

	}

}

