//
//  OpenSearchCorrelationFilter.cs
//
//  Author:
//       Emmanuel Mathot <emmanuel.mathot@terradue.com>
//
//  Copyright (c) 2014 Terradue

using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Specialized;
using Terradue.OpenSearch.Engine;
using Terradue.OpenSearch.Response;
using Terradue.OpenSearch.Result;
using Terradue.OpenSearch.Schema;

namespace Terradue.OpenSearch.Filters {

    /// <summary>
    /// OpenSearch correlation filter.
    /// </summary>
    public abstract partial class OpenSearchCorrelationFilter {

        protected OpenSearchEngine ose;
        protected IOpenSearchableFactory factory;

        /// <summary>
        /// Initializes a new instance of the <see cref="Terradue.OpenSearch.Filters.OpenSearchCorrelationFilter"/> class.
        /// </summary>
        /// <param name="ose">Ose.</param>
        /// <param name="factory">Factory.</param>
        public OpenSearchCorrelationFilter(OpenSearchEngine ose, IOpenSearchableFactory factory = null) {
            this.ose = ose;
            if (factory != null) {
                this.factory = factory;
            } else {
                this.factory = ose;
            }
        }

        /// <summary>
        /// Applies the result filters.
        /// </summary>
        /// <param name="osr">Osr.</param>
        /// <param name="originalParameters">Original parameters.</param>
        public abstract void ApplyResultFilters(ref IOpenSearchResultCollection osr, NameValueCollection originalParameters, IOpenSearchable entity);

        public abstract void AddSearchLink(ref IOpenSearchResultCollection osr, NameValueCollection originalParameters, IOpenSearchable entity, string with, string finalContentType);

        /// <summary>
        /// Gets the correlated URL.
        /// </summary>
        /// <returns>The correlated URL.</returns>
        /// <param name="searchParameters">Search parameters.</param>
        public static OpenSearchUrl GetCorrelatedUrl (NameValueCollection searchParameters){

            string corWith = searchParameters["correlatedTo"];
            if (string.IsNullOrEmpty(corWith)) return null;

            try {
                return new OpenSearchUrl(corWith);
            }
            catch (UriFormatException){
                throw new FormatException(string.Format("Wrong format for param cor:with={0}. must be an URL", searchParameters["correlatedTo"]));
            }

        }

        /// <summary>
        /// Gets the correlation function.
        /// </summary>
        /// <returns>The function.</returns>
        /// <param name="searchParameters">Search parameters.</param>
        public static string GetFunction (NameValueCollection searchParameters){

            return searchParameters["corFunction"];
          

        }

        /// <summary>
        /// Gets the minimum correlation parameter.
        /// </summary>
        /// <returns>The minimum.</returns>
        /// <param name="searchParameters">Search parameters.</param>
        public static int GetMinimum (NameValueCollection searchParameters){

            string value = searchParameters["minimumCor"];
            try {
                return int.Parse(value);
            } catch (Exception){
                return 1;
            }

        }

        /// <summary>
        /// Gets the minimum correlation parameter.
        /// </summary>
        /// <returns>The minimum.</returns>
        /// <param name="searchParameters">Search parameters.</param>
        public static int GetSpatialCover (NameValueCollection searchParameters){

            string value = searchParameters["spatialCover"];
            try {
                int sc = int.Parse(value);
                if ( sc < 0 || sc > 100 )
                    throw new FormatException(string.Format("Wrong format for param cor:spatial={0}. must be an integer betwenn 0 and 100", value));
                return sc;
            } catch (Exception){
                throw new FormatException(string.Format("Wrong format for param cor:spatial={0}. must be an integer", value));
            }

        }

        public DateTime[] GetTimeCoverage (NameValueCollection searchParameters, IOpenSearchResultItem item){

            string value = searchParameters["timeCover"];
            if (string.IsNullOrEmpty(value)) {
                return null;
            }
            try {
                DateTime[] itemDates = GetStartAndStopTime(item);
                var time = value.Split(',');
                if ( time.Length != 2 ){
                    throw new FormatException(string.Format("Wrong format for param cor:time={0}. must be 2 timespan separated by ,", value));
                }
                TimeSpan[] timeSpans = Array.ConvertAll<string, TimeSpan>(time, t => TimeSpan.Parse(t));
                return new DateTime[]{itemDates[0].Add(timeSpans[0]), itemDates[1].Add(timeSpans[1])};
            } catch (Exception){
                throw new FormatException(string.Format("Wrong format for param cor:time={0}. must be 2 timespan separated by ,", value));
            }

        }

        /// <summary>
        /// Gets the correlation open search parameters.
        /// </summary>
        /// <returns>The correlation open search parameters.</returns>
        public static UniqueValueDictionary<string,string> GetCorrelationOpenSearchParameters (){
            UniqueValueDictionary<string,string> osdic = new UniqueValueDictionary<string, string>();

            osdic.Add("correlatedTo", "{cor:with?}" );
            osdic.Add("timeCover", "{cor:time?}" );
            osdic.Add("spatialCover", "{cor:spatial?}");
            osdic.Add("minimumCor", "{cor:minimum?}" );
            osdic.Add("corFunction", "{cor:function?}");
            osdic.Add("corParams", "{cor:parameters?}" );

            return osdic;
        }

        /// <summary>
        /// Determines if is correlation search the specified parameters.
        /// </summary>
        /// <returns><c>true</c> if is correlation search the specified parameters; otherwise, <c>false</c>.</returns>
        /// <param name="parameters">Parameters.</param>
        public static bool IsCorrelationSearch(NameValueCollection parameters) {
            return (GetCorrelatedUrl(parameters) != null);
        }

        /// <summary>
        /// Gets the correlation free parameters.
        /// </summary>
        /// <returns>The correlation free parameters.</returns>
        /// <param name="parameters">Parameters.</param>
        public static NameValueCollection GetCorrelationFreeParameters(NameValueCollection parameters) {
            NameValueCollection nvc = new NameValueCollection(parameters);
            nvc.Remove("correlatedTo");
            // TODO remove all cor param.
            return nvc;
        }

        protected abstract DateTime[] GetStartAndStopTime(IOpenSearchResultItem item);
    }

}

