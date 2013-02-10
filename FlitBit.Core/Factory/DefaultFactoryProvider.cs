
namespace FlitBit.Core.Factory
{
	internal sealed class DefaultFactoryProvider : IFactoryProvider
	{
		IFactory _factory = new DefaultFactory();
		public IFactory GetFactory() { return _factory;	}
	}
}
