﻿#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.InteropServices;
using System.Threading;
using FlitBit.Core.Log;
using FlitBit.Core.Parallel;
using FlitBit.Core.Properties;

namespace FlitBit.Core
{
  /// <summary>
  ///   Utility class for collecting actions and disposable items for cleanup. Actions and
  ///   disposable items, at dispose time, are either disposed (IDisposables)
  ///   or invoked (Actions) in the reverse order in which they are added to the scope.
  /// </summary>
  public class CleanupScope : Disposable, ICleanupScope
  {

	  internal class CleanupScopeContextFlowProvider : IContextFlowProvider
	  {
		  static readonly Lazy<CleanupScopeContextFlowProvider> Provider =
			  new Lazy<CleanupScopeContextFlowProvider>(CreateAndRegisterContextFlowProvider, LazyThreadSafetyMode.ExecutionAndPublication);

		  static CleanupScopeContextFlowProvider CreateAndRegisterContextFlowProvider()
		  {
			  var res = new CleanupScopeContextFlowProvider();
			  ContextFlow.RegisterProvider(res);
			  return res;
		  }

		  [ThreadStatic]
		  static Stack<CleanupScope> __scopes;

		  public CleanupScopeContextFlowProvider()
		  {
			  this.ContextKey = Guid.NewGuid();
		  }

			public Guid ContextKey
			{
				get; private set;
			}

			public object Capture()
			{
				var top = Peek();
				if (top != null)
				{
					return top.ShareScope();
				}
				return null;
			}

			public object Attach(ContextFlow context, object capture)
			{
				var scope = (capture as CleanupScope);
				if (scope != null)
				{
					if (__scopes == null)
					{
						__scopes = new Stack<CleanupScope>();
					}
					if (__scopes.Count > 0)
					{
						ReportAndClearOrphanedScopes(__scopes);
					}
					__scopes.Push(scope);
				}
			  return scope;
			}

			private void ReportAndClearOrphanedScopes(Stack<CleanupScope> scopes)
			{
				scopes.Clear();
			}

			public void Detach(ContextFlow context, object attachment, Exception err)
			{
				var scope = (attachment as CleanupScope);
				if (scope != null)
				{
					scope.Dispose();
				}
			}

		  internal static void Push(CleanupScope scope)
		  {
			  var dummy = Provider.Value;
			  if (__scopes == null)
			  {
				  __scopes = new Stack<CleanupScope>();
			  }
				__scopes.Push(scope);
		  }

		  internal static bool TryPop(CleanupScope scope)
		  {
			  if (__scopes != null && __scopes.Count > 0)
			  {
				  if (ReferenceEquals(__scopes.Peek(), scope))
				  {
					  __scopes.Pop();
					  return true;
				  }
			  }
			  return false;
		  }

			internal static CleanupScope Pop()
			{
				if (__scopes != null && __scopes.Count > 0)
				{
					return __scopes.Pop();
				}
				return default(CleanupScope);
			}


			internal static CleanupScope Peek()
			{
				if (__scopes != null && __scopes.Count > 0)
				{
					return __scopes.Peek();
				}
				return default(CleanupScope);
			}
	  }

		static readonly ILogSink LogSink = typeof(CleanupScope).GetLogSink();

    readonly bool _independent;
    readonly ConcurrentStack<StackItem> _items = new ConcurrentStack<StackItem>();
    readonly object _ownerNotifier;

    int _disposers = 1;

    EventHandler<CleanupScopeItemEventArgs> _itemAdded;
    EventHandler<CleanupScopeItemEventArgs> _itemDisposed;

	  // Reference counts disposing threads. We always have one.

    /// <summary>
    ///   Creates a new scope.
    /// </summary>
    public CleanupScope()
      : this(false)
    {}

    /// <summary>
    ///   Creates a new scope.
    /// </summary>
    /// <param name="independent">indicates whether the scope is independent of the stack</param>
    public CleanupScope(bool independent)
    {
      this._independent = independent;
      if (!_independent)
      {
        CleanupScopeContextFlowProvider.Push(this);
      }
    }

