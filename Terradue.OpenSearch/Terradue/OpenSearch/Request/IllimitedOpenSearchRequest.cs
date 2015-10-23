//
//  MultiOpenSearchRequest.cs
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
    /// Opensearch Collection response.
    /// </description>
    public class IllimitedOpenSearchRequest<TFeed, TItem> : OpenSearchRequest where TFeed: class, IOpenSearchResultCollection, ICloneable, new() where TItem: class, IOpenSearchResultItem {

        private log4net.ILog log = log4net.LogManager.GetLogger
            (System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        static List<IllimitedOpenSearchRequestState> requestStatesCache = new List<IllimitedOpenSearchRequestState>();
        string type;
        NameValueCollection targetParameters, virtualParameters, currentParameters;

        NameValueCollection originalParameters;

        OpenSearchEngine ose;
        long currentTotalResults;
        IOpenSearchResultCollection currentResults;
        TFeed feed;
        bool usingCache = false;

        IOpenSearchable entity;

        /// <summary>
        /// Initializes a new instance of the <see cref="Terradue.OpenSearch.Request.MultiOpenSearchRequest"/> class.
        /// </summary>
        /// <param name="ose">Instance of OpenSearchEngine, preferably with the AtomOpenSearchEngineExtension registered</param>
        /// <param name="entities">IOpenSearchable entities to be searched.</param>
        /// <param name="type">contentType of the .</param>
        /// <param name="url">URL.</param>
        public IllimitedOpenSearchRequest(OpenSearchEngine ose, IOpenSearchable entity, string type, OpenSearchUrl url, NameValueCollection originalParameters = null) : base(url, type) {
            this.originalParameters = originalParameters;
            this.entity = entity;
            this.ose = ose;
            this.type = type;
            this.targetParameters = HttpUtility.ParseQueryString(url.Query);
            this.virtualParameters = RemovePaginationParameters(this.targetParameters);


            // Ask the cache if a previous page request is present to save some requests
            usingCache = GetClosestState(entity, type, this.targetParameters, out currentTotalResults, out this.currentParameters);


        }

        #region implemented abstract members of OpenSearchRequest

        public override IOpenSearchResponse GetResponse() {

            Stopwatch sw = Stopwatch.StartNew();
            RequestCurrentPage();
            sw.Stop();
            return new OpenSearchResponse<TFeed>(feed, type, sw.Elapsed);


        }

        public override NameValueCollection OriginalParameters {
            get {
                if (originalParameters != null)
                    return originalParameters;
                return targetParameters;
            }
            set {
                originalParameters = value;
            }
        }

        #endregion

        /// <summary>
        /// Requests the current page.
        /// </summary>
        private void RequestCurrentPage() {

            Stopwatch sw = Stopwatch.StartNew();

            bool emptySources = false;
            int count = ose.DefaultCount;

            try {
                count = int.Parse(targetParameters["count"]);
            } catch (Exception) {
                currentParameters.Add("count", count.ToString());
            }

            int currentStartPage = 1;
            int targetStartPage = 1;
            try {
                currentStartPage = int.Parse(currentParameters[OpenSearchFactory.ReverseTemplateOpenSearchParameters(OpenSearchFactory.GetBaseOpenSearchParameter())["startPage"]]);
            } catch (Exception) {
                currentParameters[OpenSearchFactory.ReverseTemplateOpenSearchParameters(OpenSearchFactory.GetBaseOpenSearchParameter())["startPage"]] = currentStartPage.ToString();
            }

            try {
                targetStartPage = int.Parse(targetParameters[OpenSearchFactory.ReverseTemplateOpenSearchParameters(OpenSearchFactory.GetBaseOpenSearchParameter())["startPage"]]);
            } catch (Exception) {

            }


            int currentStartIndex = 1;
            int targetStartIndex = 1;
            try {
                currentStartIndex = int.Parse(currentParameters[OpenSearchFactory.ReverseTemplateOpenSearchParameters(OpenSearchFactory.GetBaseOpenSearchParameter())["startIndex"]]);
            } catch (Exception) {
                currentParameters[OpenSearchFactory.ReverseTemplateOpenSearchParameters(OpenSearchFactory.GetBaseOpenSearchParameter())["startIndex"]] = currentStartIndex.ToString();
            }

            try {
                targetStartIndex = int.Parse(targetParameters[OpenSearchFactory.ReverseTemplateOpenSearchParameters(OpenSearchFactory.GetBaseOpenSearchParameter())["startIndex"]]);
            } catch (Exception) {
            }


            // new page -> new feed
            feed = new TFeed();

            int fibocount = count;

            // While we do not have the count needed to reach the start index, we query
            while (currentTotalResults + 1 < targetStartIndex && emptySources == false) {

                log.DebugFormat("TR:{0} CSI:{1} TSI:{2}", currentTotalResults, currentStartIndex, targetStartIndex);
                log.DebugFormat("Query {0}", fibocount);

                // we start with a new feed. lets query
                feed = ExecuteOneRequest(entity, fibocount);

                log.DebugFormat("Got {0}", feed.Items.Count());

                currentTotalResults += feed.Items.Count();

                emptySources = (currentResults.TotalResults < currentStartIndex);

                currentStartIndex += fibocount;

                currentParameters[OpenSearchFactory.ReverseTemplateOpenSearchParameters(OpenSearchFactory.GetBaseOpenSearchParameter())["startIndex"]] = currentStartIndex.ToString();

                CacheCurrentState();

                log.DebugFormat("TR:{0} CSI:{1} TSI:{2} [cached]", currentTotalResults, currentStartIndex, targetStartIndex);

                fibocount += fibocount;

            }

            virtualParameters[OpenSearchFactory.ReverseTemplateOpenSearchParameters(OpenSearchFactory.GetBaseOpenSearchParameter())["startIndex"]] = currentTotalResults.ToString();


            while (currentStartPage <= targetStartPage) {

                fibocount = count;

                log.DebugFormat("TR:{0} CSI:{1} TSI:{2} CP:{3} TP:{4} [new page]", currentTotalResults, currentStartIndex, targetStartIndex, currentStartPage, targetStartPage);
                // new page -> new feed
                feed = null;

                // While we do not have the count needed for our results
                // and that all the sources have are not empty
                while ((feed == null || feed.Items.Count() < count) && emptySources == false) {

                    if (feed != null) {
                        
                    }

                    log.DebugFormat("TR:{0} CSI:{1} TSI:{2} CP:{3} TP:{4}", currentTotalResults, currentStartIndex, targetStartIndex, currentStartPage, targetStartPage);

                    log.DebugFormat("Query {0}", fibocount);
                    TFeed feed1 = ExecuteOneRequest(entity, fibocount);
                    log.DebugFormat("Got {0}", feed1.Items.Count());
                    currentTotalResults += feed1.Items.Count();

                    if (feed == null) {
                        feed = feed1;
                    } else {
                        log.DebugFormat("Add {0} to {1}", feed1.Items.Count(), feed.Items.Count());
                        List<TItem> newItems = feed.Items.Cast<TItem>().ToList();
                        newItems.AddRange(feed1.Items.Cast<TItem>().ToList());
                        feed.Items = newItems;
                    }

                    log.DebugFormat("CRTR:{0} TR:{1} CSI:{2}", currentResults.TotalResults, currentTotalResults, currentStartIndex);
                    emptySources = (currentResults.TotalResults < currentStartIndex);

                    log.DebugFormat("Keep {0} out of {1}", count, feed.Items.Count());

                    feed.Items = feed.Items.Take(count);

                    if (feed.Items.Count() < count) {
                        currentStartIndex += fibocount;
                        currentParameters[OpenSearchFactory.ReverseTemplateOpenSearchParameters(OpenSearchFactory.GetBaseOpenSearchParameter())["startIndex"]] = currentStartIndex.ToString();
                    } else {
                        currentStartIndex += feed1.Items.TakeWhile(i => i != feed.Items.Last()).Count() + 2;
                        currentParameters[OpenSearchFactory.ReverseTemplateOpenSearchParameters(OpenSearchFactory.GetBaseOpenSearchParameter())["startIndex"]] = currentStartIndex.ToString();

                    }



                    CacheCurrentState();
                    log.DebugFormat("TR:{0} CSI:{1} TSI:{2} CP:{3} TP:{4} [cached]", currentTotalResults, currentStartIndex, targetStartIndex, currentStartPage, targetStartPage);

                    fibocount += fibocount;
                }

                // next page
                currentStartPage++;
                virtualParameters[OpenSearchFactory.ReverseTemplateOpenSearchParameters(OpenSearchFactory.GetBaseOpenSearchParameter())["startPage"]] = currentStartPage.ToString();


            }

            sw.Stop();
            if (feed == null)
                feed = new TFeed();

            feed.TotalResults = currentTotalResults + (emptySources == true ? 0 : 1);
            
            feed.OpenSearchable = entity;
            feed.QueryTimeSpan = sw.Elapsed;

        }


        private void CatchException(Exception ex) {
            throw ex;
        }

        /// <summary>
        /// Executes one request.
        /// </summary>
        /// <param name="entity">Entity.</param>
        TFeed ExecuteOneRequest(IOpenSearchable entity, int recount) {

            NameValueCollection nvc = new NameValueCollection(currentParameters);
            nvc.Set("count", recount.ToString());

            currentResults = ose.Query((IOpenSearchable)entity, nvc);

            return (TFeed)currentResults;
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
        public static bool GetClosestState(IOpenSearchable entity, string type, NameValueCollection parameters, out long totalResults, out NameValueCollection parameters2) {

            totalResults = 0;
            int currentStartIndex = 1;

            if (entity is IProxiedOpenSearchable) {
                currentStartIndex = ((IProxiedOpenSearchable)entity).GetProxyOpenSearchDescription().Url.FirstOrDefault(p => p.Type == type).IndexOffset;
            } else {
                currentStartIndex = entity.GetOpenSearchDescription().Url.FirstOrDefault(p => p.Type == type).IndexOffset;
            }

            parameters2 = RemovePaginationParameters(parameters);
            parameters2[OpenSearchFactory.ReverseTemplateOpenSearchParameters(OpenSearchFactory.GetBaseOpenSearchParameter())["startIndex"]] = currentStartIndex.ToString();

            // Find the request states with the same combination of opensearchable items
            List<IllimitedOpenSearchRequestState> states = requestStatesCache.FindAll(s => {
                return s.Entity.Identifier == entity.Identifier && s.Entity.GetType() == entity.GetType();
            });
            // and with the same opensearch parameters
            states = states.FindAll(s => OpenSearchFactory.PaginationFreeEqual(s.Parameters, parameters) == true);

            // if no such state, create new one with the entity pagination parameter unset (=1)
            if (states.Count <= 0) {
                return false;
            }

            // if found, what was is the startPage parameter requested (assume 1)
            int targetStartPage = 1;
            try {
                targetStartPage = int.Parse(parameters["startPage"]);
            } catch (Exception) {
            }

            int targetStartIndex = 1;
            try {
                targetStartIndex = int.Parse(parameters["startIndex"]);
            } catch (Exception) {
            }

            // Lets try the find the latest startPage regarding the requested startPage (closest) in the cached states
            List<IllimitedOpenSearchRequestState> temp = states.Where(m => int.Parse(m.Parameters["startPage"]) <= targetStartPage).ToList();
            temp = temp.OrderByDescending(m => int.Parse(m.Parameters["startPage"])).ToList();

            List<IllimitedOpenSearchRequestState> temp2 = temp.Where(m => int.Parse(m.Parameters["startIndex"]) <= targetStartIndex).ToList();
            temp2 = temp2.OrderByDescending(m => int.Parse(m.Parameters["startIndex"])).ToList();

            IllimitedOpenSearchRequestState state = temp2.FirstOrDefault();

            // If not, useless and create new one with the entity pagination parameter unset (=1)
            if (state.Entity == null) {
                return false;
            }

            //Set the OpenSearch params to the closest pagination params found
            parameters2 = new NameValueCollection(state.Parameters);
            totalResults = state.TotalResults;

            return true;

        }

        /// <summary>
        /// Caches the state of the current request with the subentities offset;
        /// </summary>
        void CacheCurrentState() {

            IllimitedOpenSearchRequestState state = new IllimitedOpenSearchRequestState();
            state.Entity = entity;
            state.Parameters = new NameValueCollection(currentParameters);
            state.Type = type;
            state.TotalResults = currentTotalResults;

            requestStatesCache.Add(state);

        }

        static NameValueCollection RemovePaginationParameters(NameValueCollection parameters) {
            NameValueCollection nvc = new NameValueCollection(parameters);
            nvc.Remove(OpenSearchFactory.ReverseTemplateOpenSearchParameters(OpenSearchFactory.GetBaseOpenSearchParameter())["startPage"]);
            nvc.Remove(OpenSearchFactory.ReverseTemplateOpenSearchParameters(OpenSearchFactory.GetBaseOpenSearchParameter())["startIndex"]);
            return nvc;
        }

        /// Structure to hold a request state
        struct IllimitedOpenSearchRequestState {
            public IOpenSearchable Entity;
            public NameValueCollection Parameters;
            public string Type;
            public long TotalResults;
        }
    }
}

