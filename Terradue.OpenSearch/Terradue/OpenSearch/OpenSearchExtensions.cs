using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace Terradue.OpenSearch
{
	public static class OpenSearchExtensions
	{
		public static string Join(this NameValueCollection collection, Func<string, string> selector, string separator)
		{
			return String.Join(separator, collection.Cast<string>().Select(e => selector(e)));
		}

		public static string Print(this Dictionary<IOpenSearchable, int> dic, Func<IOpenSearchable, string> selector, string separator)
		{
			return String.Join(separator, dic.Keys.Cast<IOpenSearchable>().Select(e => selector(e)));
		}

	}
}