    /// <summary>
    ///   Creates a new instance.
    /// </summary>
    /// <param name="independent">indicates whether the scope is independent of the stack</param>
    /// <param name="ownerNotifier">the owner, notifier</param>
    public CleanupScope(object ownerNotifier, bool independent)
      : this(independent)
    {
      _ownerNotifier = ownerNotifier;
    }

    /// <summary>
    ///   Creates a new instance.
    /// </summary>
    /// <param name="ownerNotifier">the owner, notifier</param>
    public CleanupScope(object ownerNotifier)
      : this(ownerNotifier, false)
    {}

    /// <summary>
    ///   Creates a new scope and adds to it the disposable item given.
    /// </summary>
    /// <param name="independent">indicates whether the scope is independent of the stack</param>
    /// <param name="items">Items to be disposed when the scope is cleaned up.</param>
    public CleanupScope(bool independent, params IDisposable[] items)
      : this(independent)
    {
      Contract.Assume(items != null);
      foreach (var item in items)
      {
        if (item != null)
        {
          _items.Push(new StackItem(item));
        }
      }
    }

    /// <summary>
    ///   Creates a new scope and adds to it the disposable item given.
    /// </summary>
    /// <param name="items">Items to be disposed when the scope is cleaned up.</param>
    public CleanupScope(params IDisposable[] items)
      : this(false, items)
    {}

    /// <summary>
    ///   Creates a new scope and adds an action to be performed when the scope is cleaned up.
    /// </summary>
    /// <param name="independent">indicates whether the scope is independent of the stack</param>
    /// <param name="actions">an array of actions to be performed when the scope is cleaned up.</param>
    public CleanupScope(bool independent, params Action[] actions)
      : this(independent)
    {
      foreach (var action in actions)
      {
        _items.Push(new StackItem(action));
      }
    }

    /// <summary>
    ///   Creates a new scope and adds an action to be performed when the scope is cleaned up.
    /// </summary>
    /// <param name="actions">an array of actions to be performed when the scope is cleaned up.</param>
    public CleanupScope(params Action[] actions)
      : this(false, actions)
    {}

    /// <summary>
    ///   Shares the scope. Callers must guarantee that there is a matching call to Dispose
    ///   for every call to share. Preferrably by wrapping it in a using clause.
    /// </summary>
    /// <returns>the shared scope</returns>
    public ICleanupScope ShareScope()
    {
      if (IsDisposed)
      {
        throw new ObjectDisposedException(typeof(CleanupScope).FullName);
      }

      Interlocked.Increment(ref _disposers);
      return this;
    }

    /// <summary>
    ///   Performs the scope's disposal.
    /// </summary>
    /// <param name="disposing">indicates whether the scope is disposing</param>
    /// <returns>
    ///   <em>true</em> if disposed as a result of the call; otherwise <em>false</em>
    /// </returns>
    protected override bool PerformDispose(bool disposing)
    {
      if (disposing && Interlocked.Decrement(ref _disposers) > 0)
      {
        return false;
      }
      if (disposing
          && !_independent
          && !CleanupScopeContextFlowProvider.TryPop(this))
      {
        // Notify the caller that they are calling dispose out of order.
        // This never happens if the caller uses a 'using' 
        const string message =
          "Cleanup scope disposed out of order. To eliminate this possibility always wrap the scope in a 'using clause'.";
        try
        {
          LogSink.Warning(message);
        }
          // ReSharper disable EmptyGeneralCatchClause
        catch
          // ReSharper restore EmptyGeneralCatchClause
        {
          /* safety net, intentionally eat the since we might be in GC thread */
        }
      }
      if (disposing)
      {
        StackItem item;
        while (_items.TryPop(out item))
        {
          try
          {
            if (item.Disposable != null)
            {
              item.Disposable.Dispose();
              NotifyItemDisposed(item.Disposable);
            }
            else if (item.Action != null)
            {
              item.Action();
            }
          }
          catch (Exception e)
          {
            // We may be in the GC, trace as a warning and eat any exception
            // thrown by trace logic...
            try
            {
	            var errItem = item;
              LogSink.Warning(() =>
                String.Concat(Resources.Warn_ErrorWhileDisposingCleanupScope,
                  ": ",
									(errItem.Disposable == null)
										? errItem.Action.GetFullName()
										: errItem.Disposable.GetType()
                          .FullName,
                  "; ", e.FormatForLogging())
                );
            }
// ReSharper disable EmptyGeneralCatchClause
            catch
// ReSharper restore EmptyGeneralCatchClause
            {
              /* safety net, intentionally eat the since we might be in GC thread */
            }
          }
        }
      }
      return true;
    }

