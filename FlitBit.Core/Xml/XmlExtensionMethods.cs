﻿#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Xml.Linq;

namespace FlitBit.Core.Xml
{
  /// <summary>
  ///   Extensions for working with XElement and XML
  /// </summary>
  public static class XElementExtensions
  {
    /// <summary>
    ///   Reads a named value from an xml element.
    /// </summary>
    /// <param name="element">element</param>
    /// <param name="name">name</param>
    /// <returns>the value</returns>
    [SuppressMessage("Microsoft.Design", "CA1011", Justification = "By design.")]
    public static bool ReadBooleanOrDefault(this XElement element, string name)
    {
      bool value;
      ReadNamedValueOrDefault(element, name, out value);
      return value;
    }

    /// <summary>
    ///   Reads a named byte value from an xml element.
    /// </summary>
    /// <param name="element">element</param>
    /// <param name="name">name</param>
    /// <returns>the value</returns>
    [SuppressMessage("Microsoft.Design", "CA1011", Justification = "By design.")]
    public static byte ReadByteOrDefault(this XElement element, string name)
    {
      byte value;
      ReadNamedValueOrDefault(element, name, out value);
      return value;
    }

    /// <summary>
    ///   Reads a named byte array value from an xml element.
    /// </summary>
    /// <param name="element">element</param>
    /// <param name="name">name</param>
    /// <returns>the value</returns>
    [SuppressMessage("Microsoft.Design", "CA1011", Justification = "By design.")]
    public static byte[] ReadBytesOrDefault(this XElement element, string name)
    {
      byte[] value;
      ReadNamedValueOrDefault(element, name, out value);
      return value;
    }

    /// <summary>
    ///   Reads a named char value from an xml element.
    /// </summary>
    /// <param name="element">element</param>
    /// <param name="name">name</param>
    /// <returns>the value</returns>
    [SuppressMessage("Microsoft.Design", "CA1011", Justification = "By design.")]
    public static char ReadCharOrDefault(this XElement element, string name)
    {
      char value;
      ReadNamedValueOrDefault(element, name, out value);
      return value;
    }

    /// <summary>
    ///   Reads a named decimal value from an xml element.
    /// </summary>
    /// <param name="element">element</param>
    /// <param name="name">name</param>
    /// <returns>the value</returns>
    [SuppressMessage("Microsoft.Design", "CA1011", Justification = "By design.")]
    public static decimal ReadDecimalOrDefault(this XElement element, string name)
    {
      decimal value;
      ReadNamedValueOrDefault(element, name, out value);
      return value;
    }

    /// <summary>
    ///   Reads a named double value from an xml element.
    /// </summary>
    /// <param name="element">element</param>
    /// <param name="name">name</param>
    /// <returns>the value</returns>
    [SuppressMessage("Microsoft.Design", "CA1011", Justification = "By design.")]
    public static double ReadDoubleOrDefault(this XElement element, string name)
    {
      double value;
      ReadNamedValueOrDefault(element, name, out value);
      return value;
    }

    /// <summary>
    ///   Reads a named Guid value from an xml element.
    /// </summary>
    /// <param name="element">element</param>
    /// <param name="name">name</param>
    /// <returns>the value</returns>
    [SuppressMessage("Microsoft.Design", "CA1011", Justification = "By design.")]
    public static Guid ReadGuidOrDefault(this XElement element, string name)
    {
      Guid value;
      ReadNamedValueOrDefault(element, name, out value);
      return value;
    }

    /// <summary>
    ///   Reads a named Int16 value from an xml element.
    /// </summary>
    /// <param name="element">element</param>
    /// <param name="name">name</param>
    /// <returns>the value</returns>
    [SuppressMessage("Microsoft.Design", "CA1011", Justification = "By design.")]
    public static short ReadInt16OrDefault(this XElement element, string name)
    {
      short value;
      ReadNamedValueOrDefault(element, name, out value);
      return value;
    }

    /// <summary>
    ///   Reads a named Int32 value from an xml element.
    /// </summary>
    /// <param name="element">element</param>
    /// <param name="name">name</param>
    /// <returns>the value</returns>
    [SuppressMessage("Microsoft.Design", "CA1011", Justification = "By design.")]
    public static int ReadInt32OrDefault(this XElement element, string name)
    {
      int value;
      ReadNamedValueOrDefault(element, name, out value);
      return value;
    }

    /// <summary>
    ///   Reads a named Int64 value from an xml element.
    /// </summary>
    /// <param name="element">element</param>
    /// <param name="name">name</param>
    /// <returns>the value</returns>
    [SuppressMessage("Microsoft.Design", "CA1011", Justification = "By design.")]
    public static long ReadInt64OrDefault(this XElement element, string name)
    {
      long value;
      ReadNamedValueOrDefault(element, name, out value);
      return value;
    }

    /// <summary>
    ///   Read a named value from an xml element; if the value doesn't exist, value is set to
    ///   the default.
    /// </summary>
    /// <param name="element">element</param>
    /// <param name="name">name</param>
    /// <param name="value">reference to a variable that will receive the value upon success.</param>
    [SuppressMessage("Microsoft.Design", "CA1011", Justification = "By design.")]
    public static void ReadNamedValueOrDefault(this XElement element, string name, out bool value)
    {
      TryReadNamedValue(element, name, out value);
    }

    /// <summary>
    ///   Read a named value from an xml element; if the value doesn't exist, value is set to
    ///   the default given.
    /// </summary>
    /// <param name="element">element</param>
    /// <param name="name">name</param>
    /// <param name="value">reference to a variable that will receive the value upon success.</param>
    /// <param name="defa">default value used if no value is present on the element</param>
    [SuppressMessage("Microsoft.Design", "CA1011", Justification = "By design.")]
    public static void ReadNamedValueOrDefault(this XElement element, string name, out bool value, bool defa)
    {
      if (!TryReadNamedValue(element, name, out value))
      {
        value = defa;
      }
    }

    /// <summary>
    ///   Read a named value from an xml element; if the value doesn't exist, value is set to
    ///   the default given.
    /// </summary>
    /// <param name="element">element</param>
    /// <param name="name">name</param>
    /// <param name="value">reference to a variable that will receive the value upon success.</param>
    /// <param name="defa">default value used if no value is present on the element</param>
    [SuppressMessage("Microsoft.Design", "CA1011", Justification = "By design.")]
    public static void ReadNamedValueOrDefault(this XElement element, string name, out bool value, Func<bool> defa)
    {
      Contract.Requires(defa != null);

      if (!TryReadNamedValue(element, name, out value))
      {
        value = defa();
      }
    }

    /// <summary>
    ///   Read a named value from an xml element; if the value doesn't exist, value is set to
    ///   the default.
    /// </summary>
    /// <param name="element">element</param>
    /// <param name="name">name</param>
    /// <param name="value">reference to a variable that will receive the value upon success.</param>
    [SuppressMessage("Microsoft.Design", "CA1011", Justification = "By design.")]
    public static void ReadNamedValueOrDefault(this XElement element, string name, out byte value)
    {
      TryReadNamedValue(element, name, out value);
    }

