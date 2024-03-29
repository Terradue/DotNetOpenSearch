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
using System.Security.Cryptography;

namespace Terradue.OpenSearch
{

    /// <summary>
    /// Represents a set of OpenSearchable as a unique OpenSearshable
    /// </summary>
    public class MultiGenericOpenSearchable : IOpenSearchable
    {

        List<IOpenSearchable> entities;

        OpenSearchableFactorySettings settings;

        bool concurrent;

        /// <summary>
        /// Initializes a new instance of the <see cref="Terradue.OpenSearch.MultiGenericOpenSearchable"/> class.
        /// </summary>
        /// <param name="entities">Entities.</param>
        /// <param name="ose">Ose.</param>
        public MultiGenericOpenSearchable(List<IOpenSearchable> entities, OpenSearchableFactorySettings settings, bool concurrent = false)
        {
            this.concurrent = concurrent;
            this.settings = settings;
            this.entities = new List<IOpenSearchable>(entities);
        }

        public string Identifier
        {
            get;
            set;
        }

        string Description
        {
            get;
            set;
        }

        public IComparer<IOpenSearchResultItem> SortingComparer { get; set; }

        /// <summary>
        /// Gets the internal open search URL.
        /// </summary>
        /// <returns>The internal open search URL.</returns>
        /// <param name="parameters">Parameters.</param>
        protected OpenSearchUrl GetInternalOpenSearchUrl(NameValueCollection parameters)
        {
            long hash = 0;
            entities.SingleOrDefault(e =>
            {
                hash += Math.Abs(e.Identifier.GetHashCode());
                return false;
            });
            UriBuilder url = new UriBuilder(string.Format("http://{0}", System.Environment.MachineName));
            url.Path += string.Format("/multi/{0}", hash);
            var array = (from key in parameters.AllKeys
                         from value in parameters.GetValues(key)
                         select string.Format("{0}={1}", HttpUtility.UrlEncode(key), HttpUtility.UrlEncode(value)))
                .ToArray();
            url.Query = string.Join("&", array);
            return new OpenSearchUrl(url.ToString());
        }

        #region IOpenSearchable implementation

        public OpenSearchDescription GetOpenSearchDescription()
        {

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

            OpenSearchDescriptionUrl url = new OpenSearchDescriptionUrl("application/atom+xml", "dummy://dummy", "results", osd.ExtraNamespace);

            osd.Url = new OpenSearchDescriptionUrl[1];
            osd.Url[0] = url;

            return osd;
        }

        public NameValueCollection GetOpenSearchParameters(string mimeType)
        {
            if (mimeType != "application/atom+xml")
                return null;
            return OpenSearchFactory.MergeOpenSearchParameters(entities.ToArray(), "application/atom+xml");
        }

        public OpenSearchRequest Create(QuerySettings querySettings, NameValueCollection parameters)
        {

            OpenSearchUrl url = GetInternalOpenSearchUrl(parameters);

            var mosr = new MultiOpenSearchRequest<AtomFeed, AtomItem>(settings, entities.ToArray(), querySettings.PreferredContentType, url, concurrent, this);
            if ( SortingComparer != null )
                mosr.SortingComparer = SortingComparer;

            return mosr;

        }

        public long TotalResults
        {
            get
            {

                long count = 0;

                foreach (IOpenSearchable entity in entities)
                {
                    count = count + entity.TotalResults;
                }
                return count;
            }
        }

        public void ApplyResultFilters(OpenSearchRequest request, ref IOpenSearchResultCollection osr, string finalContentType)
        {

        }

        public QuerySettings GetQuerySettings(OpenSearchEngine ose)
        {
            IOpenSearchEngineExtension osee = ose.GetExtensionByContentTypeAbility(this.DefaultMimeType);
            if (osee == null)
                return null;
            return new QuerySettings(this.DefaultMimeType, osee.ReadNative);
        }

        public string DefaultMimeType
        {
            get
            {
                return "application/atom+xml";
            }
        }

        public bool CanCache
        {
            get
            {
                return true;
            }
        }

        #endregion

    }
}

