//
//  IOpenSearchable.cs
//
//  Author:
//       Emmanuel Mathot <emmanuel.mathot@terradue.com>
//
//  Copyright (c) 2014 Terradue
//

using System.Collections.Generic;
using System.Collections.Specialized;
using Terradue.OpenSearch.Engine;
using Terradue.OpenSearch.Request;
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

namespace Terradue.OpenSearch
{

    /// <summary>Delegate type for generating a specific OpenSearchResult from an OpenSearch response.</summary>
    /// <remarks>Instances of this delegate are used to transform an OpenSearchResponse into a an IOpenSearchResultCollection, which correspond to an output format.</remarks>
    /// <param name="osr">The OpenSearch response object to be transformed.</param>
    /// <returns>The result collection, i.e, an object that can be serialized to the desired output format.</returns> 
    public delegate IOpenSearchResultCollection ReadNativeFunction(IOpenSearchResponse osr);

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