    /// <summary>
    ///   Read a named value from an xml element; if the value doesn't exist, value is set to
    ///   the default given.
    /// </summary>
    /// <param name="element">element</param>
    /// <param name="name">name</param>
    /// <param name="value">reference to a variable that will receive the value upon success.</param>
    /// <param name="defa">default value used if no value is present on the element</param>
    [SuppressMessage("Microsoft.Design", "CA1011", Justification = "By design.")]
    public static void ReadNamedValueOrDefault(this XElement element, string name, out byte value, byte defa)
    {
      if (!TryReadNamedValue(element, name, out value))
      {
        value = defa;
      }
    }

    /// <summary>
    ///   Read a named value from an xml element; if the value doesn't exist, value is set to
    ///   the default given.
    /// </summary>
    /// <param name="element">element</param>
    /// <param name="name">name</param>
    /// <param name="value">reference to a variable that will receive the value upon success.</param>
    /// <param name="defa">default value used if no value is present on the element</param>
    [SuppressMessage("Microsoft.Design", "CA1011", Justification = "By design.")]
    public static void ReadNamedValueOrDefault(this XElement element, string name, out byte value, Func<byte> defa)
    {
      Contract.Requires(defa != null);

      if (!TryReadNamedValue(element, name, out value))
      {
        value = defa();
      }
    }

    /// <summary>
    ///   Read a named value from an xml element; if the value doesn't exist, value is set to
    ///   the default.
    /// </summary>
    /// <param name="element">element</param>
    /// <param name="name">name</param>
    /// <param name="value">reference to a variable that will receive the value upon success.</param>
    [SuppressMessage("Microsoft.Design", "CA1011", Justification = "By design.")]
    public static void ReadNamedValueOrDefault(this XElement element, string name, out char value)
    {
      TryReadNamedValue(element, name, out value);
    }

    /// <summary>
    ///   Read a named value from an xml element; if the value doesn't exist, value is set to
    ///   the default given.
    /// </summary>
    /// <param name="element">element</param>
    /// <param name="name">name</param>
    /// <param name="value">reference to a variable that will receive the value upon success.</param>
    /// <param name="defa">default value used if no value is present on the element</param>
    [SuppressMessage("Microsoft.Design", "CA1011", Justification = "By design.")]
    public static void ReadNamedValueOrDefault(this XElement element, string name, out char value, char defa)
    {
      if (!TryReadNamedValue(element, name, out value))
      {
        value = defa;
      }
    }

    /// <summary>
    ///   Read a named value from an xml element; if the value doesn't exist, value is set to
    ///   the default given.
    /// </summary>
    /// <param name="element">element</param>
    /// <param name="name">name</param>
    /// <param name="value">reference to a variable that will receive the value upon success.</param>
    /// <param name="defa">default value used if no value is present on the element</param>
    [SuppressMessage("Microsoft.Design", "CA1011", Justification = "By design.")]
    public static void ReadNamedValueOrDefault(this XElement element, string name, out char value, Func<char> defa)
    {
      Contract.Requires(defa != null);

      if (!TryReadNamedValue(element, name, out value))
      {
        value = defa();
      }
    }

    /// <summary>
    ///   Read a named value from an xml element; if the value doesn't exist, value is set to
    ///   the default.
    /// </summary>
    /// <param name="element">element</param>
    /// <param name="name">name</param>
    /// <param name="value">reference to a variable that will receive the value upon success.</param>
    [SuppressMessage("Microsoft.Design", "CA1011", Justification = "By design.")]
    public static void ReadNamedValueOrDefault(this XElement element, string name, out DateTime value)
    {
      TryReadNamedValue(element, name, out value);
    }

    /// <summary>
    ///   Read a named value from an xml element; if the value doesn't exist, value is set to
    ///   the default given.
    /// </summary>
    /// <param name="element">element</param>
    /// <param name="name">name</param>
    /// <param name="value">reference to a variable that will receive the value upon success.</param>
    /// <param name="defa">default value used if no value is present on the element</param>
    [SuppressMessage("Microsoft.Design", "CA1011", Justification = "By design.")]
    public static void ReadNamedValueOrDefault(this XElement element, string name, out DateTime value, DateTime defa)
    {
      if (!TryReadNamedValue(element, name, out value))
      {
        value = defa;
      }
    }

    /// <summary>
    ///   Read a named value from an xml element; if the value doesn't exist, value is set to
    ///   the default given.
    /// </summary>
    /// <param name="element">element</param>
    /// <param name="name">name</param>
    /// <param name="value">reference to a variable that will receive the value upon success.</param>
    /// <param name="defa">default value used if no value is present on the element</param>
    [SuppressMessage("Microsoft.Design", "CA1011", Justification = "By design.")]
    public static void ReadNamedValueOrDefault(this XElement element, string name, out DateTime value,
      Func<DateTime> defa)
    {
      Contract.Requires(defa != null);

      if (!TryReadNamedValue(element, name, out value))
      {
        value = defa();
      }
    }

    /// <summary>
    ///   Read a named value from an xml element; if the value doesn't exist, value is set to
    ///   the default.
    /// </summary>
    /// <param name="element">element</param>
    /// <param name="name">name</param>
    /// <param name="value">reference to a variable that will receive the value upon success.</param>
    [SuppressMessage("Microsoft.Design", "CA1011", Justification = "By design.")]
    public static void ReadNamedValueOrDefault(this XElement element, string name, out decimal value)
    {
      TryReadNamedValue(element, name, out value);
    }

    /// <summary>
    ///   Read a named value from an xml element; if the value doesn't exist, value is set to
    ///   the default given.
    /// </summary>
    /// <param name="element">element</param>
    /// <param name="name">name</param>
    /// <param name="value">reference to a variable that will receive the value upon success.</param>
    /// <param name="defa">default value used if no value is present on the element</param>
    [SuppressMessage("Microsoft.Design", "CA1011", Justification = "By design.")]
    public static void ReadNamedValueOrDefault(this XElement element, string name, out decimal value, decimal defa)
    {
      if (!TryReadNamedValue(element, name, out value))
      {
        value = defa;
      }
    }

    /// <summary>
    ///   Read a named value from an xml element; if the value doesn't exist, value is set to
    ///   the default given.
    /// </summary>
    /// <param name="element">element</param>
    /// <param name="name">name</param>
    /// <param name="value">reference to a variable that will receive the value upon success.</param>
    /// <param name="defa">default value used if no value is present on the element</param>
    [SuppressMessage("Microsoft.Design", "CA1011", Justification = "By design.")]
    public static void ReadNamedValueOrDefault(this XElement element, string name, out decimal value, Func<decimal> defa)
    {
      Contract.Requires(defa != null);

      if (!TryReadNamedValue(element, name, out value))
      {
        value = defa();
      }
    }

