#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.Contracts;
using System.Linq;

namespace FlitBit.Core.Collections
{
  /// <summary>
  ///   Contains extensions for NameValueCollection
  /// </summary>
  public static class NameValueCollectionExtensions
  {
    /// <summary>
    ///   Transforms the value part of a name-value pair to type T if it
    ///   is present in the collection.
    /// </summary>
    /// <typeparam name="T">result type T</typeparam>
    /// <param name="nvc">the collection</param>
    /// <param name="name">the value's name</param>
    /// <returns>a result type T if the name-value pair is present; otherwise default(T)</returns>
    public static T FirstValueOrDefault<T>(this NameValueCollection nvc, string name)
    {
      Contract.Requires(nvc != null);
      Contract.Requires(name != null);
      Contract.Requires(name.Length > 0);

      var values = nvc.GetValues(name);
      if (values != null
          && values.Length > 0)
      {
        return (T)Convert.ChangeType(values[0], typeof(T));
      }
      return default(T);
    }

    /// <summary>
    ///   Transforms the value part of a name-value pair to type T if it
    ///   is present in the collection.
    /// </summary>
    /// <typeparam name="T">result type T</typeparam>
    /// <param name="nvc">the collection</param>
    /// <param name="name">the value's name</param>
    /// <param name="transform">optional function used to transform the value</param>
    /// <returns>a result type T if the name-value pair is present; otherwise default(T)</returns>
    public static T FirstValueOrDefault<T>(this NameValueCollection nvc, string name, Func<string, T> transform)
    {
      Contract.Requires(nvc != null);
      Contract.Requires(name != null);
      Contract.Requires(name.Length > 0);

      var values = nvc.GetValues(name);
      if (values != null
          && transform != null
          && values.Length > 0)
      {
        return transform(values[0]);
      }
      return default(T);
    }

    /// <summary>
    ///   Transforms the multi-value part of a name-value pair to an array of type T
    ///   if present in the collection.
    /// </summary>
    /// <typeparam name="T">result type T</typeparam>
    /// <param name="nvc">the collection</param>
    /// <param name="name">the value's name</param>
    /// <returns>an enumeration of type T containing values</returns>
    public static IEnumerable<T> ToMultiValue<T>(this NameValueCollection nvc, string name)
    {
      Contract.Requires(nvc != null);
      Contract.Requires(name != null);
      Contract.Requires(name.Length > 0);

      if (nvc != null)
      {
        var values = nvc.GetValues(name);
        if (values != null
            && values.Any())
        {
          return values.Select(v => Convert.ChangeType(v, typeof(T)))
                       .Cast<T>();
        }
      }
      return Enumerable.Empty<T>();
    }

    /// <summary>
    ///   Transforms the multi-value part of a name-value pair to an array of type T
    ///   if present in the collection.
    /// </summary>
    /// <typeparam name="T">result type T</typeparam>
    /// <param name="nvc">the collection</param>
    /// <param name="name">the value's name</param>
    /// <param name="transform">transforms the string to result type T</param>
    /// <returns>an enumeration of type T containing values</returns>
    public static IEnumerable<T> ToMultiValue<T>(this NameValueCollection nvc, string name, Func<string, T> transform)
    {
      Contract.Requires(nvc != null);
      Contract.Requires(name != null);
      Contract.Requires(name.Length > 0);

      if (nvc != null)
      {
        var values = nvc.GetValues(name);
        if (values != null
            && values.Any())
        {
          return values.Select(transform);
        }
      }
      return Enumerable.Empty<T>();
    }
  }
}