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
    public class MultiAtomOpenSearchRequest : OpenSearchRequest {
        static List<MultiAtomOpenSearchRequestState> requestStatesCache = new List<MultiAtomOpenSearchRequestState>();
        string type;
        NameValueCollection originalParameters, entitiesParameters, currentParameters;
        OpenSearchEngine ose;
        CountdownEvent countdown;
        Dictionary<IOpenSearchable, int> currentEntities;
        Dictionary<IOpenSearchable,IOpenSearchResult> results;
        AtomFeed feed;
        bool usingCache = false;
        ulong totalResults = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="Terradue.OpenSearch.Request.MultiAtomOpenSearchRequest"/> class.
        /// </summary>
        /// <param name="ose">Instance of OpenSearchEngine, preferably with the AtomOpenSearchEngineExtension registered</param>
        /// <param name="entities">IOpenSearchable entities to be searched.</param>
        /// <param name="type">contentType of the .</param>
        /// <param name="url">URL.</param>
        public MultiAtomOpenSearchRequest(OpenSearchEngine ose, IOpenSearchable[] entities, string type, OpenSearchUrl url) : base(url) {

            this.ose = ose;
            this.type = type;
            this.originalParameters = HttpUtility.ParseQueryString(url.Query);
            this.entitiesParameters = RemovePaginationParameters(this.originalParameters);
            // Ask the cache if a previous page request is present to save some requests
            usingCache = GetClosestState(entities, type, this.originalParameters, out this.currentEntities, out this.currentParameters);

        }

        #region implemented abstract members of OpenSearchRequest

        public override OpenSearchResponse GetResponse() {

            Stopwatch sw = Stopwatch.StartNew();
            RequestCurrentPage();
            sw.Stop();
            return new AtomOpenSearchResponse(feed, sw.Elapsed);


        }

        #endregion

        /// <summary>
        /// Requests the current page.
        /// </summary>
        private void RequestCurrentPage() {
            bool emptySources = false;
            int count = ose.DefaultCount;

            try {
                count = int.Parse(originalParameters["count"]);
            } catch (Exception) {
                currentParameters.Add("count", count.ToString());
            }

            int currentStartPage = 1;
            int originalStartPage = 1;
            try {
                currentStartPage = int.Parse(currentParameters[OpenSearchFactory.ReverseTemplateOpenSearchParameters(OpenSearchFactory.GetBaseOpenSearchParameter())["startPage"]]);
            } catch (Exception) {
                currentParameters[OpenSearchFactory.ReverseTemplateOpenSearchParameters(OpenSearchFactory.GetBaseOpenSearchParameter())["startPage"]] = currentStartPage.ToString();
            }

            try {
                originalStartPage = int.Parse(originalParameters[OpenSearchFactory.ReverseTemplateOpenSearchParameters(OpenSearchFactory.GetBaseOpenSearchParameter())["startPage"]]);
            } catch (Exception) {

            }

            PrepareTotalResults();

            while (currentStartPage <= originalStartPage) {

                // new page -> new feed
                feed = new AtomFeed();

                // count=0 case for totalResults
                if (count == 0) {
                    ExecuteConcurrentRequest();
                    MergeResults();
                    feed = new AtomFeed();
                }

                // While we do not have the count needed for our results
                // and that all the sources have are not empty
                while (feed.Items.Count() < count && emptySources == false) {

                    //
                    ExecuteConcurrentRequest();

                    MergeResults();

                    SetCurrentEntitiesOffset();

                    var r1 = results.Values.FirstOrDefault(r => {
                        SyndicationFeed result = (SyndicationFeed)r.Result;
                        if (result.Items.Count() > 0) return true;
                        return false;
                    });

                    emptySources = (r1 == null);

                }

                // next page
                currentStartPage++;
                currentParameters[OpenSearchFactory.ReverseTemplateOpenSearchParameters(OpenSearchFactory.GetBaseOpenSearchParameter())["startPage"]] = currentStartPage.ToString();
                CacheCurrentState();

            }
                
            feed.ElementExtensions.Add("totalResults", "http://a9.com/-/spec/opensearch/1.1/", totalResults);
        }

        /// <summary>
        /// Executes concurrent requests.
        /// </summary>
        private void ExecuteConcurrentRequest() {

            countdown = new CountdownEvent(currentEntities.Count);
            results = new Dictionary<IOpenSearchable, IOpenSearchResult>();

            foreach (IOpenSearchable entity in currentEntities.Keys) {
                //Thread queryThread = new Thread(this.ExecuteOneRequest);
                //queryThread.Start(entity);
                ExecuteOneRequest(entity);
            }

            //countdown.Wait();



        }

        /// <summary>
        /// Executes one request.
        /// </summary>
        /// <param name="entity">Entity.</param>
        void ExecuteOneRequest(object entity) {

            int offset = currentEntities[(IOpenSearchable)entity];

            NameValueCollection entityParameters = new NameValueCollection(entitiesParameters);

            entityParameters["startIndex"] = offset.ToString();

            IOpenSearchResult result = ose.Query((IOpenSearchable)entity, entityParameters, typeof(SyndicationFeed));
            results.Add((IOpenSearchable)entity, result);
            countdown.Signal();

        }

        /// <summary>
        /// Merges the results.
        /// </summary>
        void MergeResults() {

            foreach (IOpenSearchResult result in results.Values) {

                AtomFeed f1 = (AtomFeed)result.Result;

                if (f1.Items.Count() == 0) continue;
                feed = Merge(feed, f1);
				
            }
        }

        /// <summary>
        /// Merge SyndicationFeed
        /// </summary>
        /// <param name="f1">F1.</param>
        /// <param name="f2">F2.</param>
        AtomFeed Merge(AtomFeed f1, AtomFeed f2) {

            AtomFeed feed = new AtomFeed(f1, false);

            int originalCount = ose.DefaultCount;
            try {
                originalCount = int.Parse(originalParameters["count"]);
            } catch (Exception e) {
            }

            if (feed.Items.Count() >= originalCount) {

                if (f1.Items.Count() != 0 && f2.Items.Count() != 0) {
                    if (f1.Items.Last().PublishDate >= f2.Items.First().PublishDate) return feed;
                }

            }

            feed.Items = f1.Items.Union(f2.Items).OrderByDescending(u => u.PublishDate);

            feed.Items = feed.Items.Take(originalCount);


            return new AtomFeed(feed);
        }

        /// <summary>
        /// Prepares the total results.
        /// </summary>
        void PrepareTotalResults() {
            totalResults = 0;
            foreach (IOpenSearchable entity in currentEntities.Keys) {

                totalResults += entity.TotalResults();

            }
        }

        /// <summary>
        /// Gets the cache state
        /// </summary>
        /// <returns><c>true</c>, if closest state was gotten, <c>false</c> otherwise.</returns>
        /// <param name="entities">Entities.</param>
        /// <param name="type">Type.</param>
        /// <param name="parameters">Parameters.</param>
        /// <param name="entities2">Entities2.</param>
        /// <param name="parameters2">Parameters2.</param>
        public static bool GetClosestState(IOpenSearchable[] entities, string type, NameValueCollection parameters, out Dictionary<IOpenSearchable, int> entities2, out NameValueCollection parameters2) {

            entities2 = new Dictionary<IOpenSearchable, int>(entities.Length);

            List<MultiAtomOpenSearchRequestState> states = requestStatesCache.FindAll(s => s.Entities.Keys.ToArray().SequenceEqual(entities, new OpenSearchableComparer()));

            states = states.FindAll(s => OpenSearchFactory.PaginationFreeEqual(s.Parameters, parameters) == true);

            if (states.Count <= 0) {
                foreach (IOpenSearchable entity in entities) entities2.Add(entity, 1);
                parameters2 = RemovePaginationParameters(parameters);
                return false;
            }

            int startPage = 1;

            try {
                startPage = int.Parse(parameters["startPage"]);
            } catch (Exception) {
            }

            MultiAtomOpenSearchRequestState state = 
                states.Where(m => int.Parse(m.Parameters["startPage"]) <= startPage)
					.OrderByDescending(m => int.Parse(m.Parameters["startPage"]))
					.FirstOrDefault();

            if (state.Entities == null) {
                foreach (IOpenSearchable entity in entities) entities2.Add(entity, 1);
                parameters2 = RemovePaginationParameters(parameters);
                return false;
            }

            foreach (IOpenSearchable entity in state.Entities.Keys) {
                entities2.Add(entity, state.Entities[entity]);
            }

            parameters2 = state.Parameters;

            return true;

        }

        void SetCurrentEntitiesOffset() {

            var it = currentEntities.Keys.ToArray();

            foreach (IOpenSearchable entity in it) {

                // the offset for this entity will be the number of items taken from its current result.
                int offset = ((SyndicationFeed)results[entity].Result).Items.Intersect(feed.Items).Count();

                // Add this offset to the current state for this entity
                currentEntities[entity] += offset;

            }
        }

        /// <summary>
        /// Caches the state of the current request with the subentities offset;
        /// </summary>
        void CacheCurrentState() {

            MultiAtomOpenSearchRequestState state = new MultiAtomOpenSearchRequestState();
            state.Entities = new Dictionary<IOpenSearchable, int>(currentEntities);
            state.Parameters = new NameValueCollection(currentParameters);
            state.Type = type;

            requestStatesCache.Add(state);

        }

        static NameValueCollection RemovePaginationParameters(NameValueCollection parameters) {
            NameValueCollection nvc = new NameValueCollection(parameters);
            nvc.Remove(OpenSearchFactory.ReverseTemplateOpenSearchParameters(OpenSearchFactory.GetBaseOpenSearchParameter())["startPage"]);
            nvc.Remove(OpenSearchFactory.ReverseTemplateOpenSearchParameters(OpenSearchFactory.GetBaseOpenSearchParameter())["startIndex"]);
            return nvc;
        }

        /// Structure to hold a request state
        struct MultiAtomOpenSearchRequestState {
            public Dictionary<IOpenSearchable, int> Entities;
            public NameValueCollection Parameters;
            public string Type;
        }
    }
}