    /// <summary>
    ///   Read a named value from an xml element; if the value doesn't exist, value is set to
    ///   the default.
    /// </summary>
    /// <param name="element">element</param>
    /// <param name="name">name</param>
    /// <param name="value">reference to a variable that will receive the value upon success.</param>
    [SuppressMessage("Microsoft.Design", "CA1011", Justification = "By design.")]
    public static void ReadNamedValueOrDefault(this XElement element, string name, out double value)
    {
      TryReadNamedValue(element, name, out value);
    }

    /// <summary>
    ///   Read a named value from an xml element; if the value doesn't exist, value is set to
    ///   the default given.
    /// </summary>
    /// <param name="element">element</param>
    /// <param name="name">name</param>
    /// <param name="value">reference to a variable that will receive the value upon success.</param>
    /// <param name="defa">default value used if no value is present on the element</param>
    [SuppressMessage("Microsoft.Design", "CA1011", Justification = "By design.")]
    public static void ReadNamedValueOrDefault(this XElement element, string name, out double value, double defa)
    {
      if (!TryReadNamedValue(element, name, out value))
      {
        value = defa;
      }
    }

    /// <summary>
    ///   Read a named value from an xml element; if the value doesn't exist, value is set to
    ///   the default given.
    /// </summary>
    /// <param name="element">element</param>
    /// <param name="name">name</param>
    /// <param name="value">reference to a variable that will receive the value upon success.</param>
    /// <param name="defa">default value used if no value is present on the element</param>
    [SuppressMessage("Microsoft.Design", "CA1011", Justification = "By design.")]
    public static void ReadNamedValueOrDefault(this XElement element, string name, out double value, Func<double> defa)
    {
      Contract.Requires(defa != null);

      if (!TryReadNamedValue(element, name, out value))
      {
        value = defa();
      }
    }

    /// <summary>
    ///   Read a named value from an xml element; if the value doesn't exist, value is set to
    ///   the default.
    /// </summary>
    /// <param name="element">element</param>
    /// <param name="name">name</param>
    /// <param name="value">reference to a variable that will receive the value upon success.</param>
    [SuppressMessage("Microsoft.Design", "CA1011", Justification = "By design.")]
    public static void ReadNamedValueOrDefault(this XElement element, string name, out short value)
    {
      TryReadNamedValue(element, name, out value);
    }

    /// <summary>
    ///   Read a named value from an xml element; if the value doesn't exist, value is set to
    ///   the default given.
    /// </summary>
    /// <param name="element">element</param>
    /// <param name="name">name</param>
    /// <param name="value">reference to a variable that will receive the value upon success.</param>
    /// <param name="defa">default value used if no value is present on the element</param>
    [SuppressMessage("Microsoft.Design", "CA1011", Justification = "By design.")]
    public static void ReadNamedValueOrDefault(this XElement element, string name, out short value, short defa)
    {
      if (!TryReadNamedValue(element, name, out value))
      {
        value = defa;
      }
    }

    /// <summary>
    ///   Read a named value from an xml element; if the value doesn't exist, value is set to
    ///   the default given.
    /// </summary>
    /// <param name="element">element</param>
    /// <param name="name">name</param>
    /// <param name="value">reference to a variable that will receive the value upon success.</param>
    /// <param name="defa">default value used if no value is present on the element</param>
    [SuppressMessage("Microsoft.Design", "CA1011", Justification = "By design.")]
    public static void ReadNamedValueOrDefault(this XElement element, string name, out short value, Func<short> defa)
    {
      Contract.Requires(defa != null);

      if (!TryReadNamedValue(element, name, out value))
      {
        value = defa();
      }
    }

    /// <summary>
    ///   Read a named value from an xml element; if the value doesn't exist, value is set to
    ///   the default.
    /// </summary>
    /// <param name="element">element</param>
    /// <param name="name">name</param>
    /// <param name="value">reference to a variable that will receive the value upon success.</param>
    [SuppressMessage("Microsoft.Design", "CA1011", Justification = "By design.")]
    public static void ReadNamedValueOrDefault(this XElement element, string name, out int value)
    {
      TryReadNamedValue(element, name, out value);
    }

    /// <summary>
    ///   Read a named value from an xml element; if the value doesn't exist, value is set to
    ///   the default given.
    /// </summary>
    /// <param name="element">element</param>
    /// <param name="name">name</param>
    /// <param name="value">reference to a variable that will receive the value upon success.</param>
    /// <param name="defa">default value used if no value is present on the element</param>
    [SuppressMessage("Microsoft.Design", "CA1011", Justification = "By design.")]
    public static void ReadNamedValueOrDefault(this XElement element, string name, out int value, int defa)
    {
      if (!TryReadNamedValue(element, name, out value))
      {
        value = defa;
      }
    }

    /// <summary>
    ///   Read a named value from an xml element; if the value doesn't exist, value is set to
    ///   the default given.
    /// </summary>
    /// <param name="element">element</param>
    /// <param name="name">name</param>
    /// <param name="value">reference to a variable that will receive the value upon success.</param>
    /// <param name="defa">default value used if no value is present on the element</param>
    [SuppressMessage("Microsoft.Design", "CA1011", Justification = "By design.")]
    public static void ReadNamedValueOrDefault(this XElement element, string name, out int value, Func<int> defa)
    {
      Contract.Requires(defa != null);

      if (!TryReadNamedValue(element, name, out value))
      {
        value = defa();
      }
    }

    /// <summary>
    ///   Read a named value from an xml element; if the value doesn't exist, value is set to
    ///   the default.
    /// </summary>
    /// <param name="element">element</param>
    /// <param name="name">name</param>
    /// <param name="value">reference to a variable that will receive the value upon success.</param>
    [SuppressMessage("Microsoft.Design", "CA1011", Justification = "By design.")]
    public static void ReadNamedValueOrDefault(this XElement element, string name, out long value)
    {
      TryReadNamedValue(element, name, out value);
    }

    /// <summary>
    ///   Read a named value from an xml element; if the value doesn't exist, value is set to
    ///   the default given.
    /// </summary>
    /// <param name="element">element</param>
    /// <param name="name">name</param>
    /// <param name="value">reference to a variable that will receive the value upon success.</param>
    /// <param name="defa">default value used if no value is present on the element</param>
    [SuppressMessage("Microsoft.Design", "CA1011", Justification = "By design.")]
    public static void ReadNamedValueOrDefault(this XElement element, string name, out long value, long defa)
    {
      if (!TryReadNamedValue(element, name, out value))
      {
        value = defa;
      }
    }

    /// <summary>
    ///   Read a named value from an xml element; if the value doesn't exist, value is set to
    ///   the default given.
    /// </summary>
    /// <param name="element">element</param>
    /// <param name="name">name</param>
    /// <param name="value">reference to a variable that will receive the value upon success.</param>
    /// <param name="defa">default value used if no value is present on the element</param>
    [SuppressMessage("Microsoft.Design", "CA1011", Justification = "By design.")]
    public static void ReadNamedValueOrDefault(this XElement element, string name, out long value, Func<long> defa)
    {
      Contract.Requires(defa != null);

      if (!TryReadNamedValue(element, name, out value))
      {
        value = defa();
      }
    }

