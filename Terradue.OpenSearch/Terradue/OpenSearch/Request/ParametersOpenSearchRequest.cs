//
//  ParametersOpenSearchRequest.cs
//
//  Author:
//       Emmanuel Mathot <emmanuel.mathot@terradue.com>
//
//  Copyright (c) 2014 Terradue

using System;
using System.Collections.Specialized;
using System.Threading;
using System.Web;
using Terradue.OpenSearch.Response;
using Terradue.OpenSearch.Schema;
using Terradue.OpenSearch.Result;

namespace Terradue.OpenSearch.Request
{
    public class ParametersOpenSearchRequest : OpenSearchRequest
	{
        IOpenSearchable entity;

        public ParametersOpenSearchRequest(IOpenSearchable entity) : base(new OpenSearchUrl("http://localhost/params")){
            this.entity = entity;

        }

        #region implemented abstract members of OpenSearchRequest
        public override OpenSearchResponse GetResponse() {
           
            ParametersResult parameters = entity.DescribeParameters();

            return new ParametersOpenSearchResponse(parameters);

        }
        #endregion
	}

}