    void NotifyItemAdded(object item)
    {
      if (_itemAdded != null)
      {
        var sender = _ownerNotifier ?? this;
        _itemAdded(sender, new CleanupScopeItemEventArgs(item));
      }
    }

    void NotifyItemDisposed(object item)
    {
      if (_itemDisposed != null)
      {
        var sender = _ownerNotifier ?? this;
        object args = new[]
        {
          sender,
          new CleanupScopeItemEventArgs(item)
        };
        foreach (var handler in _itemDisposed.GetInvocationList())
        {
          try
          {
            handler.DynamicInvoke(args);
          }
// ReSharper disable EmptyGeneralCatchClause
          catch
// ReSharper restore EmptyGeneralCatchClause
          {
            /* Ouch!, we're in the GC - nothing to do here. */
          }
        }
      }
    }

    /// <summary>
    ///   Event fired when items are added to he scope.
    /// </summary>
    public event EventHandler<CleanupScopeItemEventArgs> ItemAdded
    {
      add { _itemAdded += value; }
// ReSharper disable DelegateSubtraction
      remove { _itemAdded -= value; }
// ReSharper restore DelegateSubtraction
    }

    /// <summary>
    ///   Event fired when items are disposed by the scope.
    /// </summary>
    public event EventHandler<CleanupScopeItemEventArgs> ItemDisposed
    {
      add { _itemDisposed += value; }
// ReSharper disable DelegateSubtraction
      remove { _itemDisposed -= value; }
// ReSharper restore DelegateSubtraction
    }

    #region ICleanupScope Members

    /// <summary>
    ///   Adds a disposable item to the cleanup scope. Actions and disposable items are collected
    ///   and at cleanup whill be either disposed (IDisposables) or invoked (Actions) in the reverse
    ///   order in which they are added.
    /// </summary>
    /// <typeparam name="T">Type of the item being added; ensures IDisposable by inference.</typeparam>
    /// <param name="item">An item to be disposed when the scope is cleaned up.</param>
    /// <returns>Returns the item given.</returns>
    public T Add<T>(T item)
      where T : class, IDisposable
    {
      _items.Push(new StackItem(item));
      NotifyItemAdded(item);
      return item;
    }

    /// <summary>
    ///   Adds an action to the cleanup scope. Actions and IDisposables collected in the same queue and
    ///   are either disposed (IDisposables) or invoked (Actions) in the reverse order in which they are
    ///   added.
    /// </summary>
    /// <param name="action">An action to be performed when the scope is cleaned up.</param>
    public void AddAction(Action action)
    {
      _items.Push(new StackItem(action));
      NotifyItemAdded(action);
    }


    #endregion

    /// <summary>
    ///   Gets the current "ambient" cleanup scope. This is the nearest
    ///   cleanup scope in the call stack.
    /// </summary>
    public static ICleanupScope Current
    {
      get
      {
	      return CleanupScopeContextFlowProvider.Peek();
      }
    }

    /// <summary>
    ///   Shares the ambient scope if it exists; otherwise, creates a new scope.
    /// </summary>
    /// <returns>a cleanup scope</returns>
    public static ICleanupScope NewOrSharedScope()
    {
			var ambient = CleanupScopeContextFlowProvider.Peek();
      return (ambient != null)
               ? ambient.ShareScope()
							 : new CleanupScope();
    }

    [StructLayout(LayoutKind.Sequential)]
    struct StackItem
    {
      public readonly Action Action;
      public readonly IDisposable Disposable;

      public StackItem(IDisposable d)
      {
        this.Action = null; // keep compiler happy
        this.Disposable = d;
      }

      public StackItem(Action a)
      {
        this.Disposable = null; // keep compiler happy
        this.Action = a;
      }
    }
  }
}