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
using System.Runtime.Caching;

namespace Terradue.OpenSearch.Request {
    /// <summary>
    /// An OpenSearchRequest able to request multiple IOpenSearchable as a unique one.
    /// </summary>
    /// <description>
    /// This request will return an atom response and thus the entities requested must be able to return 
    /// Opensearch Collection response.
    /// </description>
    public class MultiOpenSearchRequest<TFeed, TItem> : OpenSearchRequest where TFeed: class, IOpenSearchResultCollection, ICloneable, new() where TItem: class, IOpenSearchResultItem {
        static MemoryCache requestStatesCache = new MemoryCache("mosr-cache");
        string type;
		NameValueCollection targetParameters, entitiesParameters, currentParameters;
        OpenSearchableFactorySettings settings;
        CountdownEvent countdown;
        Dictionary<IOpenSearchable, int> currentEntities;
        Dictionary<IOpenSearchable,IOpenSearchResultCollection> results;
        TFeed feed;
        bool usingCache = false;
		long totalResults = 0;
		long currentMergedResults = 0;

        bool concurrent = true;

        IOpenSearchable parent;

        Mutex _m;
        static Mutex _m2 = new Mutex();

        private log4net.ILog log = log4net.LogManager.GetLogger
            (System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Initializes a new instance of the <see cref="Terradue.OpenSearch.Request.MultiOpenSearchRequest"/> class.
        /// </summary>
        /// <param name="ose">Instance of OpenSearchEngine, preferably with the AtomOpenSearchEngineExtension registered</param>
        /// <param name="entities">IOpenSearchable entities to be searched.</param>
        /// <param name="type">contentType of the .</param>
        /// <param name="url">URL.</param>
        public MultiOpenSearchRequest(OpenSearchableFactorySettings settings, IOpenSearchable[] entities, string type, OpenSearchUrl url, bool concurrent, IOpenSearchable parent) : base(url, type) {
            this.parent = parent;
            this.concurrent = concurrent;

            this.settings = settings;
            this.type = type;
            this.targetParameters = HttpUtility.ParseQueryString(url.Query);
            this.entitiesParameters = RemovePaginationParameters(this.targetParameters);

            _m = new Mutex();

            try {
                _m2.WaitOne();
                // Ask the cache if a previous page request is present to save some requests
                usingCache = GetClosestState(entities.Distinct(new OpenSearchableComparer(settings.OpenSearchEngine)).ToArray(), type, this.targetParameters, out this.currentEntities, out this.currentParameters);
            } finally {
                _m2.ReleaseMutex();
            }



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
                return targetParameters;
            }
            set {
                targetParameters = value;
            }
        }

        #endregion

