using FlitBit.Core.Factory;

namespace FlitBit.Core
{
	/// <summary>
	///   Accesses the current factory.
	/// </summary>
	public static class FactoryProvider
	{
		static IFactoryProvider __provider;

		/// <summary>
		///   Gets and sets the global factory.
		/// </summary>
		public static IFactory Factory
		{
			get { return Current.GetFactory(); }
		}

		/// <summary>
		///   Gets the current factory provider.
		/// </summary>
		public static IFactoryProvider Current
		{
			get { return Util.NonBlockingLazyInitializeVolatile(ref __provider, () => new DefaultFactoryProvider()); }
		}

		/// <summary>
		///   Sets the factory provider.
		/// </summary>
		/// <param name="provider"></param>
		public static void SetFactoryProvider(IFactoryProvider provider) { __provider = provider; }
	}
}