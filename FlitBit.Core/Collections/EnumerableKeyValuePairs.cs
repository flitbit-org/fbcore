#region COPYRIGHT© 2009-2013 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace FlitBit.Core.Collections
{
	/// <summary>
	///   Enumerable interface over key-value-pairs.
	/// </summary>
	public class EnumerableKeyValuePairs : IEnumerable<KeyValuePair>
	{
		readonly KeyValuePair[] _kvps;

		/// <summary>
		///   Creates a new instance from an input string.
		/// </summary>
		/// <param name="input">The input string.</param>
		/// <param name="pairSep">The character that separates key value pairs</param>
		/// <param name="kvpSep">The character that separates keys from values</param>
		public EnumerableKeyValuePairs(string input, char pairSep, char kvpSep)
		{
			Contract.Requires<ArgumentNullException>(input != null);

			var items = new List<KeyValuePair>();
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

		#region IEnumerable<KeyValuePair> Members

		/// <summary>
		///   Gets the enumerator.
		/// </summary>
		/// <returns></returns>
		public IEnumerator<KeyValuePair> GetEnumerator() {
			return ((IEnumerable<KeyValuePair>) this._kvps).GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

		#endregion
	}
}