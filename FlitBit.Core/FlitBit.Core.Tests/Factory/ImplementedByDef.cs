namespace FlitBit.Core.Tests.Factory
{
	public class ImplementedByDef : IImplementedByDefault
	{
		public ImplementedByDef()
		{
			this.Foo = new DataGenerator().GetWords(20);
		}

		public string Foo { get; set; }
	}
}