    /// <summary>
    ///   Read a named value from an xml element; if the value doesn't exist, value is set to
    ///   the default.
    /// </summary>
    /// <param name="element">element</param>
    /// <param name="name">name</param>
    /// <param name="value">reference to a variable that will receive the value upon success.</param>
    [SuppressMessage("Microsoft.Design", "CA1011", Justification = "By design.")]
    public static void ReadNamedValueOrDefault<T>(this XElement element, string name, out T value)
    {
      TryReadNamedValue(element, name, out value);
    }

    /// <summary>
    ///   Read a named value from an xml element; if the value doesn't exist, value is set to
    ///   the default given.
    /// </summary>
    /// <param name="element">element</param>
    /// <param name="name">name</param>
    /// <param name="value">reference to a variable that will receive the value upon success.</param>
    /// <param name="defa">default value used if no value is present on the element</param>
    [SuppressMessage("Microsoft.Design", "CA1011", Justification = "By design.")]
    public static void ReadNamedValueOrDefault<T>(this XElement element, string name, out T value, T defa)
    {
      if (!TryReadNamedValue(element, name, out value))
      {
        value = defa;
      }
    }

    /// <summary>
    ///   Read a named value from an xml element; if the value doesn't exist, value is set to
    ///   the default given.
    /// </summary>
    /// <param name="element">element</param>
    /// <param name="name">name</param>
    /// <param name="value">reference to a variable that will receive the value upon success.</param>
    /// <param name="defa">default value used if no value is present on the element</param>
    [SuppressMessage("Microsoft.Design", "CA1011", Justification = "By design.")]
    public static void ReadNamedValueOrDefault<T>(this XElement element, string name, out T value, Func<T> defa)
    {
      Contract.Requires(defa != null);

      if (!TryReadNamedValue(element, name, out value))
      {
        value = defa();
      }
    }

    /// <summary>
    ///   Read a named value from an xml element; if the value doesn't exist, value is set to
    ///   the default.
    /// </summary>
    /// <param name="element">element</param>
    /// <param name="name">name</param>
    /// <param name="value">reference to a variable that will receive the value upon success.</param>
    [CLSCompliant(false), SuppressMessage("Microsoft.Design", "CA1011", Justification = "By design.")]
    public static void ReadNamedValueOrDefault(this XElement element, string name, out sbyte value)
    {
      TryReadNamedValue(element, name, out value);
    }

    /// <summary>
    ///   Read a named value from an xml element; if the value doesn't exist, value is set to
    ///   the default given.
    /// </summary>
    /// <param name="element">element</param>
    /// <param name="name">name</param>
    /// <param name="value">reference to a variable that will receive the value upon success.</param>
    /// <param name="defa">default value used if no value is present on the element</param>
    [CLSCompliant(false), SuppressMessage("Microsoft.Design", "CA1011", Justification = "By design.")]
    public static void ReadNamedValueOrDefault(this XElement element, string name, out sbyte value, sbyte defa)
    {
      if (!TryReadNamedValue(element, name, out value))
      {
        value = defa;
      }
    }

    /// <summary>
    ///   Read a named value from an xml element; if the value doesn't exist, value is set to
    ///   the default given.
    /// </summary>
    /// <param name="element">element</param>
    /// <param name="name">name</param>
    /// <param name="value">reference to a variable that will receive the value upon success.</param>
    /// <param name="defa">default value used if no value is present on the element</param>
    [CLSCompliant(false), SuppressMessage("Microsoft.Design", "CA1011", Justification = "By design.")]
    public static void ReadNamedValueOrDefault(this XElement element, string name, out sbyte value, Func<sbyte> defa)
    {
      Contract.Requires(defa != null);

      if (!TryReadNamedValue(element, name, out value))
      {
        value = defa();
      }
    }

    /// <summary>
    ///   Read a named value from an xml element; if the value doesn't exist, value is set to
    ///   the default.
    /// </summary>
    /// <param name="element">element</param>
    /// <param name="name">name</param>
    /// <param name="value">reference to a variable that will receive the value upon success.</param>
    [SuppressMessage("Microsoft.Design", "CA1011", Justification = "By design.")]
    public static void ReadNamedValueOrDefault(this XElement element, string name, out float value)
    {
      TryReadNamedValue(element, name, out value);
    }

    /// <summary>
    ///   Read a named value from an xml element; if the value doesn't exist, value is set to
    ///   the default given.
    /// </summary>
    /// <param name="element">element</param>
    /// <param name="name">name</param>
    /// <param name="value">reference to a variable that will receive the value upon success.</param>
    /// <param name="defa">default value used if no value is present on the element</param>
    [SuppressMessage("Microsoft.Design", "CA1011", Justification = "By design.")]
    public static void ReadNamedValueOrDefault(this XElement element, string name, out float value, float defa)
    {
      if (!TryReadNamedValue(element, name, out value))
      {
        value = defa;
      }
    }

    /// <summary>
    ///   Read a named value from an xml element; if the value doesn't exist, value is set to
    ///   the default given.
    /// </summary>
    /// <param name="element">element</param>
    /// <param name="name">name</param>
    /// <param name="value">reference to a variable that will receive the value upon success.</param>
    /// <param name="defa">default value used if no value is present on the element</param>
    [SuppressMessage("Microsoft.Design", "CA1011", Justification = "By design.")]
    public static void ReadNamedValueOrDefault(this XElement element, string name, out float value, Func<float> defa)
    {
      Contract.Requires(defa != null);

      if (!TryReadNamedValue(element, name, out value))
      {
        value = defa();
      }
    }

    /// <summary>
    ///   Read a named value from an xml element; if the value doesn't exist, value is set to
    ///   the default.
    /// </summary>
    /// <param name="element">element</param>
    /// <param name="name">name</param>
    /// <param name="value">reference to a variable that will receive the value upon success.</param>
    [SuppressMessage("Microsoft.Design", "CA1011", Justification = "By design.")]
    public static void ReadNamedValueOrDefault(this XElement element, string name, out string value)
    {
      TryReadNamedValue(element, name, out value);
    }

    /// <summary>
    ///   Read a named value from an xml element; if the value doesn't exist, value is set to
    ///   the default given.
    /// </summary>
    /// <param name="element">element</param>
    /// <param name="name">name</param>
    /// <param name="value">reference to a variable that will receive the value upon success.</param>
    /// <param name="defa">default value used if no value is present on the element</param>
    [SuppressMessage("Microsoft.Design", "CA1011", Justification = "By design.")]
    public static void ReadNamedValueOrDefault(this XElement element, string name, out string value, string defa)
    {
      if (!TryReadNamedValue(element, name, out value))
      {
        value = defa;
      }
    }

