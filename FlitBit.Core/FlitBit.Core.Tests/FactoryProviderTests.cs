using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FlitBit.Core.Tests
{
	[TestClass]
	public class FactoryProviderTests
	{
		[TestMethod]
		public void FactoryProvider_AlwaysHasDefault()
		{
			// Can always get a factory.
			var factory = FactoryProvider.Factory;
			Assert.IsNotNull(factory);

			// Can always get a provider.
			var current = FactoryProvider.Current;
			Assert.IsNotNull(current);

			// Can reset the provider...
			FactoryProvider.SetFactoryProvider(null);

			// Can still get a factory...
			factory = FactoryProvider.Factory;
			Assert.IsNotNull(factory);
		}
	}
}