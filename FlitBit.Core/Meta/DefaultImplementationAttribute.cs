#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using FlitBit.Core.Factory;

namespace FlitBit.Core.Meta
{
  /// <summary>
  ///   Indicates the default implementation of an interface.
  /// </summary>
  [AttributeUsage(AttributeTargets.Interface)]
  public class DefaultImplementationAttribute : AutoImplementedAttribute
  {
    /// <summary>
    ///   Creates a new instance.
    /// </summary>
    public DefaultImplementationAttribute() { }

    /// <summary>
    ///   Creates a new instance.
    /// </summary>
    /// <param name="recommemdedScope">Recommended scope for the resultant type.</param>
    /// <param name="defaultImplType">The default implementation used when non-other is contributed.</param>
    public DefaultImplementationAttribute(InstanceScopeKind recommemdedScope, Type defaultImplType)
      : base(recommemdedScope)
    {
      this.RecommemdedScope = recommemdedScope;
      this.DefaultImplementationType = defaultImplType;
    }

    /// <summary>
    ///   The default implementation used when non-other is contributed.
    /// </summary>
    public Type DefaultImplementationType { get; set; }

    /// <summary>
    ///   Gets the implementation for type
    /// </summary>
    /// <param name="factory">the factory from which the type was requested.</param>
    /// <param name="type">the target types</param>
    /// <param name="complete">callback invoked when the implementation is available</param>
    /// <returns>
    ///   <em>true</em> if implemented; otherwise <em>false</em>.
    /// </returns>
    /// <exception cref="ArgumentException">thrown if <paramref name="type" /> is not eligible for implementation</exception>
    /// <remarks>
    ///   If the <paramref name="complete" /> callback is invoked, it must be given either an implementation type
    ///   assignable to type T, or a factory function that creates implementations of type T.
    /// </remarks>
    public override bool GetImplementation(IFactory factory, Type type, Action<Type, Func<object>> complete)
    {
      if (DefaultImplementationType != null)
      {
        complete(DefaultImplementationType, null);
        return true;
      }
      return false;
    }
  }
}