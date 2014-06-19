﻿//
//  MultiGenericOpenSearchable.cs
//
//  Author:
//       Emmanuel Mathot <emmanuel.mathot@terradue.com>
//
//  Copyright (c) 2014 Terradue

using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web;
using Terradue.OpenSearch.Engine.Extensions;
using Terradue.OpenSearch.Request;
using Terradue.OpenSearch.Engine;
using Terradue.OpenSearch.Response;
using Terradue.OpenSearch.Result;
using Terradue.OpenSearch.Schema;

namespace Terradue.OpenSearch {

    /// <summary>
    /// Represents a set of OpenSearchable as a unique OpenSearshable
    /// </summary>
    public class MultiGenericOpenSearchable : IOpenSearchable {

        List<IOpenSearchable> entities;

        OpenSearchEngine ose;

        /// <summary>
        /// Initializes a new instance of the <see cref="Terradue.OpenSearch.MultiGenericOpenSearchable"/> class.
        /// </summary>
        /// <param name="entities">Entities.</param>
        /// <param name="ose">Ose.</param>
        public MultiGenericOpenSearchable(List<IOpenSearchable> entities, OpenSearchEngine ose) {
            this.ose = ose;
            this.entities = new List<IOpenSearchable>(entities);
        }

        public string Identifier {
            get;
            set;
        }

        string Description {
            get;
            set;
        }

        /// <summary>
        /// Gets the internal open search URL.
        /// </summary>
        /// <returns>The internal open search URL.</returns>
        /// <param name="parameters">Parameters.</param>
        protected OpenSearchUrl GetInternalOpenSearchUrl(NameValueCollection parameters) {
            UriBuilder url = new UriBuilder(string.Format("http://{0}", System.Environment.MachineName));
            url.Path += "/multi/";
            var array = (from key in parameters.AllKeys
                         from value in parameters.GetValues(key)
                         select string.Format("{0}={1}", HttpUtility.UrlEncode(key), HttpUtility.UrlEncode(value)))
                .ToArray();
            url.Query = string.Join("&", array);
            return new OpenSearchUrl(url.ToString());
        }
          
        #region IOpenSearchable implementation

        public OpenSearchDescription GetOpenSearchDescription () {

            OpenSearchDescription osd = new OpenSearchDescription();
            osd.ShortName = this.Identifier;
            osd.Description = this.Description;
            osd.SyndicationRight = "open";
            osd.AdultContent = "false";
            osd.Language = "en-us";
            osd.OutputEncoding = "UTF-8";
            osd.InputEncoding = "UTF-8";
            osd.Developer = "Terradue GeoSpatial Development Team";

            // Create the union Link

            OpenSearchDescriptionUrl url = new OpenSearchDescriptionUrl("application/atom+xml", "dummy://dummy" , "results");

            osd.Url = new OpenSearchDescriptionUrl[1];
            osd.Url[0] = url;

            return osd;
        }

        public NameValueCollection GetOpenSearchParameters(string mimeType) {
            if (mimeType != "application/atom+xml") return null;
            return OpenSearchFactory.MergeOpenSearchParameters(entities.ToArray(), "application/atom+xml");
        }

        public OpenSearchRequest Create(string type, NameValueCollection parameters) {

            OpenSearchUrl url = GetInternalOpenSearchUrl(parameters);

            return new MultiAtomOpenSearchRequest(ose, entities.ToArray(), type, url);

        }

        public ulong TotalResults () {

            ulong count = 0;

            foreach (IOpenSearchable entity in entities) {
                count = count + entity.TotalResults();
            }
            return count;
        }

        public void ApplyResultFilters(ref IOpenSearchResult osr) {

        }

        public Tuple<string, Func<OpenSearchResponse, object>> GetTransformFunction(OpenSearchEngine ose, Type resultType) {
            return OpenSearchFactory.BestTransformFunctionByNumberOfParam(this, ose.GetExtension(resultType));
        }

        public OpenSearchUrl GetSearchBaseUrl(string mimetype) {
            throw new NotImplementedException();
        }

        #endregion

    }
}

