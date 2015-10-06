using System;
using System.Collections.Generic;

namespace Terradue.OpenSearch.Result {
    public class OpenSearchResultItemComparer : IEqualityComparer<IOpenSearchResultItem> {
        #region IEqualityComparer implementation

        public bool Equals(IOpenSearchResultItem x, IOpenSearchResultItem y) {
            if (x.Identifier == y.Identifier) return true;
            else return false;
        }

        public int GetHashCode(IOpenSearchResultItem obj) {
            return obj.Identifier.GetHashCode();
        }

        #endregion
    }
}

