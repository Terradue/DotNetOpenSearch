using System;

namespace Terradue.OpenSearch {

    /// <summary>
    /// Represents a factory able to create IOpenSearchable object
    /// </summary>
    public interface IOpenSearchableFactory {

        /// <summary>
        /// Create an IOpenSearchable from an OpenSearchUrl
        /// </summary>
        /// <param name="url">URL to either a search or a description</param>
        IOpenSearchable Create(OpenSearchUrl url);

        /// <summary>
        /// Create an IOpenSearchable from an OpenSearchDescription
        /// </summary>
        /// <param name="osd">OPenSearchDescription</param>
        IOpenSearchable Create(OpenSearchDescription osd);

    }
}

