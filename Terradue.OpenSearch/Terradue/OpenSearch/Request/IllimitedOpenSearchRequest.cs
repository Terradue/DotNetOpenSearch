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
        
        static List<IllimitedOpenSearchRequestState> requestStatesCache = new List<IllimitedOpenSearchRequestState>();
        string type;
        NameValueCollection originalParameters, entitiesParameters, currentParameters;
        OpenSearchEngine ose;
        CountdownEvent countdown;
        KeyValuePair<IOpenSearchable, int> currentEntities;
        KeyValuePair<IOpenSearchable,IOpenSearchResultCollection> results;
        TFeed feed;
        bool usingCache = false;
        long totalResults = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="Terradue.OpenSearch.Request.MultiOpenSearchRequest"/> class.
        /// </summary>
        /// <param name="ose">Instance of OpenSearchEngine, preferably with the AtomOpenSearchEngineExtension registered</param>
        /// <param name="entities">IOpenSearchable entities to be searched.</param>
        /// <param name="type">contentType of the .</param>
        /// <param name="url">URL.</param>
        public IllimitedOpenSearchRequest(OpenSearchEngine ose, IOpenSearchable entity, string type, OpenSearchUrl url) : base(url, type) {
            this.ose = ose;
            this.type = type;
            this.originalParameters = HttpUtility.ParseQueryString(url.Query);
            this.entitiesParameters = RemovePaginationParameters(this.originalParameters);
            // Ask the cache if a previous page request is present to save some requests
            usingCache = GetClosestState(entity, type, this.originalParameters, out this.currentEntities, out this.currentParameters);



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
                return originalParameters;
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


            int currentStartIndex = 1;
            int originalStartIndex = 1;
            try {
                currentStartIndex = int.Parse(currentParameters[OpenSearchFactory.ReverseTemplateOpenSearchParameters(OpenSearchFactory.GetBaseOpenSearchParameter())["startIndex"]]);
            } catch (Exception) {
                currentParameters[OpenSearchFactory.ReverseTemplateOpenSearchParameters(OpenSearchFactory.GetBaseOpenSearchParameter())["startIndex"]] = currentStartIndex.ToString();
            }

            try {
                originalStartIndex = int.Parse(originalParameters[OpenSearchFactory.ReverseTemplateOpenSearchParameters(OpenSearchFactory.GetBaseOpenSearchParameter())["startIndex"]]);
            } catch (Exception) {
            }


            // new page -> new feed
            feed = new TFeed();

            totalResults = 0;

            // While we do not have the count needed for our results
            // and that all the sources have are not empty
            while (feed.Items.Count() < originalStartIndex - currentStartIndex && emptySources == false) {

                feed = new TFeed();

                //
                ExecuteOneRequest(currentEntities.Key);

                MergeResults();

                feed.Items = feed.Items.Take(originalStartIndex - currentStartIndex);

                SetCurrentEntitiesOffset(count);

                emptySources = (results.Value.TotalResults < currentEntities.Value);

                currentStartIndex += count;

                currentParameters[OpenSearchFactory.ReverseTemplateOpenSearchParameters(OpenSearchFactory.GetBaseOpenSearchParameter())["startIndex"]] = currentStartIndex.ToString();

                CacheCurrentState();

            }



            while (currentStartPage <= originalStartPage) {

                // new page -> new feed
                feed = new TFeed();

                // count=0 case for totalResults
                if (count == 0) {
                    ExecuteOneRequest(currentEntities.Key);
                    MergeResults();
                    feed = new TFeed();
                }

                // While we do not have the count needed for our results
                // and that all the sources have are not empty
                while (feed.Items.Count() < count && emptySources == false) {

                    //
                    ExecuteOneRequest(currentEntities.Key);

                    MergeResults();

                    SetCurrentEntitiesOffset(count);

                    emptySources = (results.Value.TotalResults < currentEntities.Value);

                }

                if (currentStartPage == 1) {
                    // nest startIndex
                    currentStartIndex += feed.Items.Count();
                    currentParameters[OpenSearchFactory.ReverseTemplateOpenSearchParameters(OpenSearchFactory.GetBaseOpenSearchParameter())["startIndex"]] = currentStartIndex.ToString();
                    // lets cache and then reset
                    CacheCurrentState();
                    currentStartIndex -= feed.Items.Count();
                    currentParameters[OpenSearchFactory.ReverseTemplateOpenSearchParameters(OpenSearchFactory.GetBaseOpenSearchParameter())["startIndex"]] = currentStartIndex.ToString();
                }

                // next page
                currentStartPage++;
                currentParameters[OpenSearchFactory.ReverseTemplateOpenSearchParameters(OpenSearchFactory.GetBaseOpenSearchParameter())["startPage"]] = currentStartPage.ToString();
                CacheCurrentState();

            }

            sw.Stop();
            feed.TotalResults = totalResults;
            if (!emptySources)
                feed.TotalResults += 1;
            
            feed.OpenSearchable = currentEntities.Key;
            feed.QueryTimeSpan = sw.Elapsed;

        }


        private void CatchException(Exception ex) {
            throw ex;
        }

        /// <summary>
        /// Executes one request.
        /// </summary>
        /// <param name="entity">Entity.</param>
        void ExecuteOneRequest(IOpenSearchable entity) {

            try {
                int offset = currentEntities.Value;

                NameValueCollection entityParameters = new NameValueCollection(entitiesParameters);

                entityParameters["startIndex"] = offset.ToString();

                IOpenSearchResultCollection result = ose.Query((IOpenSearchable)entity, entityParameters);
                results = new KeyValuePair<IOpenSearchable, IOpenSearchResultCollection>(entity, result);
            } catch (Exception ex) {
                TFeed result = new TFeed();
                result.Id = "Exception";
                result.ElementExtensions.Add(new SyndicationElementExtension("exception", "", 
                                                                             new ExceptionMessage {
                    Message = ex.Message,
                    Source = ex.Source,
                    HelpLink = ex.HelpLink
                }
                )
                );
                results = new KeyValuePair<IOpenSearchable, IOpenSearchResultCollection>(entity, result);
            }

        }

        /// <summary>
        /// Merges the results.
        /// </summary>
        void MergeResults() {

            IOpenSearchResultCollection result = results.Value;

            TFeed f1 = (TFeed)result;

            if (f1.ElementExtensions.ReadElementExtensions<ExceptionMessage>("exception", "").Count > 0) {
                foreach (var ext in f1.ElementExtensions) {
                    if (ext.OuterName == "exception")
                        feed.ElementExtensions.Add(ext);
                }
            }

            feed = Merge(feed, f1);

            totalResults += f1.Count;
        }

        /// <summary>
        /// Merge SyndicationFeed
        /// </summary>
        /// <param name="f1">F1.</param>
        /// <param name="f2">F2.</param>
        TFeed Merge(TFeed f1, TFeed f2) {

            TFeed feed = (TFeed)f1.Clone();

            int originalCount;
            try {
                originalCount = int.Parse(originalParameters["count"]);
            } catch (Exception e) {
                originalCount = ose.DefaultCount;
            }

            if (feed.Items.Count() >= originalCount) {

                if (f1.Items.Count() != 0 && f2.Items.Count() != 0) {
                    if (f1.Items.Last().LastUpdatedTime >= f2.Items.First().LastUpdatedTime)
                        return feed;
                }

            }

            feed.Items = f1.Items.Union(f2.Items).OrderBy(u => u.Id).OrderByDescending(u => u.LastUpdatedTime);

            feed.Items = feed.Items.Take(originalCount);
                
            return (TFeed)feed.Clone();
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
        public static bool GetClosestState(IOpenSearchable entity, string type, NameValueCollection parameters, out KeyValuePair<IOpenSearchable, int> entity2, out NameValueCollection parameters2) {

            entity2 = new KeyValuePair<IOpenSearchable, int>();

            // Find the request states with the same combination of opensearchable items
            List<IllimitedOpenSearchRequestState> states = requestStatesCache.FindAll(s => s.Entity.Key.Identifier == entity.Identifier && s.Entity.Key.GetType() == entity.Identifier.GetType());
            // and with the same opensearch parameters
            states = states.FindAll(s => OpenSearchFactory.PaginationFreeEqual(s.Parameters, parameters) == true);

            // if no such state, create new one with the entity pagination parameter unset (=1)
            if (states.Count <= 0) {
                entity2 = new KeyValuePair<IOpenSearchable, int>(entity, entity.GetOpenSearchDescription().DefaultUrl.IndexOffset);
                parameters2 = RemovePaginationParameters(parameters);
                return false;
            }

            // if found, what was is the startPage parameter requested (assume 1)
            int startPage = 1;
            try {
                startPage = int.Parse(parameters["startPage"]);
            } catch (Exception) {
            }

            int startIndex = 1;
            try {
                startIndex = int.Parse(parameters["startIndex"]);
            } catch (Exception) {
            }

            // Lets try the find the latest startPage regarding the requested startPage (closest) in the cached states
            List<IllimitedOpenSearchRequestState> temp = states.Where(m => int.Parse(m.Parameters["startPage"]) <= startPage).ToList();
            temp = temp.OrderByDescending(m => int.Parse(m.Parameters["startPage"])).ToList();

            List<IllimitedOpenSearchRequestState> temp2 = temp.Where(m => int.Parse(m.Parameters["startIndex"]) <= startIndex).ToList();
            temp2 = temp2.OrderByDescending(m => int.Parse(m.Parameters["startIndex"])).ToList();

            IllimitedOpenSearchRequestState state = temp2.FirstOrDefault();

            // If not, useless and create new one with the entity pagination parameter unset (=1)
            if (state.Entity.Key == null) {
                if (entity is IProxiedOpenSearchable) {
                    entity2 = new KeyValuePair<IOpenSearchable, int>(entity, ((IProxiedOpenSearchable)entity).GetProxyOpenSearchDescription().Url.FirstOrDefault(p => p.Type == type).IndexOffset);
                } else {
                    entity2 = new KeyValuePair<IOpenSearchable, int>(entity, entity.GetOpenSearchDescription().Url.FirstOrDefault(p => p.Type == type).IndexOffset);
                }
                parameters2 = RemovePaginationParameters(parameters);
                return false;
            }

            //Set the OpenSearch params to the closest pagination params found
            entity2 = state.Entity;
            parameters2 = new NameValueCollection(state.Parameters);

            return true;

        }

        void SetCurrentEntitiesOffset(int count) {

            // Add this offset to the current state for this entity
            currentEntities = new KeyValuePair<IOpenSearchable, int>(currentEntities.Key, currentEntities.Value + count);
        }

        /// <summary>
        /// Caches the state of the current request with the subentities offset;
        /// </summary>
        void CacheCurrentState() {

            IllimitedOpenSearchRequestState state = new IllimitedOpenSearchRequestState();
            state.Entity = new KeyValuePair<IOpenSearchable, int>(currentEntities.Key, currentEntities.Value);
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
        struct IllimitedOpenSearchRequestState {
            public KeyValuePair<IOpenSearchable, int> Entity;
            public NameValueCollection Parameters;
            public string Type;
        }
    }
}

