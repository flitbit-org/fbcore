#region COPYRIGHT© 2009-2013 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using System.Collections.Concurrent;
using System.Collections.Specialized;
using System.Globalization;

namespace FlitBit.Core.Collections
{
	/// <summary>
	///   Contains utility extensions.
	/// </summary>
	public static class Extensions
	{
		/// <summary>
		///   Reliably sets the value for a key in the concurrent dictionary.
		/// </summary>
		/// <typeparam name="K">key type K</typeparam>
		/// <typeparam name="V">value type V</typeparam>
		/// <param name="dictionary">the dictionary</param>
		/// <param name="key">the key</param>
		/// <param name="value">the value</param>
		/// <returns>
		///   If there is a value in the dictionary already associated with the key
		///   then that value is returned; otherwise default(V).
		/// </returns>
		/// <remarks>
		///   Because this method is performed on a concurrent dictionary in a non-blocking
		///   manner it is possible that parallel operations may change the value associated
		///   with the key concurrently. What this method guarantees is that the value
		///   associated with the key is set to the value given; not that it remains so.
		///   The result indicates the value that was replaced at the time the the operation
		///   succeeded.
		/// </remarks>
		public static V ReliableSetValue<K, V>(this ConcurrentDictionary<K, V> dictionary, K key, V value)
		{
			while (!dictionary.TryAdd(key, value))
			{
				V current;
				if (dictionary.TryGetValue(key, out current)
					&& dictionary.TryUpdate(key, value, current))
				{
					return current;
				}
			}
			return default(V);
		}

		/// <summary>
		///   Tries to get a named byte value.
		/// </summary>
		/// <param name="collection">the collection</param>
		/// <param name="name">the name</param>
		/// <param name="value">reference to a variable where the value will be returned upon success</param>
		/// <returns>
		///   <em>true</em> if the value is present and can be parsed; otherwise <em>false</em>
		/// </returns>
		public static bool TryGetValue(this NameValueCollection collection, string name, out byte value)
		{
			var k = collection[name];
			byte v;
			if (!String.IsNullOrEmpty(k) && byte.TryParse(k, out v))
			{
				value = v;
				return true;
			}
			value = default(byte);
			return false;
		}

		/// <summary>
		///   Tries to get a named boolean value.
		/// </summary>
		/// <param name="collection">the collection</param>
		/// <param name="name">the name</param>
		/// <param name="value">reference to a variable where the value will be returned upon success</param>
		/// <returns>
		///   <em>true</em> if the value is present and can be parsed; otherwise <em>false</em>
		/// </returns>
		public static bool TryGetValue(this NameValueCollection collection, string name, out bool value)
		{
			var k = collection[name];
			bool v;
			if (!String.IsNullOrEmpty(k) && bool.TryParse(k, out v))
			{
				value = v;
				return true;
			}
			value = default(bool);
			return false;
		}

		/// <summary>
		///   Tries to get a named byte array value.
		/// </summary>
		/// <param name="collection">the collection</param>
		/// <param name="name">the name</param>
		/// <param name="value">reference to a variable where the value will be returned upon success</param>
		/// <returns>
		///   <em>true</em> if the value is present and can be parsed; otherwise <em>false</em>
		/// </returns>
		public static bool TryGetValue(this NameValueCollection collection, string name, out byte[] value)
		{
			var k = collection[name];
			if (!String.IsNullOrEmpty(k))
			{
				value = Convert.FromBase64String(k);
				return true;
			}
			value = default(byte[]);
			return false;
		}

		/// <summary>
		///   Tries to get a named double value.
		/// </summary>
		/// <param name="collection">the collection</param>
		/// <param name="name">the name</param>
		/// <param name="value">reference to a variable where the value will be returned upon success</param>
		/// <returns>
		///   <em>true</em> if the value is present and can be parsed; otherwise <em>false</em>
		/// </returns>
		public static bool TryGetValue(this NameValueCollection collection, string name, out double value)
		{
			var k = collection[name];
			double v;
			if (!String.IsNullOrEmpty(k) && double.TryParse(k, out v))
			{
				value = v;
				return true;
			}
			value = default(double);
			return false;
		}

		/// <summary>
		///   Tries to get a named decimal value.
		/// </summary>
		/// <param name="collection">the collection</param>
		/// <param name="name">the name</param>
		/// <param name="value">reference to a variable where the value will be returned upon success</param>
		/// <returns>
		///   <em>true</em> if the value is present and can be parsed; otherwise <em>false</em>
		/// </returns>
		public static bool TryGetValue(this NameValueCollection collection, string name, out decimal value)
		{
			var k = collection[name];
			decimal v;
			if (!String.IsNullOrEmpty(k) && decimal.TryParse(k, out v))
			{
				value = v;
				return true;
			}
			value = default(decimal);
			return false;
		}

		/// <summary>
		///   Tries to get a named float value.
		/// </summary>
		/// <param name="collection">the collection</param>
		/// <param name="name">the name</param>
		/// <param name="value">reference to a variable where the value will be returned upon success</param>
		/// <returns>
		///   <em>true</em> if the value is present and can be parsed; otherwise <em>false</em>
		/// </returns>
		public static bool TryGetValue(this NameValueCollection collection, string name, out float value)
		{
			var k = collection[name];
			float v;
			if (!String.IsNullOrEmpty(k) && float.TryParse(k, out v))
			{
				value = v;
				return true;
			}
			value = default(float);
			return false;
		}

