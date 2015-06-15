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

        /// <summary>
        /// Gets the correlated URL.
        /// </summary>
        /// <returns>The correlated URL.</returns>
        /// <param name="searchParameters">Search parameters.</param>
        public static OpenSearchUrl GetCorrelatedUrl (NameValueCollection searchParameters){

            string corWith = searchParameters["correlatedTo"];
            if (corWith == null) return null;

            OpenSearchUrl urlCor = new OpenSearchUrl(corWith);

            return urlCor;

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
    }

}

