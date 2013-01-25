#region COPYRIGHT© 2009-2013 Phillip Clark. All rights reserved.
// For licensing information see License.txt (MIT style licensing).
#endregion

using System;
using System.Collections.Generic;

namespace FlitBit.Core.Collections
{
	/// <summary>
	/// Enumerable interface over key-value-pairs.
	/// </summary>
	public class EnumerableKeyValuePairs : IEnumerable<KeyValuePair>
	{
		KeyValuePair[] _kvps;

		/// <summary>
		/// Creates a new instance from an input string.
		/// </summary>
		/// <param name="input">The input string.</param>
		/// <param name="pairSep">The character that separates key value pairs</param>
		/// <param name="kvpSep">The character that separates keys from values</param>
		public EnumerableKeyValuePairs(string input, char pairSep, char kvpSep)
		{
			if (input == null) throw new ArgumentNullException("input");

			List<KeyValuePair> items = new List<KeyValuePair>();
			foreach (var s in input.Split(pairSep))
			{
				KeyValuePair kvp;
				if (KeyValuePair.TryParse(s, kvpSep, out kvp))
				{
					items.Add(kvp);
				}
			}
			_kvps = items.ToArray();
		}

		/// <summary>
		/// Gets the enumerator.
		/// </summary>
		/// <returns></returns>
		public IEnumerator<KeyValuePair> GetEnumerator()
		{
			foreach (var kvp in _kvps)
			{
				yield return kvp;
			}
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}
