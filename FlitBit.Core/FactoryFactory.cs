using System;
using System.Diagnostics.Contracts;
using FlitBit.Core.Factory;

namespace FlitBit.Core
{
	/// <summary>
	/// Accesses the global factory if one is present.
	/// </summary>
	public static class FactoryFactory
	{
		/// <summary>
		/// Gets the global factory/adapter if one is present.
		/// </summary>
		/// <typeparam name="TFactory">A fallback factory.</typeparam>
		/// <returns>the global factory if one is present; otherwise a new instance of TFactory.</returns>
		public static IFactory GetFactoryOrFallback<TFactory>()
			where TFactory: IFactory, new()
		{
			Contract.Ensures(Contract.Result<IFactory>() != null);

			var res = Instance;
			return (res == null) ? new TFactory() : res;
		}

		/// <summary>
		/// Gets the global factory/adapter if one is present.
		/// </summary>
		/// <param name="provider">a function that provides an appropriate factory if no global is present.</param>
		/// <returns>the global factory if one is present; otherwise the provider's fallback factory.</returns>
		public static IFactory GetFactoryOrFallback(Func<IFactory> provider)
		{
			Contract.Requires<ArgumentNullException>(provider != null);
			Contract.Ensures(Contract.Result<IFactory>() != null);

			var res = Instance;
			return (res == null) ? provider() : res;
		}

		/// <summary>
		/// Gets and sets the global factory.
		/// </summary>
		public static IFactory Instance { get; set; }
	}
}
