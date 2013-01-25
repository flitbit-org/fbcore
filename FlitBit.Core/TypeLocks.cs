#region COPYRIGHT© 2009-2013 Phillip Clark. All rights reserved.
// For licensing information see License.txt (MIT style licensing).
#endregion

using System;
using System.Collections.Concurrent;
using System.Diagnostics.Contracts;

namespace FlitBit.Core
{
  /// <summary>
  /// Utility class for locks used on types.
  /// </summary>
  public static class TypeLocks
  {
    static ConcurrentDictionary<string, object> __safeLocks = new ConcurrentDictionary<string, object>();

    /// <summary>
    /// Gets a lock for a type suitable for synchronizing activity on the type
    /// without blocking other activity in the VM.
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static Object GetLockForType(this Type type)
    {
      Contract.Requires<ArgumentNullException>(type != null);
      Contract.Ensures(Contract.Result<object>() != null);

      return GetKeyForType(type);
    }

    /// <summary>
    /// Gets a key for a type suitable for representing the type as a hashtable
    /// or dictionary key without pinning the type and its assembly into memory.
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static object GetKeyForType(this Type type)
    {
      Contract.Requires<ArgumentNullException>(type != null);
      Contract.Ensures(Contract.Result<object>() != null);
      Contract.Assume(type.AssemblyQualifiedName != null);

      string key = Util.InternIt(type.AssemblyQualifiedName);
      return __safeLocks.GetOrAdd(key, k => new Object());
     }
  }
}
