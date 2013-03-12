using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FlitBit.Core.Tests
{
	[TestClass]
	public class UtilTests
	{
		[TestMethod]
		public void InternIt()
		{
			var it = "it".InternIt();
			ReferenceEquals("it", it);
		}
	}
}