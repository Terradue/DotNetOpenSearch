//
//  IOpenSearchResponse.cs
//
//  Author:
//       Emmanuel Mathot <emmanuel.mathot@terradue.com>
//
//  Copyright (c) 2014 Terradue
//

using System;
using System.Collections.Generic;
using System.IO;
using Terradue.OpenSearch.Benchmarking;

namespace Terradue.OpenSearch.Response
{

    public interface IOpenSearchResponse
	{

        string ContentType { get; }

        Type ObjectType { get; }

        object GetResponseObject();

        IOpenSearchable Entity { get; set; }

        IEnumerable<Metric> Metrics { get; set; }

        IOpenSearchResponse CloneForCache();

        TimeSpan Validity { get; set; }

        DateTime Created { get; }

	}

}