        /// <summary>
        /// Requests the current page.
        /// </summary>
        private void RequestCurrentPage() {

            Stopwatch sw = Stopwatch.StartNew();

            bool emptySources = false;
            int count = settings.OpenSearchEngine.DefaultCount;

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

			totalResults = 0;
			currentMergedResults = 0;

			log.DebugFormat("[multi] [target] CSI:{0} TSI:{1} CSP:{2} TSP:{3}", currentStartIndex, targetStartIndex, currentStartPage, targetStartPage);

			/// While we do not have the count needed to reach the start index, we query
			while (currentStartIndex < targetStartIndex &&  emptySources == false) {

                feed = new TFeed();

				log.DebugFormat("[multi] [startIndex] TR:{0} CMR:{1} CSI:{2} TSI:{3}", totalResults, currentMergedResults, currentStartIndex, targetStartIndex);
				log.DebugFormat("[multi] [startIndex] Query {0}", count);
                
                ExecuteConcurrentRequest();

                MergeResults();

				log.DebugFormat("[multi] [startIndex] Got merged {0}", feed.Items.Count());

				feed.Items = feed.Items.Take(targetStartIndex - currentStartIndex);

				log.DebugFormat("[multi] [startIndex] Keep {0}", feed.Items.Count());

				currentMergedResults += feed.Items.Count();

                SetCurrentEntitiesOffset();

                var r1 = results.Values.FirstOrDefault(r => {
                    TFeed result = (TFeed)r;
                    if (result.Items.Count() > 0)
                        return true;
                    return false;
                });

                currentStartIndex += feed.Items.Count();

				emptySources = (r1 == null || totalResults <= currentStartIndex - 1 + (count * (currentStartPage - 1)));

				currentParameters[OpenSearchFactory.ReverseTemplateOpenSearchParameters(OpenSearchFactory.GetBaseOpenSearchParameter())["startIndex"]] = currentStartIndex.ToString();

                CacheCurrentState();

				log.DebugFormat("[multi] [startIndex] TR:{0} CMR:{1} CSI:{2} TSI:{3}", totalResults, currentMergedResults, currentStartIndex, targetStartIndex);

			}

			while (currentStartPage <= targetStartPage) {

                // new page -> new feed
                feed = new TFeed();

                // count=0 case for totalResults
                if (count == 0) {
                    ExecuteConcurrentRequest();
                    MergeResults();
                    feed.Items = new List<TItem>();
                }


                // While we do not have the count needed for our results
                // and that all the sources are not empty
				while (feed.Items.Count() < count && emptySources == false) {

                    log.DebugFormat("[multi] [startPage] TR:{0} CSP:{1} TSP:{2}", totalResults, currentStartPage, targetStartPage);
					log.DebugFormat("[multi] [startPage] Query {0}", count);

                    ExecuteConcurrentRequest();

                    MergeResults();

					log.DebugFormat("[multi] [startPage] Got merged {0}", feed.Items.Count());

                    SetCurrentEntitiesOffset();

                    var r1 = results.Values.FirstOrDefault(r => {
                        TFeed result = (TFeed)r;
                        if (result.Items.Count() > 0)
                            return true;
                        return false;
                    });

					emptySources = (r1 == null || totalResults <= currentStartIndex -1 + ( count * (currentStartPage) ) );

					log.DebugFormat("[multi] [startPage] TR:{0} CSP:{1} TSP:{2}", totalResults, currentStartPage, targetStartPage);

				}

                // next page
                currentStartPage++;
                currentParameters[OpenSearchFactory.ReverseTemplateOpenSearchParameters(OpenSearchFactory.GetBaseOpenSearchParameter())["startPage"]] = currentStartPage.ToString();
				if (!emptySources) {
					try {
						_m2.WaitOne();
						CacheCurrentState();
					} finally {
						_m2.ReleaseMutex();
					}
				}

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

            results = new Dictionary<IOpenSearchable, IOpenSearchResultCollection>();

            Parallel.ForEach<IOpenSearchable>(currentEntities.Keys.Distinct(new OpenSearchableComparer(settings.OpenSearchEngine)),
                                              entity => {
                ExecuteOneRequest(entity);
            });

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

				log.DebugFormat("[multi] [{0}] Query SI:{1}", ((IOpenSearchable)entity).Identifier, offset);

                IOpenSearchResultCollection result = settings.OpenSearchEngine.Query((IOpenSearchable)entity, entityParameters, typeof(TFeed));

                try {
                    _m.WaitOne();
                    results.Add((IOpenSearchable)entity, result);
                } finally {
                    _m.ReleaseMutex();

                }
            } catch (Exception ex) {
                TFeed result = new TFeed();
                try {
                    _m.WaitOne();
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
                } finally {
                    _m.ReleaseMutex();

                }
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

				log.DebugFormat("[multi] [{1}] Got {0}", f1.Items.Count(), f1.Identifier);

                totalResults += f1.TotalResults;

				log.DebugFormat("[multi] [{1}] TR:{0}", f1.TotalResults, f1.Identifier);

                //if (f1.Items.Count() == 0)
                    //continue;
                feed = Merge(feed, f1, key);

            }
        }

