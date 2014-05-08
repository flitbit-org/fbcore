#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading;
using FlitBit.Core.Properties;

namespace FlitBit.Core
{
  /// <summary>
  ///   Utility structure for performing and tracking threadsafe state transitions.
  /// </summary>
  /// <typeparam name="TEnum">State type E (should be an enum)</typeparam>
  [Serializable]
// ReSharper disable InconsistentNaming
  public struct Status<TEnum>
// ReSharper restore InconsistentNaming
    where TEnum : struct
  {
// ReSharper disable StaticFieldInGenericType
    static readonly int CHashCodeSeed = typeof(Status<TEnum>).AssemblyQualifiedName.GetHashCode();
// ReSharper restore StaticFieldInGenericType

    int _status;

    [SuppressMessage("Microsoft.Usage", "CA2207")]
    static Status() { Contract.Assert(typeof(TEnum).IsEnum, Resources.Err_StatusTypeMustBeEnum); }

    /// <summary>
    ///   Creates a new instance.
    /// </summary>
    /// <param name="initialState">Initial state</param>
    public Status(TEnum initialState) { _status = Convert.ToInt32(initialState); }

    /// <summary>
    ///   Accesses the current state.
    /// </summary>
    public TEnum CurrentState { get { return (TEnum)Enum.ToObject(typeof(TEnum), Thread.VolatileRead(ref _status)); } }

    /// <summary>
    ///   Tests whether the status is equal to another
    /// </summary>
    /// <param name="obj">the other</param>
    /// <returns>true if equal; otherwise false</returns>
    public override bool Equals(object obj) { return (obj is Status<TEnum>) && Equals((Status<TEnum>)obj); }

    /// <summary>
    ///   Gets the hashcode.
    /// </summary>
    /// <returns>the hashcode</returns>
    public override int GetHashCode()
    {
      const int prime = Constants.NotSoRandomPrime;
      var code = CHashCodeSeed * prime;
// ReSharper disable NonReadonlyFieldInGetHashCode
      return code ^ this._status * prime;
// ReSharper restore NonReadonlyFieldInGetHashCode
    }

    /// <summary>
    ///   Transitions to the given state.
    /// </summary>
    /// <param name="value">the target state</param>
    public bool ChangeState(TEnum value)
    {
      var v = Convert.ToInt32(value);
      var fin = Thread.VolatileRead(ref _status);
      while (true)
      {
        if (fin == v)
        {
          return false;
        }

        var init = fin;
        fin = Interlocked.CompareExchange(ref _status, v, init);
        if (fin == init)
        {
          return true;
        }
      }
    }

    /// <summary>
    ///   Compares the current state to the comparand, if they are equal, replaces the current state with the values
    /// </summary>
    /// <param name="value">the value</param>
    /// <param name="comparand">the comparand</param>
    /// <returns>the status prior</returns>
    public TEnum CompareExchange(TEnum value, TEnum comparand)
    {
      return
        (TEnum)
        Enum.ToObject(typeof(TEnum),
          Interlocked.CompareExchange(ref _status, Convert.ToInt32(value), Convert.ToInt32(comparand)));
    }

    /// <summary>
    ///   Tests whethe the status is equal to another.
    /// </summary>
    /// <param name="other">the other</param>
    /// <returns>true if equal; otherwise false</returns>
    public bool Equals(Status<TEnum> other) { return this._status == other._status; }

    /// <summary>
    ///   Determines if the current state includes the value given.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public bool HasState(TEnum value)
    {
      var v = Convert.ToInt32(value);
      return (_status & v) == v;
    }

    /// <summary>
    ///   Determines if the current state is greater than the comparand.
    /// </summary>
    /// <param name="comparand">comparand</param>
    /// <returns>
    ///   <em>true</em> if the current state is greater than <paramref name="comparand" />; otherwise <em>false</em>
    /// </returns>
    public bool IsGreaterThan(TEnum comparand) { return Thread.VolatileRead(ref _status) > Convert.ToInt32(comparand); }

    /// <summary>
    ///   Determines if the current state is less than the comparand.
    /// </summary>
    /// <param name="comparand">comparand</param>
    /// <returns>
    ///   <em>true</em> if the current state is less than <paramref name="comparand" />; otherwise <em>false</em>
    /// </returns>
    public bool IsLessThan(TEnum comparand) { return Thread.VolatileRead(ref _status) < Convert.ToInt32(comparand); }

    /// <summary>
    ///   Performs a state transition if the current state compares greater than the <paramref name="comparand" />
    /// </summary>
    /// <param name="value">the target state</param>
    /// <param name="comparand">comparand state</param>
    /// <returns>
    ///   <em>true</em> if the current state compares greater than <paramref name="comparand" />; otherwise <em>false</em>
    /// </returns>
    public bool SetStateIfGreaterThan(TEnum value, TEnum comparand)
    {
      var c = Convert.ToInt32(comparand);
      var v = Convert.ToInt32(value);

      var fin = Thread.VolatileRead(ref _status);
      while (true)
      {
        if (fin < c)
        {
          return false;
        }

        var init = fin;
        fin = Interlocked.CompareExchange(ref _status, v, init);
        if (fin == init)
        {
          return true;
        }
      }
    }

    /// <summary>
    ///   Performs a state transition if the current state compares less than the <paramref name="comparand" />
    /// </summary>
    /// <param name="value">the target state</param>
    /// <param name="comparand">comparand state</param>
    /// <returns>
    ///   <em>true</em> if the current state compares less than <paramref name="comparand" />; otherwise <em>false</em>
    /// </returns>
    public bool SetStateIfLessThan(TEnum value, TEnum comparand)
    {
      var c = Convert.ToInt32(comparand);
      var v = Convert.ToInt32(value);

      var fin = Thread.VolatileRead(ref _status);
      while (true)
      {
        if (fin > c)
        {
          return false;
        }

        var init = fin;
        fin = Interlocked.CompareExchange(ref _status, v, init);
        if (fin == init)
        {
          return true;
        }
      }
    }

    /// <summary>
    ///   Performs a state transition if the current state compares less than the <paramref name="comparand" />
    /// </summary>
    /// <param name="value">the target state</param>
    /// <param name="comparand">comparand state</param>
    /// <param name="action">An action to be performed if the state transition succeeds</param>
    /// <returns>
    ///   <em>true</em> if the current state compares less than <paramref name="comparand" />; otherwise <em>false</em>
    /// </returns>
    public bool SetStateIfLessThan(TEnum value, TEnum comparand, Action action)
    {
      Contract.Requires<ArgumentNullException>(action != null);
      if (SetStateIfLessThan(value, comparand))
      {
        action();
        return true;
      }
      return false;
    }

    /// <summary>
    ///   Toggles between the toggle state and the desired state - with
    ///   a spin-wait if necessary.
    /// </summary>
    /// <param name="desired">desired state</param>
    /// <param name="toggle">state from which the desired state can toggle</param>
    /// <returns>
    ///   <em>true</em> if the state transitions to the desired state from the toggle state; otherwise <em>false</em>
    /// </returns>
    public bool SpinToggleState(TEnum desired, TEnum toggle)
    {
      var d = Convert.ToInt32(desired);
      var t = Convert.ToInt32(toggle);

      spin:
      var r = Interlocked.CompareExchange(ref _status, d, t);
      // If the state was the toggle state then we're successful and done...
      if (r == t)
      {
        return true;
      }
      // otherwise if the result is anything but the desired state we're
      // unsuccessful and done...
      if (r != d)
      {
        return false;
      }
      // otherwise we spin
      goto spin;
    }

    /// <summary>
    ///   Performs a spinwait until the current state equals the target state.
    /// </summary>
    /// <param name="targetState">the target state</param>
    /// <param name="loopAction">An action to perform inside the spin cycle</param>
    public void SpinWaitForState(TEnum targetState, Action loopAction)
    {
      var state = Convert.ToInt32(targetState);
      while (Thread.VolatileRead(ref _status) != state)
      {
        loopAction();
      }
    }

    /// <summary>
    ///   Performs a spinwait until the current state equals the target state.
    /// </summary>
    /// <param name="targetState">the target state</param>
    /// <param name="loopAction">
    ///   An action to perform inside the spin cycle;
    ///   waiting continues until either the target state is reached or the loop
    ///   action returns false.
    /// </param>
    /// <returns>
    ///   <em>true</em> if the target state was reached; otherwise <em>false</em>.
    /// </returns>
    public bool TrySpinWaitForState(TEnum targetState, Func<TEnum, bool> loopAction)
    {
      var target = Convert.ToInt32(targetState);
      int current;
      while ((current = Thread.VolatileRead(ref _status)) != target)
      {
        if (!loopAction((TEnum)Enum.ToObject(typeof(TEnum), current)))
        {
          // loop signaled to stop waiting...
          return false;
        }
      }
      // wait completed, state reached...
      return true;
    }

    /// <summary>
    ///   Tries to transition the state
    /// </summary>
    /// <param name="value">the target state</param>
    /// <param name="comparand">comparand state must match current state</param>
    /// <returns>
    ///   <em>true</em> if the current state matches <paramref name="comparand" /> and the state is transitioned to
    ///   <paramref
    ///     name="value" />
    ///   ; otherwise <em>false</em>
    /// </returns>
    public bool TryTransition(TEnum value, TEnum comparand)
    {
      var c = Convert.ToInt32(comparand);
      return Interlocked.CompareExchange(ref _status, Convert.ToInt32(value), c) == c;
    }

    /// <summary>
    ///   Tries to transition the state.
    /// </summary>
    /// <param name="value">the target state</param>
    /// <param name="comparand">one or more comparands</param>
    /// <returns>
    ///   true if the current state matches one of the comparands and is transitioned to <paramref name="value" />; otherwise
    ///   false
    /// </returns>
    public bool TryTransition(TEnum value, params TEnum[] comparand)
    {
      var comparands = (from c in comparand
                        select Convert.ToInt32(c)).ToArray();
      var v = Convert.ToInt32(value);

      var state = Thread.VolatileRead(ref _status);
      while (comparands.Contains(state))
      {
        if (Interlocked.CompareExchange(ref _status, v, state) == state)
        {
          return true;
        }
        state = Thread.VolatileRead(ref _status);
      }

      return false;
    }

    /// <summary>
    ///   Tries to transition the state. Upon success executes the action given.
    /// </summary>
    /// <param name="value">the target state</param>
    /// <param name="comparand">comparand state must match current state</param>
    /// <param name="action">action to perform if the state transition is successful</param>
    /// <returns>
    ///   <em>true</em> if the current state matches <paramref name="comparand" /> and the state is transitioned to
    ///   <paramref
    ///     name="value" />
    ///   ; otherwise <em>false</em>
    /// </returns>
    public bool TryTransition(TEnum value, TEnum comparand, Action action)
    {
      Contract.Requires<ArgumentNullException>(action != null);
      if (TryTransition(value, comparand))
      {
        action();
        return true;
      }
      return false;
    }

    /// <summary>
    ///   Specialized == operator
    /// </summary>
    /// <param name="lhs">left hand comparand</param>
    /// <param name="rhs">right hand comparand</param>
    /// <returns>true if the comparands are equal</returns>
    public static bool operator ==(Status<TEnum> lhs, Status<TEnum> rhs) { return lhs.Equals(rhs); }

    /// <summary>
    ///   Specialized == operator
    /// </summary>
    /// <param name="lhs">left hand comparand</param>
    /// <param name="rhs">right hand comparand</param>
    /// <returns>true if the comparands are equal</returns>
    public static bool operator ==(Status<TEnum> lhs, TEnum rhs) { return lhs.Equals(ToStatus(rhs)); }

    /// <summary>
    ///   Specialized == operator
    /// </summary>
    /// <param name="lhs">left hand comparand</param>
    /// <param name="rhs">right hand comparand</param>
    /// <returns>true if the comparands are equal</returns>
    public static bool operator ==(TEnum lhs, Status<TEnum> rhs)
    {
      return ToStatus(lhs)
        .Equals(rhs);
    }

    /// <summary>
    ///   Specialized != operator
    /// </summary>
    /// <param name="lhs">left hand comparand</param>
    /// <param name="rhs">right hand comparand</param>
    /// <returns>true if the comparands are not equal</returns>
    public static bool operator !=(Status<TEnum> lhs, Status<TEnum> rhs) { return !lhs.Equals(rhs); }

    /// <summary>
    ///   Specialized != operator
    /// </summary>
    /// <param name="lhs">left hand comparand</param>
    /// <param name="rhs">right hand comparand</param>
    /// <returns>true if the comparands are not equal</returns>
    public static bool operator !=(Status<TEnum> lhs, TEnum rhs) { return !lhs.Equals(ToStatus(rhs)); }

    /// <summary>
    ///   Specialized != operator
    /// </summary>
    /// <param name="lhs">left hand comparand</param>
    /// <param name="rhs">right hand comparand</param>
    /// <returns>true if the comparands are not equal</returns>
    public static bool operator !=(TEnum lhs, Status<TEnum> rhs)
    {
      return !ToStatus(lhs)
                .Equals(rhs);
    }

    /// <summary>
    ///   Converts Status&lt;E> to E
    /// </summary>
    /// <param name="s">the status</param>
    /// <returns>the equivalent E</returns>
    [SuppressMessage("Microsoft.Design", "CA1000", Justification = "By design.")]
    public static TEnum ToObject(Status<TEnum> s) { return s.CurrentState; }

    /// <summary>
    ///   Converts E to Status&lt;E>
    /// </summary>
    /// <param name="s">the value</param>
    /// <returns>the equivalent Status&lt;E></returns>
    [SuppressMessage("Microsoft.Design", "CA1000", Justification = "By design.")]
    public static Status<TEnum> ToStatus(TEnum s) { return new Status<TEnum>(s); }
  }
}