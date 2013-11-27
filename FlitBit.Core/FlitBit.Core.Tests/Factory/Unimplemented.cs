namespace FlitBit.Core.Tests.Factory
{
	public abstract class Unimplemented : IUnimplementedInterface
	{	
		public abstract string Foo { get; set; }
	}

  public abstract class Unimplemented2 : IUnimplementedInterface2
  {
    public abstract string Foo { get; set; }
  }
}