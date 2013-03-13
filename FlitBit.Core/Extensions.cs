#region COPYRIGHT© 2009-2013 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace FlitBit.Core
{
	/// <summary>
	///   Various extension methods.
	/// </summary>
	public static class Extensions
	{
		/// <summary>
		///   Produces a combined hashcode from the enumerated items.
		/// </summary>
		/// <typeparam name="T">element type T</typeparam>
		/// <param name="items">an enumerable</param>
		/// <param name="seed">the hash seed (starting value)</param>
		/// <returns>the combined hashcode</returns>
		public static int CalculateCombinedHashcode<T>(this IEnumerable<T> items, int seed)
		{
			if (items == null)
			{
				return seed;
			}

			var comp = EqualityComparer<T>.Default;

			const int prime = Constants.NotSoRandomPrime;
			var result = seed ^ (items.GetHashCode() * prime);
// ReSharper disable LoopCanBeConvertedToQuery
			foreach (var item in items)
			{
				if (!comp.Equals(default(T), item))
				{
					result ^= item.GetHashCode() * prime;
				}
			}
// ReSharper restore LoopCanBeConvertedToQuery
			return result;
		}

		/// <summary>
		///   Counts the number of bits turned on.
		/// </summary>
		/// <param name="value">a value</param>
		/// <returns>number of bits turned on</returns>
		[CLSCompliant(false)]
		public static int CountBitsInFlag(this uint value)
		{
			// http://en.wikipedia.org/wiki/Hamming_weight
			value = value - ((value >> 1) & 0x55555555); // reuse input as temporary
			value = (value & 0x33333333) + ((value >> 2) & 0x33333333); // temp
			var c = ((value + (value >> 4) & 0xF0F0F0F) * 0x1010101) >> 24;
			unchecked
			{
				return (int) c;
			}
		}

		/// <summary>
		///   Counts the number of bits turned on.
		/// </summary>
		/// <param name="value">a value</param>
		/// <returns>number of bits turned on</returns>
		public static int CountBitsInFlag(this int value)
		{
			// http://en.wikipedia.org/wiki/Hamming_weight
			var v = (uint) value;
			v = v - ((v >> 1) & 0x55555555);
			v = (v & 0x33333333) + ((v >> 2) & 0x33333333);
			var c = ((v + (v >> 4) & 0xF0F0F0F) * 0x1010101) >> 24;
			unchecked
			{
				return (int) c;
			}
		}

		/// <summary>
		///   Double quotes the given string, delimiting inner quotes.
		/// </summary>
		/// <param name="source"></param>
		/// <returns></returns>
		public static string DoubleQuote(this string source)
		{
			return (String.IsNullOrEmpty(source))
				? "\"\""
				: String.Concat("\"", source.Replace("\"", "\\\""), "\"");
		}

		/// <summary>
		///   Determines if the arrays are equal or if the items in two different arrays
		///   are equal.
		/// </summary>
		/// <typeparam name="T">Item type T</typeparam>
		/// <param name="lhs">Left-hand comparand</param>
		/// <param name="rhs">Right-hand comparand</param>
		/// <returns>
		///   <b>true</b> if the arrays are equal or if the items in the arrays are equal.
		/// </returns>
		public static bool EqualsOrItemsEqual<T>(this T[] lhs, T[] rhs)
		{
			var result = Equals(lhs, rhs);
			if (result == false && lhs != null && rhs != null
				&& lhs.LongLength == rhs.LongLength)
			{
				if (lhs.Length == 0)
				{
					// two empty arrays are equal.
					result = true;
				}
				else
				{
					IEqualityComparer<T> comparer = EqualityComparer<T>.Default;
					for (var i = 0; i < lhs.LongLength; i++)
					{
						result = comparer.Equals(lhs[i], rhs[i]);
						if (!result)
						{
							break;
						}
					}
				}
			}
			return result;
		}

		/// <summary>
		///   Formats an exception for output into the log.
		/// </summary>
		/// <param name="ex">the exception</param>
		/// <returns>a string representation of the exception</returns>
		public static string FormatForLogging(this Exception ex)
		{
#if DEBUG
			return FormatForLogging(ex, true);
#else
			return FormatForLogging(ex, false);
#endif
		}

		/// <summary>
		///   Formats an exception for output into the log.
		/// </summary>
		/// <param name="ex">the exception</param>
		/// <param name="exposeStackTrace">indicates whether stack trace should be exposed in the output</param>
		/// <returns>a string representation of the exception</returns>
		public static string FormatForLogging(this Exception ex, bool exposeStackTrace)
		{
			Contract.Requires<ArgumentNullException>(ex != null);
			var builder = new StringBuilder(400)
				.Append(ex.GetType().FullName).Append(": ").Append(ex.Message);
			var e = ex.InnerException;
			var indent = 1;
			while (e != null)
			{
				builder.Append(Environment.NewLine).Append(new String('\t', indent++)).Append("InnerException >")
							.Append(e.GetType().FullName).Append(": ").Append(e.Message);

				e = e.InnerException;
			}
			if (exposeStackTrace)
			{
				builder.Append(Environment.NewLine).Append(new String('\t', indent)).Append("\t StackTrace >>")
							.Append(ex.StackTrace);
			}
			return builder.ToString();
		}

		/// <summary>
		///   Gets the fully qualified, human readable name for a delegate.
		/// </summary>
		/// <param name="d"></param>
		/// <returns></returns>
		public static string GetFullName(this Delegate d)
		{
			Contract.Requires<ArgumentNullException>(d != null);
			Contract.Requires<ArgumentException>(d.Method != null);
			Contract.Requires<ArgumentException>(d.Target != null);

			var type = d.Method.DeclaringType;
			return String.Concat(type.GetReadableFullName(), ".", d.Method.Name, "()");
		}

		/// <summary>
		///   Gets a member from the expression given.
		/// </summary>
		/// <typeparam name="T">type T</typeparam>
		/// <param name="expression">the expression</param>
		/// <returns>the expression's target member</returns>
		[SuppressMessage("Microsoft.Design", "CA1011"), SuppressMessage("Microsoft.Design", "CA1006")]
		public static MemberInfo GetMemberFromExpression<T>(this Expression<Func<T, object>> expression)
		{
			Contract.Requires<ArgumentNullException>(expression != null);

			var body = expression.Body as MemberExpression;
			if (body != null)
			{
				var memberExpression = body;
				return memberExpression.Member;
			}
			var unaryExpression = (UnaryExpression) expression.Body;

			var operand = unaryExpression.Operand as MemberExpression;
			if (operand != null)
			{
				var memberExpression = operand;
				return memberExpression.Member;
			}
			return null;
		}

		/// <summary>
		///   Gets a readable full name. Since this method uses reflection it should be used
		///   rarely. It was created to supply simpler type names when constructing error messages.
		/// </summary>
		/// <param name="type">The type.</param>
		/// <returns>A readable name such as My.Namespace.MyType&lt;string, int></returns>
		public static string GetReadableFullName(this Type type)
		{
			Contract.Requires<ArgumentNullException>(type != null);

			var tt = (type.IsArray) ? type.GetElementType() : type;
			var simpleName = tt.Name;

			Contract.Assume(simpleName != null);
			Contract.Assert(simpleName.Length >= 0);

			if (simpleName.Contains('`'))
			{
				simpleName = simpleName.Substring(0, simpleName.IndexOf("`", StringComparison.InvariantCulture));
				var args = tt.GetGenericArguments();
				for (var i = 0; i < args.Length; i++)
				{
					simpleName = String.Concat(simpleName, i == 0 ? '<' : ',', args[i].GetReadableSimpleName());
				}
				simpleName = String.Concat(simpleName, '>');
			}
			return tt.IsNested
				? String.Concat(tt.DeclaringType.GetReadableFullName(), "+", simpleName)
				: String.Concat(tt.Namespace, ".", simpleName);
		}

		/// <summary>
		///   Gets a readable simple name for a type.
		/// </summary>
		/// <param name="type">the type</param>
		/// <returns>A readable name such as MyType&lt;string, int></returns>
		public static string GetReadableSimpleName(this Type type)
		{
			Contract.Requires<ArgumentNullException>(type != null);

			var tt = (type.IsArray) ? type.GetElementType() : type;
			var simpleName = tt.Name;
			if (simpleName.Contains('`'))
			{
				simpleName = simpleName.Substring(0, simpleName.IndexOf("`", StringComparison.InvariantCulture));
				var args = tt.GetGenericArguments();
				for (var i = 0; i < args.Length; i++)
				{
					simpleName = String.Concat(simpleName, i == 0 ? '<' : ',', args[i].GetReadableSimpleName());
				}
				simpleName = String.Concat(simpleName, '>');
			}
			return simpleName;
		}

		/// <summary>
		///   Creates a dynamic object over the given JSON.
		/// </summary>
		/// <param name="json">JSON input</param>
		/// <returns>a dynamic object</returns>
		public static dynamic JsonToDynamic(this string json)
		{
			if (String.IsNullOrWhiteSpace(json))
			{
				return null;
			}

			var obj = JsonConvert.DeserializeObject(json);
			if (obj is string)
			{
				return obj as string;
			}
			return ConvertJson((JToken) obj);
		}

		/// <summary>
		///   Removes a string from the end of another string if present.
		/// </summary>
		/// <param name="target">The target string.</param>
		/// <param name="value">The value to remove.</param>
		/// <returns>the target string with the value removed</returns>
		public static string RemoveTrailing(this string target, string value)
		{
			if (!String.IsNullOrEmpty(target) && !String.IsNullOrEmpty(value)
				&& target.EndsWith(value))
			{
				return target.Substring(0, target.Length - value.Length);
			}
			return target;
		}

		/// <summary>
		///   Converts the source object to JSON
		/// </summary>
		/// <param name="source">the source</param>
		/// <returns>the JSON representation of the source</returns>
		public static string ToJson(this object source) { return JsonConvert.SerializeObject(source, Formatting.Indented, new IsoDateTimeConverter()); }

		/// <summary>
		///   Converts an enumerable to a readonly collection.
		/// </summary>
		/// <typeparam name="T">type T</typeparam>
		/// <param name="collection">the collection</param>
		/// <returns>returns a read-only collection</returns>
		public static ReadOnlyCollection<T> ToReadOnly<T>(this IEnumerable<T> collection)
		{
			var roc = collection as ReadOnlyCollection<T>;
			if (roc == null)
			{
				if (collection == null)
				{
					roc = new List<T>(Enumerable.Empty<T>()).AsReadOnly();
				}
				else if (collection is List<T>)
				{
					roc = (collection as List<T>).AsReadOnly();
				}
				else
				{
					roc = new List<T>(collection).AsReadOnly();
				}
			}
			return roc;
		}

		static dynamic ConvertArrayContainingDissimilarTypes(IEnumerable<JToken> arr)
		{
			var items = new List<ExpandoObject>();
			foreach (var item in arr)
			{
				var value = ConvertJson(item);
				if (item is JValue)
				{
					var wrapper = new ExpandoObject();
					((IDictionary<string, object>) wrapper).Add("Value", value);
					value = wrapper;
				}
				items.Add(value);
			}
			return items;
		}

		static dynamic ConvertArrayContainingObjects(IEnumerable<JToken> arr) { return arr.Select(item => item.Type == JTokenType.Null ? null : ConvertJson(item)).Cast<ExpandoObject>().ToList(); }

		static dynamic ConvertArrayContainingSimilarTypes(JArray arr)
		{
			var first = arr.FirstOrDefault();
			if (first == null)
			{
				return new List<Object>();
			}
			switch (first.Type)
			{
				case JTokenType.Array:
					break;
				case JTokenType.Boolean:
					return ConvertArrayContainingTypeof<bool>(arr);
				case JTokenType.Bytes:
					return ConvertArrayContainingTypeof<byte[]>(arr);
				case JTokenType.Comment:
					break;
				case JTokenType.Constructor:
					break;
				case JTokenType.Date:
					return ConvertArrayContainingTypeof<DateTime>(arr);
				case JTokenType.Float:
					return ConvertArrayContainingTypeof<float>(arr);
				case JTokenType.Guid:
					return ConvertArrayContainingTypeof<Guid>(arr);
				case JTokenType.Integer:
					return ConvertArrayContainingTypeof<int>(arr);
				case JTokenType.Object:
					return ConvertArrayContainingObjects(arr);
				case JTokenType.String:
					return ConvertArrayContainingTypeof<string>(arr);
				case JTokenType.TimeSpan:
					break;
				case JTokenType.Undefined:
					break;
				case JTokenType.Uri:
					break;
			}
			throw new NotImplementedException();
		}

		static dynamic ConvertArrayContainingTypeof<T>(IEnumerable<JToken> arr) { return arr.Select(item => item.Type == JTokenType.Null ? default(T) : item.Value<T>()).ToList(); }

		static dynamic ConvertJson(IEnumerable<JToken> token)
		{
			var value = token as JValue;
			if (value != null)
			{
				return value.Value;
			}
			if (token is JObject)
			{
				var expando = new ExpandoObject();
				(from childToken in token where childToken is JProperty select childToken as JProperty).ToList()
																																															.ForEach(
																																																			 property =>
																																																				((IDictionary<string, object>) expando).Add(property.Name,
																																																																										ConvertJson(property.Value)));
				return expando;
			}
			var arr = token as JArray;
			if (arr != null)
			{
				var types = arr
					.Where(tk => tk.Type != JTokenType.Null)
					.GroupBy(tk => tk.Type)
					.Count();
				if (types > 1)
				{
					return ConvertArrayContainingDissimilarTypes(arr);
				}
				return ConvertArrayContainingSimilarTypes(arr);
			}
			throw new ArgumentException(string.Format("Unknown token type '{0}'",
																								token.GetType()), "token"
				);
		}
	}
}