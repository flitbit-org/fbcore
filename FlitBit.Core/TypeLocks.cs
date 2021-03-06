﻿#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using System.Collections.Concurrent;
using System.Diagnostics.Contracts;

namespace FlitBit.Core
{
  /// <summary>
  ///   Utility class for locks used on types.
  /// </summary>
  public static class TypeLocks
  {
    static readonly ConcurrentDictionary<string, object> SafeLocks = new ConcurrentDictionary<string, object>();

    /// <summary>
    ///   Gets a key for a type suitable for representing the type as a hashtable
    ///   or dictionary key without pinning the type and its assembly into memory.
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static object GetKeyForType(this Type type)
    {
      Contract.Requires<ArgumentNullException>(type != null);
      Contract.Ensures(Contract.Result<object>() != null);
      Contract.Assume(type.AssemblyQualifiedName != null);

      var key = type.AssemblyQualifiedName.InternIt();
      return SafeLocks.GetOrAdd(key, k => new Object());
    }

    /// <summary>
    ///   Gets a lock for a type suitable for synchronizing activity on the type
    ///   without blocking other activity in the VM.
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static object GetLockForType(this Type type)
    {
      Contract.Requires<ArgumentNullException>(type != null);
      Contract.Ensures(Contract.Result<object>() != null);

      return GetKeyForType(type);
    }
  }
}