        /// <summary>
        /// Merge SyndicationFeed
        /// </summary>
        /// <param name="f1">F1.</param>
        /// <param name="f2">F2.</param>
        TFeed Merge(TFeed f1, TFeed f2, IOpenSearchable os) {

            TFeed tfeed = (TFeed)f1.Clone();

            int originalCount;
            try {
                originalCount = int.Parse(targetParameters["count"]);
            } catch (Exception e) {
                originalCount = settings.OpenSearchEngine.DefaultCount;
            }

            // Case if all elements in first feed are already the right sorted results
            if (tfeed.Items.Count() >= originalCount) {
                if (f1.Items.Count() != 0 && f2.Items.Count() != 0) {
                    if (f1.Items.Last().LastUpdatedTime >= f2.Items.First().LastUpdatedTime)
                        return tfeed;
                }
            }

            int common = f1.Items.Intersect(f2.Items, new OpenSearchResultItemComparer()).OrderBy(u => u.Id).OrderByDescending(u => u.SortKey).Count();
            tfeed.Items = f1.Items.Union(f2.Items, new OpenSearchResultItemComparer()).OrderBy(u => u.Id).OrderByDescending(u => u.SortKey);
            totalResults -= common;

            tfeed.Items = tfeed.Items.Take(originalCount);
            foreach (var ext in f2.ElementExtensions.Where(e => e.OuterNamespace != "http://a9.com/-/spec/opensearch/1.1/"))
            {
                tfeed.ElementExtensions.Add(ext);
            }
                
            return (TFeed)tfeed.Clone();
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
        public static bool GetClosestState(IOpenSearchable[] entities, string type, NameValueCollection parameters, out Dictionary<IOpenSearchable, int> cachedEntities, out NameValueCollection cachedParameters) {

			log4net.ILog logs = log4net.LogManager.GetLogger
			(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

            cachedEntities = new Dictionary<IOpenSearchable, int>(entities.Length);
            cachedParameters = RemovePaginationParameters(parameters);
            KeyValuePair<NameValueCollection, Dictionary<IOpenSearchable, int>> cachedPageParameters = new KeyValuePair<NameValueCollection, Dictionary<IOpenSearchable, int>>(null, null);

            // Find the request states with the same combination of opensearchable items
            MultiOpenSearchRequestState state = (MultiOpenSearchRequestState)requestStatesCache.Get(MultiOpenSearchRequestState.GetHashCode(entities).ToString());

			if (state != null) {
				cachedPageParameters = state.GetClosestPage(parameters);

			}

            // If not, useless and create new one with the entity pagination parameter unset (=1)
            if (state == null || cachedPageParameters.Key == null) {
                foreach (IOpenSearchable entity in entities) {
                    if (entity is IProxiedOpenSearchable) {
                        cachedEntities.Add(entity, ((IProxiedOpenSearchable)entity).GetProxyOpenSearchDescription().Url.FirstOrDefault(p => p.Type == type).IndexOffset);
                    } else {
                        cachedEntities.Add(entity, entity.GetOpenSearchDescription().Url.FirstOrDefault(p => p.Type == type).IndexOffset);
                    }
                }
                cachedParameters = RemovePaginationParameters(parameters);
                return false;
            }

			logs.DebugFormat("[multi] [cache {0}] {1} found: {2} ", state.GetHashCode(), cachedPageParameters.Key.Join(e => String.Format("\"{0}:{1}\"", e, cachedPageParameters.Key[e]), ","), cachedPageParameters.Value.Print(e => String.Format("\"{0}:{1}\"", e.Identifier, cachedPageParameters.Value[e]), ","));

			cachedEntities = new Dictionary<IOpenSearchable, int>(cachedPageParameters.Value);
            cachedParameters = new NameValueCollection(cachedPageParameters.Key);
            return true;

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

					log.DebugFormat("[multi] [{0}] new offset: +{1} (taken) = {2}", entity.Identifier, offset, currentEntities[entity]);

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
                _m2.WaitOne();
                // Is there a cache entry for this entity ?
                MultiOpenSearchRequestState state = (MultiOpenSearchRequestState)requestStatesCache.Get(MultiOpenSearchRequestState.GetHashCode(currentEntities.Keys).ToString());

                // no, create it!
                if (state == null) {
                    state = new MultiOpenSearchRequestState(currentEntities.Keys);
                }

                // Let's put the new page state
                state.SetState(currentParameters, currentEntities, state.GetHashCode());

                // finally, we cache the state
                requestStatesCache.Set(state.GetHashCode().ToString(), state, new CacheItemPolicy(){ AbsoluteExpiration = DateTime.Now.AddMinutes(15) });
            } finally {
                _m.ReleaseMutex();
                _m2.ReleaseMutex();
            }
        }

        static NameValueCollection RemovePaginationParameters(NameValueCollection parameters) {
            NameValueCollection nvc = new NameValueCollection(parameters);
            nvc.Remove(OpenSearchFactory.ReverseTemplateOpenSearchParameters(OpenSearchFactory.GetBaseOpenSearchParameter())["startPage"]);
            nvc.Remove(OpenSearchFactory.ReverseTemplateOpenSearchParameters(OpenSearchFactory.GetBaseOpenSearchParameter())["startIndex"]);
            return nvc;
        }

        /// Structure to hold a request state
        class MultiOpenSearchRequestState {
            
            public ICollection<IOpenSearchable> Entities;

			private log4net.ILog log = log4net.LogManager.GetLogger
		   (System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

            public MultiOpenSearchRequestState(ICollection<IOpenSearchable> entities) {
                this.Entities = new List<IOpenSearchable>(entities);
            }

            private Dictionary<NameValueCollection, Dictionary<IOpenSearchable, int>> states = new Dictionary<NameValueCollection, Dictionary<IOpenSearchable, int>>(new NameValueCollectionEqualityComparer());

            public static int GetHashCode(ICollection<IOpenSearchable> entities) {

                int hc = 0;
                foreach (var e in entities) {
                    hc ^= e.GetHashCode();
                    hc = (hc << 7) | (hc >> (32 - 7)); //rotale hc to the left to swipe over all bits
                }
                return hc;

            }

            new public int GetHashCode() {

                return MultiOpenSearchRequestState.GetHashCode(Entities);

            }

            public void SetState(NameValueCollection nvc, Dictionary<IOpenSearchable, int> startIndexes, int hash) {
				if (states.ContainsKey(nvc)) {
					log.DebugFormat("[multi] [cache {0}] {1} remove: {2} ", hash, nvc.Join(e => String.Format("\"{0}:{1}\"", e, nvc[e]), ","), startIndexes.Print(e => String.Format("\"{0}:{1}\"", e.Identifier, startIndexes[e]), ","));
					states.Remove(nvc);
				}
				log.DebugFormat("[multi] [cache {0}] {1} add: {2} ", hash, nvc.Join(e => String.Format("\"{0}:{1}\"", e, nvc[e]), ","), startIndexes.Print(e => String.Format("\"{0}:{1}\"", e.Identifier, startIndexes[e]), ","));
				states.Add(new NameValueCollection(nvc), new Dictionary<IOpenSearchable, int>(startIndexes));

			}

            public KeyValuePair<NameValueCollection, Dictionary<IOpenSearchable, int>>  GetClosestPage(NameValueCollection parameters) {

                // and with the same opensearch parameters
                var keys = states.Keys.Where(s => OpenSearchFactory.PaginationFreeEqual(s, parameters) == true).ToArray();
                Dictionary<NameValueCollection, Dictionary<IOpenSearchable, int>> tempstate = new Dictionary<NameValueCollection, Dictionary<IOpenSearchable, int>>(states);

                // if no such state, create new one with the entity pagination parameter unset (=1)
                if (keys.Count() <= 0) {
                    return new KeyValuePair<NameValueCollection, Dictionary<IOpenSearchable, int>>(null, null);
                }

                // if found, what was the startPage parameter requested (assume 1)
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
                var tempkeys1 = keys.Where(m => int.Parse(m["startPage"]) <= targetStartPage).ToList();
                tempkeys1 = tempkeys1.OrderByDescending(m => int.Parse(m["startPage"])).ToList();

                var tempkeys2 = tempkeys1.Where(m => int.Parse(m["startIndex"]) <= targetStartIndex).ToList();
                tempkeys2 = tempkeys2.OrderByDescending(m => int.Parse(m["startIndex"])).ToList();

                var key = tempkeys2.FirstOrDefault();

                // If not, useless and create new one with the entity pagination parameter unset (=1)
                if (key == null) {
                    return new KeyValuePair<NameValueCollection, Dictionary<IOpenSearchable, int>>(null, null);
                }



				return new KeyValuePair<NameValueCollection, Dictionary<IOpenSearchable, int>>(key, tempstate[key]);
            }


        }
    }
}

