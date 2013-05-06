namespace FlitBit.Core.Tests.Factory
{
	public class Implemented : Unimplemented
	{
		public Implemented()
		{
			this.Foo = new DataGenerator().GetWords(20);
		}

		public override string Foo { get; set; }
	}
}