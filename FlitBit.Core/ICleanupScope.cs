#region COPYRIGHT© 2009-2013 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using System.Diagnostics.Contracts;
using FlitBit.Core.CodeContracts;
using FlitBit.Core.Parallel;

namespace FlitBit.Core
{
  /// <summary>
  ///   Deliniates a cleanup scope.
  /// </summary>
  [ContractClass(typeof(ContractForICleanupScope))]
  public interface ICleanupScope : IInterrogateDisposable, IParallelShared
  {
    /// <summary>
    ///   Adds a disposable item to the scope. When the scope
    ///   is disposed all added items are guaranteed to also be
    ///   disposed.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="item"></param>
    /// <returns></returns>
    T Add<T>(T item) where T : class, IDisposable;

    /// <summary>
    ///   Adds an action to be performed upon scope
    ///   completion (on dispose).
    /// </summary>
    /// <param name="action"></param>
    void AddAction(Action action);
  }

  namespace CodeContracts
  {
    /// <summary>
    ///   CodeContracts Class for ICleanupScope
    /// </summary>
    [ContractClassFor(typeof(ICleanupScope))]
    internal abstract class ContractForICleanupScope : ICleanupScope
    {
      public ICleanupScope ShareScope()
      {
        Contract.Ensures(Contract.Result<ICleanupScope>() != null);

        throw new NotImplementedException();
      }

      #region ICleanupScope Members

      public T Add<T>(T item) where T : class, IDisposable
      {
        Contract.Requires<ArgumentNullException>(item != null);
        Contract.Ensures(Contract.Result<T>() != null);

        throw new NotImplementedException();
      }

      public void AddAction(Action action)
      {
        Contract.Requires<ArgumentNullException>(action != null);

        throw new NotImplementedException();
      }

      public bool IsDisposed { get { throw new NotImplementedException(); } }

      public void Dispose() { throw new NotImplementedException(); }

      public object ParallelShare() { throw new NotImplementedException(); }

      #endregion
    }
  }
}