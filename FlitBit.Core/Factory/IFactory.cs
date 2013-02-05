using System.Diagnostics.Contracts;
using System;

namespace FlitBit.Core.Factory
{
	/// <summary>
	/// Interface for classes that construct other classes. 
	/// </summary>
	/// <remarks>
	/// This interface is used by the frameworks to decouple components from the underlying IoC container.
	/// While the FlitBit.IoC's IContainer is-a IFactory, it is a trivial matter to build adapters to
	/// other IoC containers by implementing IFactory to delegate to the IoC of your choice.
	/// </remarks>
	[ContractClass(typeof(CodeContracts.ContractForFactory))]		
	public interface IFactory
	{
		/// <summary>
		/// Indicates whether the factory can construct typeof T.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		bool CanConstruct<T>();

		/// <summary>
		/// Gets the implementation type used when type T is constructed.
		/// </summary>
		/// <typeparam name="T">type T</typeparam>
		/// <returns>If the factory can construct instances of type T, the implementation type used; otherwise null.</returns>
		Type GetImplementationType<T>();

		/// <summary>
		/// Creates a new instance of type T.
		/// </summary>
		/// <typeparam name="T">type T</typeparam>
		/// <returns>a new instance</returns>
		T CreateInstance<T>();
	}

	namespace CodeContracts
	{
		/// <summary>
		/// CodeContracts Class for IFactory
		/// </summary>
		[ContractClassFor(typeof(IFactory))]
		internal abstract class ContractForFactory : IFactory
		{
			/// <summary>
			/// Creates a new instance of type T.
			/// </summary>
			/// <typeparam name="T">type T</typeparam>
			/// <returns>a new instance</returns>
			public T CreateInstance<T>()
			{
				Contract.Ensures(Contract.Result<T>() != null);

				throw new System.NotImplementedException();
			}

			/// <summary>
			/// Indicates whether the factory can construct typeof T.
			/// </summary>
			/// <typeparam name="T"></typeparam>
			/// <returns></returns>
			public bool CanConstruct<T>()
			{
				throw new NotImplementedException();
			}

			/// <summary>
			/// Gets the implementation type used when type T is constructed.
			/// </summary>
			/// <typeparam name="T">type T</typeparam>
			/// <returns>If the factory can construct instances of type T, the implementation type used; otherwise null.</returns>
			public Type GetImplementationType<T>()
			{
				throw new NotImplementedException();
			}
		}
	}		
}
