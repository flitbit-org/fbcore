using System;
using FlitBit.Core.Parallel;

namespace FlitBit.Core.Factory
{
	/// <summary>
	///   Interface for classes that construct other classes.
	/// </summary>
	/// <remarks>
	///   This interface is used by the frameworks to decouple components from the underlying IoC container.
	///   While the FlitBit.IoC's IContainer is-a IFactory, it is a trivial matter to build adapters to
	///   other IoC containers by implementing IFactory to delegate to the IoC of your choice.
	/// </remarks>
	public interface IFactory : IParallelShared
	{
		/// <summary>
		///   Gets or sets the next factory when factories are chained.
		/// </summary>
		IFactory Next { get; set; }

		/// <summary>
		///   Indicates whether the factory can construct typeof T.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		bool CanConstruct<T>();

		/// <summary>
		///   Indicates whether the factory can construct type.
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		bool CanConstruct(Type type);

		/// <summary>
		///   Creates a new instance of type T.
		/// </summary>
		/// <typeparam name="T">type T</typeparam>
		/// <returns>a new instance</returns>
		T CreateInstance<T>();

		/// <summary>
		///   Creates a new instance of the type provided.
		/// </summary>
		/// <param name="type">the type of instance to create</param>
		/// <returns>a new instance</returns>
		object CreateInstance(Type type);

		/// <summary>
		///   Gets the implementation type used when type T is constructed.
		/// </summary>
		/// <typeparam name="T">type T</typeparam>
		/// <returns>If the factory can construct instances of type T, the implementation type used; otherwise null.</returns>
		Type GetImplementationType<T>();

		/// <summary>
		///   Gets the implementation type used when type is constructed
		/// </summary>
		/// <param name="type">Type</param>
		/// <returns>If the factory can construct instances of type, the implementation type used; otherwise null.</returns>
		Type GetImplementationType(Type type);

		/// <summary>
		///   Notifies the factory that TImpl is an implementation that should be used to fulfill requests for type T.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <typeparam name="TImpl"></typeparam>
		void RegisterImplementationType<T, TImpl>() where TImpl : T;
	}
}