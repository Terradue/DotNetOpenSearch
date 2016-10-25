//
//  IOpenSearchResponse.cs
//
//  Author:
//       Emmanuel Mathot <emmanuel.mathot@terradue.com>
//
//  Copyright (c) 2014 Terradue
//

using System;
using System.IO;

namespace Terradue.OpenSearch.Response
{

    public interface IOpenSearchResponse
	{

        string ContentType { get; }

        Type ObjectType { get; }

        object GetResponseObject();

        IOpenSearchable Entity { get; set; }

        TimeSpan RequestTime { get; }

        IOpenSearchResponse CloneForCache();

        TimeSpan Validity { get; set; }

        DateTime Created { get; }

	}

}