    /// <summary>
    ///   Read a named value from an xml element; if the value doesn't exist, value is set to
    ///   the default given.
    /// </summary>
    /// <param name="element">element</param>
    /// <param name="name">name</param>
    /// <param name="value">reference to a variable that will receive the value upon success.</param>
    /// <param name="defa">default value used if no value is present on the element</param>
    [SuppressMessage("Microsoft.Design", "CA1011", Justification = "By design.")]
    public static void ReadNamedValueOrDefault(this XElement element, string name, out string value, Func<string> defa)
    {
      Contract.Requires(defa != null);

      if (!TryReadNamedValue(element, name, out value))
      {
        value = defa();
      }
    }

    /// <summary>
    ///   Read a named value from an xml element; if the value doesn't exist, value is set to
    ///   the default.
    /// </summary>
    /// <param name="element">element</param>
    /// <param name="name">name</param>
    /// <param name="value">reference to a variable that will receive the value upon success.</param>
    [CLSCompliant(false), SuppressMessage("Microsoft.Design", "CA1011", Justification = "By design.")]
    public static void ReadNamedValueOrDefault(this XElement element, string name, out ushort value)
    {
      TryReadNamedValue(element, name, out value);
    }

    /// <summary>
    ///   Read a named value from an xml element; if the value doesn't exist, value is set to
    ///   the default given.
    /// </summary>
    /// <param name="element">element</param>
    /// <param name="name">name</param>
    /// <param name="value">reference to a variable that will receive the value upon success.</param>
    /// <param name="defa">default value used if no value is present on the element</param>
    [CLSCompliant(false), SuppressMessage("Microsoft.Design", "CA1011", Justification = "By design.")]
    public static void ReadNamedValueOrDefault(this XElement element, string name, out ushort value, ushort defa)
    {
      if (!TryReadNamedValue(element, name, out value))
      {
        value = defa;
      }
    }

    /// <summary>
    ///   Read a named value from an xml element; if the value doesn't exist, value is set to
    ///   the default given.
    /// </summary>
    /// <param name="element">element</param>
    /// <param name="name">name</param>
    /// <param name="value">reference to a variable that will receive the value upon success.</param>
    /// <param name="defa">default value used if no value is present on the element</param>
    [CLSCompliant(false), SuppressMessage("Microsoft.Design", "CA1011", Justification = "By design.")]
    public static void ReadNamedValueOrDefault(this XElement element, string name, out ushort value, Func<ushort> defa)
    {
      Contract.Requires(defa != null);

      if (!TryReadNamedValue(element, name, out value))
      {
        value = defa();
      }
    }

    /// <summary>
    ///   Read a named value from an xml element; if the value doesn't exist, value is set to
    ///   the default.
    /// </summary>
    /// <param name="element">element</param>
    /// <param name="name">name</param>
    /// <param name="value">reference to a variable that will receive the value upon success.</param>
    [CLSCompliant(false), SuppressMessage("Microsoft.Design", "CA1011", Justification = "By design.")]
    public static void ReadNamedValueOrDefault(this XElement element, string name, out uint value)
    {
      TryReadNamedValue(element, name, out value);
    }

    /// <summary>
    ///   Read a named value from an xml element; if the value doesn't exist, value is set to
    ///   the default given.
    /// </summary>
    /// <param name="element">element</param>
    /// <param name="name">name</param>
    /// <param name="value">reference to a variable that will receive the value upon success.</param>
    /// <param name="defa">default value used if no value is present on the element</param>
    [CLSCompliant(false), SuppressMessage("Microsoft.Design", "CA1011", Justification = "By design.")]
    public static void ReadNamedValueOrDefault(this XElement element, string name, out uint value, uint defa)
    {
      if (!TryReadNamedValue(element, name, out value))
      {
        value = defa;
      }
    }

    /// <summary>
    ///   Read a named value from an xml element; if the value doesn't exist, value is set to
    ///   the default given.
    /// </summary>
    /// <param name="element">element</param>
    /// <param name="name">name</param>
    /// <param name="value">reference to a variable that will receive the value upon success.</param>
    /// <param name="defa">default value used if no value is present on the element</param>
    [CLSCompliant(false), SuppressMessage("Microsoft.Design", "CA1011", Justification = "By design.")]
    public static void ReadNamedValueOrDefault(this XElement element, string name, out uint value, Func<uint> defa)
    {
      Contract.Requires(defa != null);

      if (!TryReadNamedValue(element, name, out value))
      {
        value = defa();
      }
    }

    /// <summary>
    ///   Read a named value from an xml element; if the value doesn't exist, value is set to
    ///   the default.
    /// </summary>
    /// <param name="element">element</param>
    /// <param name="name">name</param>
    /// <param name="value">reference to a variable that will receive the value upon success.</param>
    [CLSCompliant(false), SuppressMessage("Microsoft.Design", "CA1011", Justification = "By design.")]
    public static void ReadNamedValueOrDefault(this XElement element, string name, out ulong value)
    {
      TryReadNamedValue(element, name, out value);
    }

    /// <summary>
    ///   Read a named value from an xml element; if the value doesn't exist, value is set to
    ///   the default given.
    /// </summary>
    /// <param name="element">element</param>
    /// <param name="name">name</param>
    /// <param name="value">reference to a variable that will receive the value upon success.</param>
    /// <param name="defa">default value used if no value is present on the element</param>
    [CLSCompliant(false), SuppressMessage("Microsoft.Design", "CA1011", Justification = "By design.")]
    public static void ReadNamedValueOrDefault(this XElement element, string name, out ulong value, ulong defa)
    {
      if (!TryReadNamedValue(element, name, out value))
      {
        value = defa;
      }
    }

    /// <summary>
    ///   Read a named value from an xml element; if the value doesn't exist, value is set to
    ///   the default given.
    /// </summary>
    /// <param name="element">element</param>
    /// <param name="name">name</param>
    /// <param name="value">reference to a variable that will receive the value upon success.</param>
    /// <param name="defa">default value used if no value is present on the element</param>
    [CLSCompliant(false), SuppressMessage("Microsoft.Design", "CA1011", Justification = "By design.")]
    public static void ReadNamedValueOrDefault(this XElement element, string name, out ulong value, Func<ulong> defa)
    {
      Contract.Requires(defa != null);

      if (!TryReadNamedValue(element, name, out value))
      {
        value = defa();
      }
    }

    /// <summary>
    ///   Read a named value from an xml element; if the value doesn't exist, value is set to
    ///   the default.
    /// </summary>
    /// <param name="element">element</param>
    /// <param name="name">name</param>
    /// <param name="value">reference to a variable that will receive the value upon success.</param>
    [SuppressMessage("Microsoft.Design", "CA1011", Justification = "By design.")]
    public static void ReadNamedValueOrDefaultAsEnum<T>(this XElement element, string name, out T value)
    {
      TryReadNamedValueAsEnum(element, name, out value);
    }

    /// <summary>
    ///   Read a named value from an xml element; if the value doesn't exist, value is set to
    ///   the default given.
    /// </summary>
    /// <param name="element">element</param>
    /// <param name="name">name</param>
    /// <param name="value">reference to a variable that will receive the value upon success.</param>
    /// <param name="defa">default value used if no value is present on the element</param>
    [SuppressMessage("Microsoft.Design", "CA1011", Justification = "By design.")]
    public static void ReadNamedValueOrDefaultAsEnum<T>(this XElement element, string name, out T value, T defa)
    {
      if (!TryReadNamedValueAsEnum(element, name, out value))
      {
        value = defa;
      }
    }

