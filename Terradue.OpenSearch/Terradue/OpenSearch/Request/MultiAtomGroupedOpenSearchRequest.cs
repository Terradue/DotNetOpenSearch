//
//  MultiAtomOpenSearchRequest.cs
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

namespace Terradue.OpenSearch.Request {
    /// <summary>
    /// An OpenSearchRequest able to request multiple IOpenSearchable as a unique one.
    /// </summary>
    /// <description>
    /// This request will return an atom response and thus the entities requested must be able to return 
    /// Atom response.
    /// </description>
    public class MultiAtomGroupedOpenSearchRequest : OpenSearchRequest {

        string type;
        NameValueCollection parameters;
        OpenSearchEngine ose;
        CountdownEvent countdown;
        IOpenSearchable[] entities;
        Dictionary<IOpenSearchable,IOpenSearchResultCollection> results;
        AtomFeed feed;

        bool concurrent = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="Terradue.OpenSearch.Request.MultiAtomOpenSearchRequest"/> class.
        /// </summary>
        /// <param name="ose">Instance of OpenSearchEngine, preferably with the AtomOpenSearchEngineExtension registered</param>
        /// <param name="entities">IOpenSearchable entities to be searched.</param>
        /// <param name="type">contentType of the .</param>
        /// <param name="url">URL.</param>
        public MultiAtomGroupedOpenSearchRequest(OpenSearchEngine ose, IOpenSearchable[] entities, string type, OpenSearchUrl url, bool concurrent) : base(url, type) {
            this.concurrent = concurrent;

            this.ose = ose;
            this.type = type;
            this.parameters = HttpUtility.ParseQueryString(url.Query);
            this.entities = entities;

        }

        #region implemented abstract members of OpenSearchRequest

        public override IOpenSearchResponse GetResponse() {

            Stopwatch sw = Stopwatch.StartNew();
            RequestCurrentPage();
            sw.Stop();
            return new AtomOpenSearchResponse(feed, sw.Elapsed);


        }

        public override NameValueCollection OriginalParameters {
            get {
                return parameters;
            }
            set {
                parameters = value;
            }
        }


        #endregion

        /// <summary>
        /// Requests the current page.
        /// </summary>
        private void RequestCurrentPage() {

            PaginatedList<IOpenSearchable> pds = new PaginatedList<IOpenSearchable>();

            int startIndex = 1;
            if (parameters["startIndex"] != null)
                startIndex = int.Parse(parameters["startIndex"]);

            pds.PageNo = 1;
            if (parameters["startPage"] != null)
                pds.PageNo = int.Parse(parameters["startPage"]);

            pds.PageSize = ose.DefaultCount;
            if (parameters["count"] != null)
                pds.PageSize = int.Parse(parameters["count"]);

            pds.StartIndex = startIndex - 1;

            pds.AddRange(entities);

            ExecuteConcurrentRequest(pds.GetCurrentPage());

            MergeResults();
        }

        /// <summary>
        /// Executes concurrent requests.
        /// </summary>
        private void ExecuteConcurrentRequest(List<IOpenSearchable> ents) {

            countdown = new CountdownEvent(ents.Count);
            results = new Dictionary<IOpenSearchable, IOpenSearchResultCollection>();

            foreach (IOpenSearchable entity in ents) {
                if (concurrent) {
                    Thread queryThread = new Thread(this.ExecuteOneRequest);
                    queryThread.Start(entity);
                } else {

                    ExecuteOneRequest(entity);
                }
            }

            countdown.Wait();

        }

        /// <summary>
        /// Executes one request.
        /// </summary>
        /// <param name="entity">Entity.</param>
        void ExecuteOneRequest(object entity) {

            IOpenSearchResultCollection result = ose.Query((IOpenSearchable)entity, parameters, typeof(AtomFeed));
            results.Add((IOpenSearchable)entity, result);
            countdown.Signal();

        }

        /// <summary>
        /// Merges the results.
        /// </summary>
        void MergeResults() {

            feed = new AtomFeed();
            List<AtomItem> items = new List<AtomItem>();

            foreach (var key in results.Keys) {

                IOpenSearchResultCollection result = results[key];
                AtomItem item = null;

                if (result.Count == 1) {
                    item = AtomItem.FromOpenSearchResultItem(result.Items.First());
                } else {
                    item = new AtomItem(result.Title.Text, 
                                        result.Description, 
                                        result.Links.FirstOrDefault(l => l.RelationshipType == "self").Uri,
                                        result.Id,
                                        result.Date);
                    item.Identifier = result.Identifier;
                    item.ElementExtensions = result.ElementExtensions;
                    result.Authors.FirstOrDefault( a => {
                        item.Authors.Add(a);
                        return false;
                    });
                    result.Categories.FirstOrDefault( a => {
                        item.Categories.Add(a);
                        return false;
                    });
                    item.Categories.Add(new SyndicationCategory("group", null, result.Id));
                    result.Contributors.FirstOrDefault( a => {
                        item.Contributors.Add(a);
                        return false;
                    });
                    item.Copyright = result.Copyright;
                    item.Links = result.Links;
                    item.PublishDate = result.Date;
                    item.SourceFeed = (SyndicationFeed)AtomFeed.CreateFromOpenSearchResultCollection(result);
                }
                items.Add(item);				
            }
            feed.Items = items;
        }
    }
}