		/// <summary>
		///   Tries to get a named short value.
		/// </summary>
		/// <param name="collection">the collection</param>
		/// <param name="name">the name</param>
		/// <param name="value">reference to a variable where the value will be returned upon success</param>
		/// <returns>
		///   <em>true</em> if the value is present and can be parsed; otherwise <em>false</em>
		/// </returns>
		public static bool TryGetValue(this NameValueCollection collection, string name, out short value)
		{
			var k = collection[name];
			short v;
			if (!String.IsNullOrEmpty(k) && short.TryParse(k, out v))
			{
				value = v;
				return true;
			}
			value = default(short);
			return false;
		}

		/// <summary>
		///   Tries to get a named int value.
		/// </summary>
		/// <param name="collection">the collection</param>
		/// <param name="name">the name</param>
		/// <param name="value">reference to a variable where the value will be returned upon success</param>
		/// <returns>
		///   <em>true</em> if the value is present and can be parsed; otherwise <em>false</em>
		/// </returns>
		public static bool TryGetValue(this NameValueCollection collection, string name, out int value)
		{
			var k = collection[name];
			int v;
			if (!String.IsNullOrEmpty(k) && int.TryParse(k, out v))
			{
				value = v;
				return true;
			}
			value = default(int);
			return false;
		}

		/// <summary>
		///   Tries to get a named long value.
		/// </summary>
		/// <param name="collection">the collection</param>
		/// <param name="name">the name</param>
		/// <param name="value">reference to a variable where the value will be returned upon success</param>
		/// <returns>
		///   <em>true</em> if the value is present and can be parsed; otherwise <em>false</em>
		/// </returns>
		public static bool TryGetValue(this NameValueCollection collection, string name, out long value)
		{
			var k = collection[name];
			long v;
			if (!String.IsNullOrEmpty(k) && long.TryParse(k, out v))
			{
				value = v;
				return true;
			}
			value = default(long);
			return false;
		}

		/// <summary>
		///   Tries to get a named boolean value.
		/// </summary>
		/// <param name="collection">the collection</param>
		/// <param name="name">the name</param>
		/// <param name="value">reference to a variable where the value will be returned upon success</param>
		/// <param name="styles">number styles</param>
		/// <returns>
		///   <em>true</em> if the value is present and can be parsed; otherwise <em>false</em>
		/// </returns>
		public static bool TryGetValue(this NameValueCollection collection, string name, out long value, NumberStyles styles)
		{
			var k = collection[name];
			long v;
			if (!String.IsNullOrEmpty(k) && long.TryParse(k, styles, null, out v))
			{
				value = v;
				return true;
			}
			value = default(long);
			return false;
		}

		/// <summary>
		///   Tries to get a named string value.
		/// </summary>
		/// <param name="collection">the collection</param>
		/// <param name="name">the name</param>
		/// <param name="value">reference to a variable where the value will be returned upon success</param>
		/// <returns>
		///   <em>true</em> if the value is present and can be parsed; otherwise <em>false</em>
		/// </returns>
		public static bool TryGetValue(this NameValueCollection collection, string name, out string value)
		{
			var k = collection[name];

			if (k != null)
			{
				value = k;
				return true;
			}
			value = default(string);
			return false;
		}

		/// <summary>
		///   Tries to get a named signed byte value.
		/// </summary>
		/// <param name="collection">the collection</param>
		/// <param name="name">the name</param>
		/// <param name="value">reference to a variable where the value will be returned upon success</param>
		/// <returns>
		///   <em>true</em> if the value is present and can be parsed; otherwise <em>false</em>
		/// </returns>
		[CLSCompliant(false)]
		public static bool TryGetValue(this NameValueCollection collection, string name, out sbyte value)
		{
			var k = collection[name];
			sbyte v;
			if (!String.IsNullOrEmpty(k) && sbyte.TryParse(k, out v))
			{
				value = v;
				return true;
			}
			value = default(sbyte);
			return false;
		}

		/// <summary>
		///   Tries to get a named unsigned short value.
		/// </summary>
		/// <param name="collection">the collection</param>
		/// <param name="name">the name</param>
		/// <param name="value">reference to a variable where the value will be returned upon success</param>
		/// <returns>
		///   <em>true</em> if the value is present and can be parsed; otherwise <em>false</em>
		/// </returns>
		[CLSCompliant(false)]
		public static bool TryGetValue(this NameValueCollection collection, string name, out ushort value)
		{
			var k = collection[name];
			ushort v;
			if (!String.IsNullOrEmpty(k) && ushort.TryParse(k, out v))
			{
				value = v;
				return true;
			}
			value = default(ushort);
			return false;
		}

		/// <summary>
		///   Tries to get a named unsigned int value.
		/// </summary>
		/// <param name="collection">the collection</param>
		/// <param name="name">the name</param>
		/// <param name="value">reference to a variable where the value will be returned upon success</param>
		/// <returns>
		///   <em>true</em> if the value is present and can be parsed; otherwise <em>false</em>
		/// </returns>
		[CLSCompliant(false)]
		public static bool TryGetValue(this NameValueCollection collection, string name, out uint value)
		{
			var k = collection[name];
			uint v;
			if (!String.IsNullOrEmpty(k) && uint.TryParse(k, out v))
			{
				value = v;
				return true;
			}
			value = default(uint);
			return false;
		}

		/// <summary>
		///   Tries to get a named unsigned long value.
		/// </summary>
		/// <param name="collection">the collection</param>
		/// <param name="name">the name</param>
		/// <param name="value">reference to a variable where the value will be returned upon success</param>
		/// <returns>
		///   <em>true</em> if the value is present and can be parsed; otherwise <em>false</em>
		/// </returns>
		[CLSCompliant(false)]
		public static bool TryGetValue(this NameValueCollection collection, string name, out ulong value)
		{
			var k = collection[name];
			ulong v;
			if (!String.IsNullOrEmpty(k) && ulong.TryParse(k, out v))
			{
				value = v;
				return true;
			}
			value = default(ulong);
			return false;
		}
	}
}