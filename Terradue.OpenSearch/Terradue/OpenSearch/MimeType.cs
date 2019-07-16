using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Terradue.OpenSearch
{
	public class MimeType
	{
		string contentType;

		public string ContentType
		{
			get
			{
				return contentType;
			}
		}

		Dictionary<string, string> parameters;

		public MimeType(string contentType)
		{
			this.contentType = contentType;
			parameters = new Dictionary<string, string>();
		}

		public static MimeType CreateFromContentType(string type)
		{
			if (type.Contains(";")) {
				MimeType mimeType = new MimeType(type.Split(';')[0].Trim());
				foreach (var param in type.Split(';').Skip(1)) {

					if (param.Contains("=")) {
						if (param.Split('=')[0].Trim() == "charset") continue;
						mimeType.parameters.Add(param.Split('=')[0].Trim(), param.Split('=')[1].Trim());

					}

				}
				return mimeType;
			} else {
				return new MimeType(type);
			}
		}

		public new static bool Equals(object x, object y)
		{
			return ((MimeType)x).contentType == ((MimeType)y).contentType && ((MimeType)x).parameters.DictionaryEqual(((MimeType)y).parameters);
		}


	}

	public static class Extensions
	{
		public static bool DictionaryEqual<TKey, TValue>(
	this IDictionary<TKey, TValue> first, IDictionary<TKey, TValue> second)
		{
			return first.DictionaryEqual(second, null);
		}

		public static bool DictionaryEqual<TKey, TValue>(
			this IDictionary<TKey, TValue> first, IDictionary<TKey, TValue> second,
			IEqualityComparer<TValue> valueComparer)
		{
			if (first == second) return true;
			if ((first == null) || (second == null)) return false;
			if (first.Count != second.Count) return false;

			valueComparer = valueComparer ?? EqualityComparer<TValue>.Default;

			foreach (var kvp in first) {
				TValue secondValue;
				if (!second.TryGetValue(kvp.Key, out secondValue)) return false;
				if (!valueComparer.Equals(kvp.Value, secondValue)) return false;
			}
			return true;
		}
	}
}