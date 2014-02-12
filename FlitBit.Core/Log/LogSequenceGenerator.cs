using System;
using System.Diagnostics.Contracts;

namespace FlitBit.Core.Log
{
  /// <summary>
  /// Utility class for accessing log sequence numbers.
  /// </summary>
  public sealed class LogSequenceGenerator
  {
    static ILogSequenceGenerator __generator;

    /// <summary>
    /// Sets the generator type. Must be called before any log events are generated.
    /// </summary>
    /// <param name="type"></param>
    public static void SetGeneratorType(Type type)
    {
      Contract.Requires<ArgumentException>(typeof(ILogSequenceGenerator).IsAssignableFrom(type));
      Contract.Requires<InvalidOperationException>(!HasSequenceGenerator);

      __generator = (ILogSequenceGenerator)Activator.CreateInstance(type);
    }

    /// <summary>
    /// Sets the generator. Must be called before any log events are generated.
    /// </summary>
    /// <param name="generator"></param>
    public static void SetGenerator(ILogSequenceGenerator generator)
    {
      Contract.Requires<ArgumentNullException>(generator != null);
      Contract.Requires<InvalidOperationException>(!HasSequenceGenerator);

      __generator = generator;
    }

    /// <summary>
    /// Gets the next sequence number.
    /// </summary>
    public static int Next
    {
      get
      {
        if (__generator == null)
        {
          __generator = new DefaultLogSequenceGenerator();
        }
        return __generator.Next;
      }
    }

    /// <summary>
    /// Indicates whether the generator has already been used.
    /// </summary>
    public static bool HasSequenceGenerator { get { return __generator != null; } }
  }
}