    /// <summary>
    ///   Read a named value from an xml element; if the value doesn't exist, value is set to
    ///   the default given.
    /// </summary>
    /// <param name="element">element</param>
    /// <param name="name">name</param>
    /// <param name="value">reference to a variable that will receive the value upon success.</param>
    /// <param name="defa">default value used if no value is present on the element</param>
    [SuppressMessage("Microsoft.Design", "CA1011", Justification = "By design.")]
    public static void ReadNamedValueOrDefaultAsEnum<T>(this XElement element, string name, out T value, Func<T> defa)
    {
      Contract.Requires(defa != null);

      if (!TryReadNamedValueAsEnum(element, name, out value))
      {
        value = defa();
      }
    }

    /// <summary>
    ///   Reads a named SByte value from an xml element.
    /// </summary>
    /// <param name="element">element</param>
    /// <param name="name">name</param>
    /// <returns>the value</returns>
    [CLSCompliant(false)]
    public static sbyte ReadSByteOrDefault(this XElement element, string name)
    {
      sbyte value;
      ReadNamedValueOrDefault(element, name, out value);
      return value;
    }

    /// <summary>
    ///   Reads a named single value from an xml element.
    /// </summary>
    /// <param name="element">element</param>
    /// <param name="name">name</param>
    /// <returns>the value</returns>
    [SuppressMessage("Microsoft.Design", "CA1011", Justification = "By design.")]
    public static float ReadSingleOrDefault(this XElement element, string name)
    {
      float value;
      ReadNamedValueOrDefault(element, name, out value);
      return value;
    }

    /// <summary>
    ///   Reads a named String value from an xml element.
    /// </summary>
    /// <param name="element">element</param>
    /// <param name="name">name</param>
    /// <returns>the value</returns>
    [SuppressMessage("Microsoft.Design", "CA1011", Justification = "By design.")]
    public static string ReadStringOrDefault(this XElement element, string name)
    {
      string value;
      ReadNamedValueOrDefault(element, name, out value);
      return value;
    }

    /// <summary>
    ///   Reads a named UInt16 value from an xml element.
    /// </summary>
    /// <param name="element">element</param>
    /// <param name="name">name</param>
    /// <returns>the value</returns>
    [CLSCompliant(false), SuppressMessage("Microsoft.Design", "CA1011", Justification = "By design.")]
    public static ushort ReadUInt16OrDefault(this XElement element, string name)
    {
      ushort value;
      ReadNamedValueOrDefault(element, name, out value);
      return value;
    }

    /// <summary>
    ///   Reads a named UInt32 value from an xml element.
    /// </summary>
    /// <param name="element">element</param>
    /// <param name="name">name</param>
    /// <returns>the value</returns>
    [CLSCompliant(false)]
    public static uint ReadUInt32OrDefault(this XElement element, string name)
    {
      uint value;
      ReadNamedValueOrDefault(element, name, out value);
      return value;
    }

    /// <summary>
    ///   Reads a named UInt64 value from an xml element.
    /// </summary>
    /// <param name="element">element</param>
    /// <param name="name">name</param>
    /// <returns>the value</returns>
    [CLSCompliant(false), SuppressMessage("Microsoft.Design", "CA1011", Justification = "By design.")]
    public static ulong ReadUInt64OrDefault(this XElement element, string name)
    {
      ulong value;
      ReadNamedValueOrDefault(element, name, out value);
      return value;
    }

    /// <summary>
    ///   Converts an XElement into a dynamic XML object.
    /// </summary>
    /// <param name="xml">the source xml element</param>
    /// <returns>a dynamic object shaped according to the input xml</returns>
    public static dynamic ToDynamic(this XElement xml)
    {
      return XDynamic.ToDynamic(xml);
    }

    /// <summary>
    ///   Tries to read a named value from an xml element.
    /// </summary>
    /// <param name="element">element</param>
    /// <param name="name">name</param>
    /// <param name="value">reference to a variable that will receive the value upon success</param>
    /// <returns>
    ///   <em>true</em> if successful; otherwise <em>false</em>
    /// </returns>
    [SuppressMessage("Microsoft.Design", "CA1011", Justification = "By design.")]
    public static bool TryReadNamedValue(this XElement element, string name, out bool value)
    {
      Contract.Requires(element != null);
      Contract.Requires(name != null);
      Contract.Requires(name.Length > 0);

      var attr = element.Attribute(name);
      if (attr != null
          && attr.Value.Length > 0)
      {
        value = (bool)attr;
        return true;
      }
      var elm = element.Element(name);
      if (elm != null
          && elm.Value.Length > 0)
      {
        value = (bool)elm;
        return true;
      }
      value = default(bool);
      return false;
    }

    /// <summary>
    ///   Tries to read a named value from an xml element.
    /// </summary>
    /// <param name="element">element</param>
    /// <param name="name">name</param>
    /// <param name="value">reference to a variable that will receive the value upon success</param>
    /// <returns>
    ///   <em>true</em> if successful; otherwise <em>false</em>
    /// </returns>
    [SuppressMessage("Microsoft.Design", "CA1011", Justification = "By design.")]
    public static bool TryReadNamedValue(this XElement element, string name, out byte value)
    {
      Contract.Requires(element != null);
      Contract.Requires(name != null);
      Contract.Requires(name.Length > 0);

      var attr = element.Attribute(name);
      if (attr != null
          && attr.Value.Length > 0)
      {
        value = Convert.ToByte((int)attr);
        return true;
      }
      var elm = element.Element(name);
      if (elm != null
          && elm.Value.Length > 0)
      {
        value = Convert.ToByte((int)elm);
        return true;
      }
      value = default(byte);
      return false;
    }

    /// <summary>
    ///   Tries to read a named value from an xml element.
    /// </summary>
    /// <param name="element">element</param>
    /// <param name="name">name</param>
    /// <param name="value">reference to a variable that will receive the value upon success</param>
    /// <returns>
    ///   <em>true</em> if successful; otherwise <em>false</em>
    /// </returns>
    [SuppressMessage("Microsoft.Design", "CA1011", Justification = "By design.")]
    public static bool TryReadNamedValue(this XElement element, string name, out char value)
    {
      Contract.Requires(element != null);
      Contract.Requires(name != null);
      Contract.Requires(name.Length > 0);

      var attr = element.Attribute(name);
      if (attr != null
          && attr.Value.Length > 0)
      {
        value = Convert.ToChar((int)attr);
        return true;
      }
      var elm = element.Element(name);
      if (elm != null
          && elm.Value.Length > 0)
      {
        value = Convert.ToChar((int)elm);
        return true;
      }
      value = default(char);
      return false;
    }

