using System;
using Terradue.OpenSearch.Result;
using System.Collections.Specialized;

namespace Terradue.OpenSearch {
    public interface IAtomizable {

        AtomItem ToAtomItem(NameValueCollection parameters);

    }
}

