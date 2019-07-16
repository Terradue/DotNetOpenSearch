//
//  IOpenSearchable.cs
//
//  Author:
//       Emmanuel Mathot <emmanuel.mathot@terradue.com>
//
//  Copyright (c) 2014 Terradue
//


/*!

\defgroup OpenSearch OpenSearch
@{
This is the internal interface for any real world entity to be processed in OpenSearch Engine

Combined with the OpenSearch extensions, the search interface offers many format to export results.

\xrefitem cptype_int "Interfaces" "Interfaces"

\xrefitem norm "Normative References" "Normative References" [OpenSearch 1.1](http://www.opensearch.org/Specifications/OpenSearch/1.1)

@}

*/

using System.Collections.Generic;

namespace Terradue.OpenSearch
{
    /// <summary>Helper class that encapsulates the settings for an OpenSearch query and its result generation.</summary>
    /// <remarks>Instances of this object are returned by classes implementing IOpenSearchable. It is used by OpenSearch engines to control the query process from the OpenSearch request to the initial result generation.</remarks>
    public class QuerySettings
    {

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

        public System.Net.ICredentials Credentials { get; set; }

        public bool ReportMetrics { get; set; }

        public int MaxRetries { get; set; }

        public bool SkipCertificateVerification { get; set; }

        public Dictionary<string, string> ParametersKeywordsTable { get; set; }

        /// <summary>Creates a new instance of QuerySettings with the specified parameters.</summary>
        /// <param name="preferredContentType">The preferred content type.</param>
        /// <param name="readNative">The function to be called to obtain the formatted OpenSearch result.</param>
        public QuerySettings(string preferredContentType, ReadNativeFunction readNative)
        {
            this.PreferredContentType = preferredContentType;
            this.ReadNative = readNative;
            this.ForceUnspecifiedParameters = false;
            this.SkipNullOrEmptyQueryStringParameters = false;
            this.ReportMetrics = false;
            this.SkipCertificateVerification = false;
            this.ParametersKeywordsTable = OpenSearchFactory.GetBaseOpenSearchParametersKeywordsTable();
        }

        /// <summary>Creates a new instance of QuerySettings with the specified parameters.</summary>
        /// <param name="preferredContentType">The preferred content type.</param>
        /// <param name="readNative">The function to be called to obtain the formatted OpenSearch result.</param>
        public QuerySettings(string preferredContentType, ReadNativeFunction readNative, OpenSearchableFactorySettings settings) : this(preferredContentType, readNative)
        {
            if (settings != null)
            {
                this.Credentials = settings.Credentials;
                this.MaxRetries = settings.MaxRetries;
                this.ReportMetrics = settings.ReportMetrics;
                this.SkipCertificateVerification = settings.SkipCertificateVerification;
                this.ParametersKeywordsTable = settings.ParametersKeywordsTable;
            }
        }

    }
}

