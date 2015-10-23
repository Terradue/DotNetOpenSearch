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
using System.Threading.Tasks;

namespace Terradue.OpenSearch.Request {
    /// <summary>
    /// An OpenSearchRequest able to request multiple IOpenSearchable as a unique one.
    /// </summary>
    /// <description>
    /// This request will return an atom response and thus the entities requested must be able to return 
    /// Opensearch Collection response.
    /// </description>
    public class MultiOpenSearchRequest<TFeed, TItem> : OpenSearchRequest where TFeed: class, IOpenSearchResultCollection, ICloneable, new() where TItem: class, IOpenSearchResultItem {
        static List<MultiOpenSearchRequestState> requestStatesCache = new List<MultiOpenSearchRequestState>();
        string type;
        NameValueCollection originalParameters, entitiesParameters, currentParameters;
        OpenSearchEngine ose;
        CountdownEvent countdown;
        Dictionary<IOpenSearchable, int> currentEntities;
        Dictionary<IOpenSearchable,IOpenSearchResultCollection> results;
        TFeed feed;
        bool usingCache = false;
        long totalResults = 0;

        bool concurrent = true;

        IOpenSearchable parent;

        static Mutex _m;

        private log4net.ILog log = log4net.LogManager.GetLogger
            (System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Initializes a new instance of the <see cref="Terradue.OpenSearch.Request.MultiOpenSearchRequest"/> class.
        /// </summary>
        /// <param name="ose">Instance of OpenSearchEngine, preferably with the AtomOpenSearchEngineExtension registered</param>
        /// <param name="entities">IOpenSearchable entities to be searched.</param>
        /// <param name="type">contentType of the .</param>
        /// <param name="url">URL.</param>
        public MultiOpenSearchRequest(OpenSearchEngine ose, IOpenSearchable[] entities, string type, OpenSearchUrl url, bool concurrent, IOpenSearchable parent) : base(url, type) {
            this.parent = parent;
            this.concurrent = concurrent;

            this.ose = ose;
            this.type = type;
            this.originalParameters = HttpUtility.ParseQueryString(url.Query);
            this.entitiesParameters = RemovePaginationParameters(this.originalParameters);

            _m = new Mutex();

            // Ask the cache if a previous page request is present to save some requests
            usingCache = GetClosestState(entities.Distinct(new OpenSearchableComparer()).ToArray(), type, this.originalParameters, out this.currentEntities, out this.currentParameters);



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
                ExecuteConcurrentRequest();

                MergeResults();

                feed.Items = feed.Items.Take(originalStartIndex - currentStartIndex);

                SetCurrentEntitiesOffset();

                var r1 = results.Values.FirstOrDefault(r => {
                    TFeed result = (TFeed)r;
                    if (result.Items.Count() > 0)
                        return true;
                    return false;
                });

                emptySources = (r1 == null);

                currentStartIndex += feed.Items.Count();

                currentParameters[OpenSearchFactory.ReverseTemplateOpenSearchParameters(OpenSearchFactory.GetBaseOpenSearchParameter())["startIndex"]] = currentStartIndex.ToString();

                CacheCurrentState();

            }



            while (currentStartPage <= originalStartPage) {

                // new page -> new feed
                feed = new TFeed();

                // count=0 case for totalResults
                if (count == 0) {
                    ExecuteConcurrentRequest();
                    MergeResults();
                    feed = new TFeed();
                }

                // While we do not have the count needed for our results
                // and that all the sources have are not empty
                while (feed.Items.Count() < count && emptySources == false) {

                    //
                    ExecuteConcurrentRequest();

                    MergeResults();

                    SetCurrentEntitiesOffset();

                    var r1 = results.Values.FirstOrDefault(r => {
                        TFeed result = (TFeed)r;
                        if (result.Items.Count() > 0)
                            return true;
                        return false;
                    });

                    emptySources = (r1 == null);

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
            feed.OpenSearchable = parent;
            feed.QueryTimeSpan = sw.Elapsed;

        }

        /// <summary>
        /// Executes concurrent requests.
        /// </summary>
        private void ExecuteConcurrentRequest() {

            //countdown = new CountdownEvent(currentEntities.Count);
            results = new Dictionary<IOpenSearchable, IOpenSearchResultCollection>();

            //List<Task> request = new List<Task>();

            Parallel.ForEach<IOpenSearchable>(currentEntities.Keys.Distinct(new OpenSearchableComparer()),
                                              entity => {
                ExecuteOneRequest(entity);
            });

            /*foreach (IOpenSearchable entity in currentEntities.Keys.Distinct(new OpenSearchableComparer())) {
                if (concurrent) {
                    request.Add(Task.Factory.StartNew(() => ExecuteOneRequest(entity)));
                    //Thread queryThread = new Thread(new ParameterizedThreadStart(this.ExecuteOneRequest));
                    //queryThread.Start(entity);
                } else {

                    ExecuteOneRequest(entity);
                }
            }*/

            //Task.WaitAll(request.ToArray());
            //countdown.Wait();

        }

        private void CatchException(Exception ex) {
            throw ex;
        }

        /// <summary>
        /// Executes one request.
        /// </summary>
        /// <param name="entity">Entity.</param>
        void ExecuteOneRequest(object entity) {

            try {
                int offset = currentEntities[(IOpenSearchable)entity];

                NameValueCollection entityParameters = new NameValueCollection(entitiesParameters);

                entityParameters["startIndex"] = offset.ToString();

                IOpenSearchResultCollection result = ose.Query((IOpenSearchable)entity, entityParameters);
                results.Add((IOpenSearchable)entity, result);
                //countdown.Signal();
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
                results.Add((IOpenSearchable)entity, result);
                //countdown.Signal();
            }

        }

        /// <summary>
        /// Merges the results.
        /// </summary>
        void MergeResults() {

            totalResults = 0;

            foreach (IOpenSearchable key in results.Keys) {

                if (!results.ContainsKey(key))
                    continue;
                
                TFeed f1 = (TFeed)results[key];

                if (f1.ElementExtensions.ReadElementExtensions<ExceptionMessage>("exception", "").Count > 0) {
                    foreach (var ext in f1.ElementExtensions) {
                        if (ext.OuterName == "exception")
                            feed.ElementExtensions.Add(ext);
                    }
                }

                totalResults += f1.TotalResults;

                if (f1.Items.Count() == 0)
                    continue;
                feed = Merge(feed, f1, key);

            }
        }

        /// <summary>
        /// Merge SyndicationFeed
        /// </summary>
        /// <param name="f1">F1.</param>
        /// <param name="f2">F2.</param>
        TFeed Merge(TFeed f1, TFeed f2, IOpenSearchable os) {

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

            feed.Items = f1.Items.Union(f2.Items, new OpenSearchResultItemComparer()).OrderBy(u => u.Id).OrderByDescending(u => u.SortKey);

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
        public static bool GetClosestState(IOpenSearchable[] entities, string type, NameValueCollection parameters, out Dictionary<IOpenSearchable, int> entities2, out NameValueCollection parameters2) {

            entities2 = new Dictionary<IOpenSearchable, int>(entities.Length);
            parameters2 = RemovePaginationParameters(parameters);

            try {
                _m.WaitOne();

                // Find the request states with the same combination of opensearchable items
                List<MultiOpenSearchRequestState> states = requestStatesCache.FindAll(s => s.Entities.Keys.ToArray().SequenceEqual(entities, new OpenSearchableComparer()));
                // and with the same opensearch parameters
                states = states.FindAll(s => OpenSearchFactory.PaginationFreeEqual(s.Parameters, parameters) == true);

                // if no such state, create new one with the entity pagination parameter unset (=1)
                if (states.Count <= 0) {
                    foreach (IOpenSearchable entity in entities)
                        entities2.Add(entity, entity.GetOpenSearchDescription().DefaultUrl.IndexOffset);
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
                List<MultiOpenSearchRequestState> temp = states.Where(m => int.Parse(m.Parameters["startPage"]) <= startPage).ToList();
                temp = temp.OrderByDescending(m => int.Parse(m.Parameters["startPage"])).ToList();

                List<MultiOpenSearchRequestState> temp2 = temp.Where(m => int.Parse(m.Parameters["startIndex"]) <= startIndex).ToList();
                temp2 = temp2.OrderByDescending(m => int.Parse(m.Parameters["startIndex"])).ToList();

                MultiOpenSearchRequestState state = temp2.FirstOrDefault();

                // If not, useless and create new one with the entity pagination parameter unset (=1)
                if (state.Entities == null) {
                    foreach (IOpenSearchable entity in entities) {
                        if (entity is IProxiedOpenSearchable) {
                            entities2.Add(entity, ((IProxiedOpenSearchable)entity).GetProxyOpenSearchDescription().Url.FirstOrDefault(p => p.Type == type).IndexOffset);
                        } else {
                            entities2.Add(entity, entity.GetOpenSearchDescription().Url.FirstOrDefault(p => p.Type == type).IndexOffset);
                        }
                    }
                    parameters2 = RemovePaginationParameters(parameters);
                    return false;
                }

                //Set the OpenSearch params to the closest pagination params found
                foreach (IOpenSearchable entity in state.Entities.Keys) {
                    entities2.Add(entity, state.Entities[entity]);
                }


                parameters2 = new NameValueCollection(state.Parameters);

                return true;

            } finally {
                _m.ReleaseMutex();

            }

            return false;

        }

        void SetCurrentEntitiesOffset() {

            try {

                _m.WaitOne();

                var it = currentEntities.Keys.ToArray();

                foreach (IOpenSearchable entity in it) {

                    // the offset for this entity will be the number of items taken from its current result.
                    int offset = ((TFeed)results[entity]).Items.Cast<TItem>().Intersect(feed.Items.Cast<TItem>(), new OpenSearchResultItemComparer()).Count();

                    // Add this offset to the current state for this entity
                    currentEntities[entity] += offset;

                    log.DebugFormat("{3} [multi] : OS {0} offset + {1} = {2}", entity.Identifier, offset, currentEntities[entity], this.OpenSearchUrl);

                }
            } finally {
                _m.ReleaseMutex();
            }
        }

        /// <summary>
        /// Caches the state of the current request with the subentities offset;
        /// </summary>
        void CacheCurrentState() {

            try {
                _m.WaitOne();

                MultiOpenSearchRequestState state = new MultiOpenSearchRequestState();
                state.Entities = new Dictionary<IOpenSearchable, int>(currentEntities);
                state.Parameters = new NameValueCollection(currentParameters);
                state.Type = type;



                requestStatesCache.Add(state);
            } finally {
                _m.ReleaseMutex();
            }

        }

        static NameValueCollection RemovePaginationParameters(NameValueCollection parameters) {
            NameValueCollection nvc = new NameValueCollection(parameters);
            nvc.Remove(OpenSearchFactory.ReverseTemplateOpenSearchParameters(OpenSearchFactory.GetBaseOpenSearchParameter())["startPage"]);
            nvc.Remove(OpenSearchFactory.ReverseTemplateOpenSearchParameters(OpenSearchFactory.GetBaseOpenSearchParameter())["startIndex"]);
            return nvc;
        }

        /// Structure to hold a request state
        struct MultiOpenSearchRequestState {
            public Dictionary<IOpenSearchable, int> Entities;
            public NameValueCollection Parameters;
            public string Type;
        }
    }
}

