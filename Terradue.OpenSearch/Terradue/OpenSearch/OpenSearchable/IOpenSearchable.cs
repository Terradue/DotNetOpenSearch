//
//  IOpenSearchable.cs
//
//  Author:
//       Emmanuel Mathot <emmanuel.mathot@terradue.com>
//
//  Copyright (c) 2014 Terradue
//

using System;
using System.Collections.Specialized;
using System.Collections.Generic;
using Terradue.OpenSearch.Request;
using Terradue.OpenSearch.Engine;
using Terradue.OpenSearch.Response;
using Terradue.OpenSearch.Result;
using Terradue.OpenSearch.Schema;

/*!

\defgroup OpenSearch OpenSearch
@{
This is the internal interface for any real world entity to be processed in OpenSearch Engine

Combined with the OpenSearch extensions, the search interface offers many format to export results.

\xrefitem cptype_int "Interfaces" "Interfaces"

\xrefitem norm "Normative References" "Normative References" [OpenSearch 1.1](http://www.opensearch.org/Specifications/OpenSearch/1.1)

@}

*/

namespace Terradue.OpenSearch {

    /// <summary>Delegate type for generating a specific OpenSearchResult from an OpenSearch response.</summary>
    /// <remarks>Instances of this delegate are used to transform an OpenSearchResponse into a an IOpenSearchResultCollection, which correspond to an output format.</remarks>
    /// <param name="osr">The OpenSearch response object to be transformed.</param>
    /// <returns>The result collection, i.e, an object that can be serialized to the desired output format.</returns> 
    public delegate IOpenSearchResultCollection ReadNativeFunction(IOpenSearchResponse osr);

    /// <summary>Helper class that encapsulates the settings for an OpenSearch query and its result generation.</summary>
    /// <remarks>Instances of this object are returned by classes implementing IOpenSearchable. It is used by OpenSearch engines to control the query process from the OpenSearch request to the initial result generation.</remarks>
    public class QuerySettings {

        /// <summary>Gets or sets the preferred content type.</summary>
        public string PreferredContentType { get; set; }

        /// <summary>Gets or sets the function that returns the in the OpenSearch result in the format preferred by the IOpenSearchable using these QuerySettings.</summary>
        public ReadNativeFunction ReadNative { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the query shall force anyway the parameters that are not declared in the 
        /// OpenSearch Description of the entity.
        /// </summary>
        /// <value><c>true</c> to force unassigned parameters; otherwise, <c>false</c>.</value>
        public bool ForceUnspecifiedParameters { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the query shall not propagate null or empty parameters of the query string part of the url. 
        /// </summary>
        /// <value><c>true</c> if skip null or empty query string parameters; otherwise, <c>false</c>.</value>
        public bool SkipNullOrEmptyQueryStringParameters { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="T:Terradue.OpenSearch.QuerySettings"/> request is for opensearch ur only
        /// URL only.
        /// </summary>
        /// <value><c>true</c> if open search URL only; otherwise, <c>false</c>.</value>
        /// TODO to be removed for a more legant solution
        public bool OpenSearchUrlOnly { get; set; }

        /// <summary>Creates a new instance of QuerySettings with the specified parameters.</summary>
        /// <param name="preferredContentType">The preferred content type.</param>
        /// <param name="readNative">The function to be called to obtain the formatted OpenSearch result.</param>
        public QuerySettings(string preferredContentType, ReadNativeFunction readNative) {
            this.PreferredContentType = preferredContentType;
            this.ReadNative = readNative;
            this.ForceUnspecifiedParameters = false;
            this.SkipNullOrEmptyQueryStringParameters = false;
        }

    }

    /// <summary>
    /// IOpenSearchable.
    /// </summary>
    /// <description>Internal interface that enables OpenSearch mechanism on its class</description>
    public partial interface IOpenSearchable {

        /// Get the transform function according to the entity. OpenSearch is pasFunc<OpenSearchResponse, IOpenSearchResultCollection> /// to be used with the default functions in OpenSearchEngine and OpenSearchFactory
        /// </summary>
        /// <seealso cref="OpenSearchEngine.GetExtension"/>
        /// <seealso cref="OpenSearchFactory.GetBestQuerySettingsByNumberOfParam"/> 
        /// <returns>A tuple with the transform function and the mime-type that will be read as input</returns>
        /// <param name="ose">OpenSearchEngine instance</param>
        QuerySettings GetQuerySettings(OpenSearchEngine ose);

        /// <summary>
        /// Create the OpenSearch Request for the requested mime-type the specified type and parameters.
        /// </summary>
        /// <param name="mimetype">Mime-Type requested to the OpenSearchable entity</param>
        /// <param name="parameters">Parameters of the request</param>
        OpenSearchRequest Create(QuerySettings querySettings, NameValueCollection parameters);

        /// <summary>
        /// Get the local identifier.
        /// </summary>
        /// <value>The local identifier of the OpenSearchable entity.</value>
        string Identifier { get; }

        /// <summary>
        /// Get the entity's OpenSearchDescription.
        /// </summary>
        /// <returns>The OpenSearchDescription describing the IOpenSearchable.</returns>
        OpenSearchDescription GetOpenSearchDescription();

        /// <summary>
        /// Gets the OpenSearch parameters for a given Mime-Type.
        /// </summary>
        /// <returns>OpenSearch parameters NameValueCollection.</returns>
        /// <param name="mimeType">MIME type for the requested parameters</param>
        NameValueCollection GetOpenSearchParameters(string mimeType);

        /// <summary>
        /// Get the total of possible results for the OpenSearchable entity
        /// </summary>
        /// <returns>a unsigned long number representing the number of items searchable</returns>
        long TotalResults { get; }

        /// <summary>
        /// Optional function that apply to the result after the search and before the result is returned by OpenSearchEngine.
        /// </summary>
        /// <param name="osr">IOpenSearchResult cotnaing the result of the a search</param>
        void ApplyResultFilters(OpenSearchRequest request, ref IOpenSearchResultCollection osr, string finalContentType);

        /// <summary>
        /// Gets the default MIME-type that the entity can be searched for
        /// </summary>
        /// <value>The default MIME-type.</value>
        string DefaultMimeType { get; }

        bool CanCache { get; }

    }

    /// <summary>
    /// OpenSearchable comparer.
    /// </summary>
    public class OpenSearchableComparer : IEqualityComparer<IOpenSearchable> {
        readonly OpenSearchEngine ose;

        public OpenSearchableComparer(OpenSearchEngine ose)
        {
            this.ose = ose;
        }

        #region IEqualityComparer implementation

        public bool Equals(IOpenSearchable x, IOpenSearchable y) {
            var osrx = x.Create(x.GetQuerySettings(ose), new NameValueCollection());
            var osry = y.Create(y.GetQuerySettings(ose), new NameValueCollection());
            return osrx.OpenSearchUrl.Equals(osry.OpenSearchUrl);
        }

        public int GetHashCode(IOpenSearchable obj) {
            QuerySettings querySettings = obj.GetQuerySettings(ose);
            
            // TODO to be removed with a more elegant soltuion
            if(querySettings != null) querySettings.OpenSearchUrlOnly = true;
            
            var osrobj = obj.Create(querySettings, new NameValueCollection());
            return osrobj.OpenSearchUrl.GetHashCode();
        }

        #endregion
    }
}

