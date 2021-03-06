﻿#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.
// For licensing information see License.txt (MIT style licensing).
#endregion

using System;

namespace FlitBit.Core
{
  /// <summary>
  ///   Extends the weak reference type.
  /// </summary>
  public static class WeakReferenceExtensions
  {
    /// <summary>
    ///   Tries to get the target of the reference.
    /// </summary>
    /// <typeparam name="T">type T of the referenced object</typeparam>
    /// <param name="weakRef">the target reference</param>
    /// <param name="target">reference to a variable that will recieve the target if successful</param>
    /// <returns>
    ///   <em>true</em> if the reference is alive and has a valid value of type T; otherwise <em>false</em>
    /// </returns>
    /// <exception cref="InvalidCastException">thrown if the target of the reference cannot be cast to type T</exception>
    public static bool TryGetStrongTarget<T>(this WeakReference weakRef, out T target)
    {
      if (weakRef.IsAlive)
      {
        target = (T)weakRef.Target;
        return true;
      }
      target = default(T);
      return false;
    }
  }
}