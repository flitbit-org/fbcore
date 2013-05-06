using FlitBit.Core.Meta;

namespace FlitBit.Core.Tests.Factory
{
	[DefaultImplementation(InstanceScopeKind.OnDemand, typeof(ImplementedByDef))]
	public interface IImplementedByDefault
	{
		string Foo { get; set; }
	}
}