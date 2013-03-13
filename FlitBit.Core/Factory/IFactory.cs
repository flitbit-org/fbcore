using System;
using System.Diagnostics.Contracts;
using FlitBit.Core.Factory.CodeContracts;
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
	[ContractClass(typeof(ContractForFactory))]
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
		///   Creates a new instance of type T.
		/// </summary>
		/// <typeparam name="T">type T</typeparam>
		/// <returns>a new instance</returns>
		T CreateInstance<T>();

		/// <summary>
		///   Gets the implementation type used when type T is constructed.
		/// </summary>
		/// <typeparam name="T">type T</typeparam>
		/// <returns>If the factory can construct instances of type T, the implementation type used; otherwise null.</returns>
		Type GetImplementationType<T>();
	}

	namespace CodeContracts
	{
		/// <summary>
		///   CodeContracts Class for IFactory
		/// </summary>
		[ContractClassFor(typeof(IFactory))]
		internal abstract class ContractForFactory : IFactory
		{
			#region IFactory Members

			public T CreateInstance<T>()
			{
				throw new NotImplementedException();
			}

			public bool CanConstruct<T>() { throw new NotImplementedException(); }

			public Type GetImplementationType<T>() { throw new NotImplementedException(); }

			public IFactory Next
			{
				get { throw new NotImplementedException(); }
				set { throw new NotImplementedException(); }
			}

			public object ParallelShare() { throw new NotImplementedException(); }

			#endregion
		}
	}
}