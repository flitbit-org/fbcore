#region COPYRIGHT© 2009-2013 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;

namespace FlitBit.Core.Collections
{
  /// <summary>
  ///   Structure around a key-value-pair.
  /// </summary>
  public struct KeyValuePair
  {
    static readonly int CHashCodeSeed = typeof(KeyValuePair).AssemblyQualifiedName.GetHashCode();

    readonly string _key;
    readonly string _value;

    /// <summary>
    ///   Creates a new instance.
    /// </summary>
    /// <param name="k">the key</param>
    /// <param name="v">the value</param>
    public KeyValuePair(string k, string v)
    {
      _key = k;
      _value = v;
    }

    /// <summary>
    ///   Gets the pair's key.
    /// </summary>
    public string Key { get { return _key; } }

    /// <summary>
    ///   Gets the pair's value.
    /// </summary>
    public string Value { get { return _value; } }

    /// <summary>
    ///   Determines if the pair is equal to another object.
    /// </summary>
    /// <param name="obj">the other object</param>
    /// <returns>
    ///   <em>true</em> if equal; otherwise <em>false</em>
    /// </returns>
    public override bool Equals(object obj)
    {
      return obj is KeyValuePair
             && Equals((KeyValuePair)obj);
    }

    /// <summary>
    ///   Calculates the pair's hashcode.
    /// </summary>
    /// <returns></returns>
    public override int GetHashCode()
    {
      const int prime = Constants.NotSoRandomPrime; // a random prime

      var result = CHashCodeSeed * prime;
      if (_key != null)
      {
        result ^= prime * _key.GetHashCode();
      }
      if (_value != null)
      {
        result ^= prime * _value.GetHashCode();
      }
      return result;
    }

    /// <summary>
    ///   Converts the pair to a string representation.
    /// </summary>
    /// <returns></returns>
    public override string ToString() { return ToString('='); }

    /// <summary>
    ///   Determines if the pair is equal to another.
    /// </summary>
    /// <param name="other">the other</param>
    /// <returns>
    ///   <em>true</em> if equal; otherwise <em>false</em>
    /// </returns>
    public bool Equals(KeyValuePair other)
    {
      return String.Equals(_key, other._key)
             && String.Equals(_value, other._value);
    }

    /// <summary>
    ///   Converts the pair to a string representation using the given separator.
    /// </summary>
    /// <param name="sep">a separator character</param>
    /// <returns></returns>
    public string ToString(char sep) { return String.Concat(_key, sep, _value); }

    /// <summary>
    ///   Determines if two pairs are equal.
    /// </summary>
    /// <param name="lhs">left hand operand</param>
    /// <param name="rhs">right hand operand</param>
    /// <returns>
    ///   <em>true</em> if equal; otherwise <em>false</em>
    /// </returns>
    public static bool operator ==(KeyValuePair lhs, KeyValuePair rhs) { return lhs.Equals(rhs); }

    /// <summary>
    ///   Determines if two pairs are unequal.
    /// </summary>
    /// <param name="lhs">left hand operand</param>
    /// <param name="rhs">right hand operand</param>
    /// <returns>
    ///   <em>true</em> if unequal; otherwise <em>false</em>
    /// </returns>
    public static bool operator !=(KeyValuePair lhs, KeyValuePair rhs) { return !lhs.Equals(rhs); }

    /// <summary>
    ///   Tries to parse a key-value-pair from an input string.
    /// </summary>
    /// <param name="input">the input</param>
    /// <param name="sep">string separating the key from the value</param>
    /// <param name="kvp">reference to a variable where the pair will be returned upon success</param>
    /// <returns>
    ///   <em>true</em> if successful; otherwise <em>false</em>
    /// </returns>
    internal static bool TryParse(string input, string sep, out KeyValuePair kvp)
    {
      if (input != null)
      {
        var i = input.IndexOf(sep, StringComparison.Ordinal);
        if (i >= 0)
        {
          kvp = new KeyValuePair(input.Substring(0, i), input.Substring(i + sep.Length));
          return true;
        }
      }

      kvp = default(KeyValuePair);
      return false;
    }

    /// <summary>
    ///   Tries to parse a key-value-pair from an input string.
    /// </summary>
    /// <param name="input">the input</param>
    /// <param name="sep">character separating the key from the value</param>
    /// <param name="kvp">reference to a variable where the pair will be returned upon success</param>
    /// <returns>
    ///   <em>true</em> if successful; otherwise <em>false</em>
    /// </returns>
    internal static bool TryParse(string input, char sep, out KeyValuePair kvp)
    {
      var i = input.IndexOf(sep);
      if (i >= 0)
      {
        kvp = new KeyValuePair(input.Substring(0, i), input.Substring(i + 1));
        return true;
      }

      kvp = default(KeyValuePair);
      return false;
    }
  }
}