    /// <summary>
    ///   Tries to read a named value from an xml element.
    /// </summary>
    /// <param name="element">element</param>
    /// <param name="name">name</param>
    /// <param name="value">reference to a variable that will receive the value upon success</param>
    /// <returns>
    ///   <em>true</em> if successful; otherwise <em>false</em>
    /// </returns>
    [SuppressMessage("Microsoft.Design", "CA1011", Justification = "By design.")]
    public static bool TryReadNamedValue(this XElement element, string name, out DateTime value)
    {
      Contract.Requires(element != null);
      Contract.Requires(name != null);
      Contract.Requires(name.Length > 0);

      var attr = element.Attribute(name);
      if (attr != null
          && attr.Value.Length > 0)
      {
        value = (DateTime)attr;
        return true;
      }
      var elm = element.Element(name);
      if (elm != null
          && elm.Value.Length > 0)
      {
        value = (DateTime)elm;
        return true;
      }
      value = default(DateTime);
      return false;
    }

    /// <summary>
    ///   Tries to read a named value from an xml element.
    /// </summary>
    /// <param name="element">element</param>
    /// <param name="name">name</param>
    /// <param name="value">reference to a variable that will receive the value upon success</param>
    /// <returns>
    ///   <em>true</em> if successful; otherwise <em>false</em>
    /// </returns>
    [SuppressMessage("Microsoft.Design", "CA1011", Justification = "By design.")]
    public static bool TryReadNamedValue(this XElement element, string name, out decimal value)
    {
      Contract.Requires(element != null);
      Contract.Requires(name != null);
      Contract.Requires(name.Length > 0);

      var attr = element.Attribute(name);
      if (attr != null
          && attr.Value.Length > 0)
      {
        value = (decimal)attr;
        return true;
      }
      var elm = element.Element(name);
      if (elm != null
          && elm.Value.Length > 0)
      {
        value = (decimal)elm;
        return true;
      }
      value = default(decimal);
      return false;
    }

    /// <summary>
    ///   Tries to read a named value from an xml element.
    /// </summary>
    /// <param name="element">element</param>
    /// <param name="name">name</param>
    /// <param name="value">reference to a variable that will receive the value upon success</param>
    /// <returns>
    ///   <em>true</em> if successful; otherwise <em>false</em>
    /// </returns>
    [SuppressMessage("Microsoft.Design", "CA1011", Justification = "By design.")]
    public static bool TryReadNamedValue(this XElement element, string name, out double value)
    {
      Contract.Requires(element != null);
      Contract.Requires(name != null);
      Contract.Requires(name.Length > 0);

      var attr = element.Attribute(name);
      if (attr != null
          && attr.Value.Length > 0)
      {
        value = (double)attr;
        return true;
      }
      var elm = element.Element(name);
      if (elm != null
          && elm.Value.Length > 0)
      {
        value = (double)elm;
        return true;
      }
      value = default(double);
      return false;
    }

    /// <summary>
    ///   Tries to read a named value from an xml element.
    /// </summary>
    /// <param name="element">element</param>
    /// <param name="name">name</param>
    /// <param name="value">reference to a variable that will receive the value upon success</param>
    /// <returns>
    ///   <em>true</em> if successful; otherwise <em>false</em>
    /// </returns>
    [SuppressMessage("Microsoft.Design", "CA1011", Justification = "By design.")]
    public static bool TryReadNamedValue(this XElement element, string name, out short value)
    {
      Contract.Requires(element != null);
      Contract.Requires(name != null);
      Contract.Requires(name.Length > 0);

      var attr = element.Attribute(name);
      if (attr != null
          && attr.Value.Length > 0)
      {
        value = (short)attr;
        return true;
      }
      var elm = element.Element(name);
      if (elm != null
          && elm.Value.Length > 0)
      {
        value = (short)elm;
        return true;
      }
      value = default(short);
      return false;
    }

    /// <summary>
    ///   Tries to read a named value from an xml element.
    /// </summary>
    /// <param name="element">element</param>
    /// <param name="name">name</param>
    /// <param name="value">reference to a variable that will receive the value upon success</param>
    /// <returns>
    ///   <em>true</em> if successful; otherwise <em>false</em>
    /// </returns>
    [SuppressMessage("Microsoft.Design", "CA1011", Justification = "By design.")]
    public static bool TryReadNamedValue(this XElement element, string name, out int value)
    {
      Contract.Requires(element != null);
      Contract.Requires(name != null);
      Contract.Requires(name.Length > 0);

      var attr = element.Attribute(name);
      if (attr != null
          && attr.Value.Length > 0)
      {
        value = (int)attr;
        return true;
      }
      var elm = element.Element(name);
      if (elm != null
          && elm.Value.Length > 0)
      {
        value = (int)elm;
        return true;
      }
      value = default(int);
      return false;
    }

    /// <summary>
    ///   Tries to read a named value from an xml element.
    /// </summary>
    /// <param name="element">element</param>
    /// <param name="name">name</param>
    /// <param name="value">reference to a variable that will receive the value upon success</param>
    /// <returns>
    ///   <em>true</em> if successful; otherwise <em>false</em>
    /// </returns>
    [SuppressMessage("Microsoft.Design", "CA1011", Justification = "By design.")]
    public static bool TryReadNamedValue(this XElement element, string name, out long value)
    {
      Contract.Requires(element != null);
      Contract.Requires(name != null);
      Contract.Requires(name.Length > 0);

      var attr = element.Attribute(name);
      if (attr != null
          && attr.Value.Length > 0)
      {
        value = (long)attr;
        return true;
      }
      var elm = element.Element(name);
      if (elm != null
          && elm.Value.Length > 0)
      {
        value = (long)elm;
        return true;
      }
      value = default(long);
      return false;
    }

    /// <summary>
    ///   Tries to read a named value from an xml container.
    /// </summary>
    /// <param name="element">element</param>
    /// <param name="name">name</param>
    /// <param name="value">reference to a variable that will receive the value upon success</param>
    /// <returns>
    ///   <em>true</em> if successful; otherwise <em>false</em>
    /// </returns>
    [SuppressMessage("Microsoft.Design", "CA1011", Justification = "By design.")]
    public static bool TryReadNamedValue<T>(this XContainer element, string name, out T value)
    {
      Contract.Requires(element != null);
      Contract.Requires(name != null);
      Contract.Requires(name.Length > 0);

      var elm = element.Element(name);
      if (elm != null
          && elm.Value.Length > 0)
      {
        throw new NotImplementedException();
        //value = DataTransfer.FromXml<T>(element.Element(name));
        //return true;
      }
      value = default(T);
      return false;
    }

    /// <summary>
    ///   Tries to read a named value from an xml element.
    /// </summary>
    /// <param name="element">element</param>
    /// <param name="name">name</param>
    /// <param name="value">reference to a variable that will receive the value upon success</param>
    /// <returns>
    ///   <em>true</em> if successful; otherwise <em>false</em>
    /// </returns>
    [CLSCompliant(false), SuppressMessage("Microsoft.Design", "CA1011", Justification = "By design.")]
    public static bool TryReadNamedValue(this XElement element, string name, out sbyte value)
    {
      Contract.Requires(element != null);
      Contract.Requires(name != null);
      Contract.Requires(name.Length > 0);

      var attr = element.Attribute(name);
      if (attr != null
          && attr.Value.Length > 0)
      {
        value = (sbyte)attr;
        return true;
      }
      var elm = element.Element(name);
      if (elm != null
          && elm.Value.Length > 0)
      {
        value = (sbyte)attr;
        return true;
      }
      value = default(sbyte);
      return false;
    }

