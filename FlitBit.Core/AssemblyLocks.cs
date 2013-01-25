﻿using System;
using System.Collections.Concurrent;
using System.Diagnostics.Contracts;
using System.Reflection;

namespace FlitBit.Core
{
  /// <summary>
  /// Utility class for locks used on assemblies.
  /// </summary>
  public static class AssemblyLocks
  {
    static ConcurrentDictionary<string, object> __safeLocks = new ConcurrentDictionary<string, object>();

    /// <summary>
    /// Gets a lock for an assembly suitable for synchronizing activity 
    /// on the assembly without blocking other activity in the VM.
    /// </summary>
    /// <param name="assembly">the assembly</param>
    /// <returns>an object suitable for locking activity against the assembly</returns>
    public static object GetLockForAssembly(this Assembly assembly)
    {
      Contract.Requires<ArgumentNullException>(assembly != null);
      Contract.Ensures(Contract.Result<object>() != null);

      return GetKeyForAssembly(assembly);
    }

    /// <summary>
    /// Gets a key for an assembly suitable for representing the assembly 
    /// as a hashtable or dictionary key without pinning the assembly 
    /// into memory.
    /// </summary>
    /// <param name="assembly">the assembly</param>
    /// <returns>a key for an assembly</returns>
    public static object GetKeyForAssembly(this Assembly assembly)
    {
      Contract.Requires<ArgumentNullException>(assembly != null);
      Contract.Ensures(Contract.Result<object>() != null);

      string key = Util.InternIt(assembly.FullName);
      object safeKey = __safeLocks.GetOrAdd(key, k => new Object());
      return safeKey;
    }
  }
}
