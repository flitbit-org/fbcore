using System;
using System.Collections.Concurrent;
using System.Linq;
using FlitBit.Core.Meta;

namespace FlitBit.Core.Factory
{
  /// <summary>
  ///   Factory capable of constructing auto-implemented types.
  /// </summary>
  public sealed class DefaultFactory : IFactory
  {
    readonly ConcurrentDictionary<object, TypeRecord> _types = new ConcurrentDictionary<object, TypeRecord>();

    #region IFactory Members

    /// <summary>
    ///   Determins if the factory can construct instances of type.
    /// </summary>
    /// <param name="type">The Type</param>
    /// <returns></returns>
    public bool CanConstruct(Type type)
    {
      var key = type.GetKeyForType();
      TypeRecord rec;
      if (_types.TryGetValue(key, out rec))
      {
        return true;
      }
      if (!type.IsAbstract)
      {
        if (type.GetConstructor(Type.EmptyTypes) != null)
        {
          rec = new TypeRecord
          {
            TargetType = type
          };
          _types.TryAdd(key, rec);
          return true;
        }
      }
      else
      {
        var gotImpl = false;
        return type.GetCustomAttributes(typeof(AutoImplementedAttribute), true)
                   .Cast<AutoImplementedAttribute>()
                   .Any(attr => attr.GetImplementation(this, type, (impl, functor) =>
                   {
                     if (impl != null
                         || functor != null)
                     {
                       // use the implementation type if provided
                       rec = new TypeRecord
                       {
                         TargetType = impl,
                         Functor = functor
                       };
                       this._types.TryAdd(key, rec);
                       gotImpl = true;
                     }
                   }) && gotImpl);
      }
      return false;
    }

    /// <summary>
    ///   Creates a new instance of type T.
    /// </summary>
    /// <typeparam name="T">type T</typeparam>
    /// <returns></returns>
    public T CreateInstance<T>()
    {
      return (T)CreateInstance(typeof(T));
    }

    /// <summary>
    ///   Creates a new instance of the type provided.
    /// </summary>
    /// <returns>a new instance</returns>
    public object CreateInstance(Type type)
    {
      var key = type.GetKeyForType();
      TypeRecord rec;
      if (this.CanConstruct(type)
          && _types.TryGetValue(key, out rec))
      {
        return rec.CreateInstance();
      }
      throw new InvalidOperationException(String.Concat("No suitable implementation found: ",
        type.GetReadableFullName(), "."));
    }

    /// <summary>
    ///   Determins if the factory can construct instances of type T.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public bool CanConstruct<T>()
    {
      return CanConstruct(typeof(T));
    }

    /// <summary>
    ///   Gets the factory's implementation type for type T.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public Type GetImplementationType<T>()
    {
      return GetImplementationType(typeof(T));
    }

    /// <summary>
    ///   Gets the factory's implementation type for type
    /// </summary>
    /// <param name="type">The type</param>
    /// <returns></returns>
    public Type GetImplementationType(Type type)
    {
      var key = type.GetKeyForType();
      TypeRecord rec;
      if (this.CanConstruct(type)
          && _types.TryGetValue(key, out rec))
      {
        return rec.TargetType;
      }
      return null;
    }

    /// <summary>
    ///   Notifies the factory that TImpl is an implementation that should be used to fulfill requests for type T.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TImpl"></typeparam>
    public void RegisterImplementationType<T, TImpl>() where TImpl : T
    {
      var key = typeof(T).GetKeyForType();
      var reg = new TypeRecord
      {
        TargetType = typeof(TImpl)
      };
      // !! Think about an implementation-type-replaced event.
      // This blanket replacement favors frameworks wired up later in the process.
      _types.AddOrUpdate(key, reg, (k, current) => reg);
    }

    /// <summary>
    ///   Gets or sets the next factory when chained.
    /// </summary>
    public IFactory Next { get; set; }

    /// <summary>
    ///   This type sharable between threads as-is.
    /// </summary>
    /// <returns></returns>
    public object ParallelShare()
    {
      return this;
    }

    #endregion

    struct TypeRecord
    {
      public Delegate Functor;
      public Type TargetType;

      internal object CreateInstance()
      {
        return (Functor != null) ? Functor.DynamicInvoke(null) : Activator.CreateInstance(TargetType);
      }
    }
  }
}