    /// <summary>
    ///   Tries to read a named value from an xml element.
    /// </summary>
    /// <param name="element">element</param>
    /// <param name="name">name</param>
    /// <param name="value">reference to a variable that will receive the value upon success</param>
    /// <returns>
    ///   <em>true</em> if successful; otherwise <em>false</em>
    /// </returns>
    [SuppressMessage("Microsoft.Design", "CA1011", Justification = "By design.")]
    public static bool TryReadNamedValue(this XElement element, string name, out float value)
    {
      Contract.Requires(element != null);
      Contract.Requires(name != null);
      Contract.Requires(name.Length > 0);

      var attr = element.Attribute(name);
      if (attr != null
          && attr.Value.Length > 0)
      {
        value = (float)attr;
        return true;
      }
      var elm = element.Element(name);
      if (elm != null
          && elm.Value.Length > 0)
      {
        value = (float)attr;
        return true;
      }
      value = default(Single);
      return false;
    }

    /// <summary>
    ///   Tries to read a named value from an xml element.
    /// </summary>
    /// <param name="element">element</param>
    /// <param name="name">name</param>
    /// <param name="value">reference to a variable that will receive the value upon success</param>
    /// <returns>
    ///   <em>true</em> if successful; otherwise <em>false</em>
    /// </returns>
    [SuppressMessage("Microsoft.Design", "CA1011", Justification = "By design.")]
    public static bool TryReadNamedValue(this XElement element, string name, out string value)
    {
      Contract.Requires(element != null);
      Contract.Requires(name != null);
      Contract.Requires(name.Length > 0);

      var attr = element.Attribute(name);
      if (attr != null
          && attr.Value.Length > 0)
      {
        value = (string)attr;
        return true;
      }
      var elm = element.Element(name);
      if (elm != null
          && elm.Value.Length > 0)
      {
        value = (string)attr;
        return true;
      }
      value = default(String);
      return false;
    }

    /// <summary>
    ///   Tries to read a named value from an xml element.
    /// </summary>
    /// <param name="element">element</param>
    /// <param name="name">name</param>
    /// <param name="value">reference to a variable that will receive the value upon success</param>
    /// <returns>
    ///   <em>true</em> if successful; otherwise <em>false</em>
    /// </returns>
    [CLSCompliant(false), SuppressMessage("Microsoft.Design", "CA1011", Justification = "By design.")]
    public static bool TryReadNamedValue(this XElement element, string name, out ushort value)
    {
      Contract.Requires(element != null);
      Contract.Requires(name != null);
      Contract.Requires(name.Length > 0);

      var attr = element.Attribute(name);
      if (attr != null
          && attr.Value.Length > 0)
      {
        value = Convert.ToUInt16((int)attr);
        return true;
      }
      var elm = element.Element(name);
      if (elm != null
          && elm.Value.Length > 0)
      {
        value = Convert.ToUInt16((int)elm);
        return true;
      }
      value = default(ushort);
      return false;
    }

    /// <summary>
    ///   Tries to read a named value from an xml element.
    /// </summary>
    /// <param name="element">element</param>
    /// <param name="name">name</param>
    /// <param name="value">reference to a variable that will receive the value upon success</param>
    /// <returns>
    ///   <em>true</em> if successful; otherwise <em>false</em>
    /// </returns>
    [CLSCompliant(false), SuppressMessage("Microsoft.Design", "CA1011", Justification = "By design.")]
    public static bool TryReadNamedValue(this XElement element, string name, out uint value)
    {
      Contract.Requires(element != null);
      Contract.Requires(name != null);
      Contract.Requires(name.Length > 0);

      var attr = element.Attribute(name);
      if (attr != null
          && attr.Value.Length > 0)
      {
        value = (uint)attr;
        return true;
      }
      var elm = element.Element(name);
      if (elm != null
          && elm.Value.Length > 0)
      {
        value = (uint)attr;
        return true;
      }
      value = default(uint);
      return false;
    }

    /// <summary>
    ///   Tries to read a named value from an xml element.
    /// </summary>
    /// <param name="element">element</param>
    /// <param name="name">name</param>
    /// <param name="value">reference to a variable that will receive the value upon success</param>
    /// <returns>
    ///   <em>true</em> if successful; otherwise <em>false</em>
    /// </returns>
    [CLSCompliant(false), SuppressMessage("Microsoft.Design", "CA1011", Justification = "By design.")]
    public static bool TryReadNamedValue(this XElement element, string name, out ulong value)
    {
      Contract.Requires(element != null);
      Contract.Requires(name != null);
      Contract.Requires(name.Length > 0);

      var attr = element.Attribute(name);
      if (attr != null
          && attr.Value.Length > 0)
      {
        value = (ulong)attr;
        return true;
      }
      var elm = element.Element(name);
      if (elm != null
          && elm.Value.Length > 0)
      {
        value = (ulong)attr;
        return true;
      }
      value = default(ulong);
      return false;
    }

    /// <summary>
    ///   Tries to read a named value from an xml element.
    /// </summary>
    /// <param name="element">element</param>
    /// <param name="name">name</param>
    /// <param name="value">reference to a variable that will receive the value upon success</param>
    /// <returns>
    ///   <em>true</em> if successful; otherwise <em>false</em>
    /// </returns>
    [SuppressMessage("Microsoft.Design", "CA1011", Justification = "By design.")]
    public static bool TryReadNamedValueAsEnum<T>(this XElement element, string name, out T value)
    {
      Contract.Requires(element != null);
      Contract.Requires(name != null);
      Contract.Requires(name.Length > 0);
      Contract.Requires(typeof(T).IsEnum, "typeof(T) must be an enum");

      var attr = element.Attribute(name);
      if (attr != null
          && attr.Value.Length > 0)
      {
        var v = (string)attr;
        if (Enum.IsDefined(typeof(T), v))
        {
          value = (T)Enum.Parse(typeof(T), v);
          return true;
        }
      }
      var elm = element.Element(name);
      if (elm != null
          && elm.Value.Length > 0)
      {
        var v = (string)elm;
        if (Enum.IsDefined(typeof(T), v))
        {
          value = (T)Enum.Parse(typeof(T), v);
          return true;
        }
      }
      value = default(T);
      return false;
    }

    /// <summary>
    ///   Converts a string into a dynamic XML object.
    /// </summary>
    /// <param name="text">the source xml</param>
    /// <returns>a dynamic object shaped according to the input xml</returns>
    public static dynamic XmlToDynamic(this string text)
    {
      return XDynamic.Parse(text);
    }
  }
}