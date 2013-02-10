using FlitBit.Core.Parallel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FlitBit.Core.Tests.Parallel
{
	[TestClass]
	public class ContextFlowTests
	{
		[TestMethod]
		public void ContextFlow_EmptyContextWhenNoContext()
		{
			var ambient = ContextFlow.ForkAmbient();
			Assert.IsNull(ambient);
		}

		[TestMethod]
		public void ContextFlow_ContextWhenNonEmptyContext()
		{
			using (var scope = CleanupScope.NewOrSharedScope())
			{
				var context = ContextFlow.Current;
				using (var ambient = ContextFlow.ForkAmbient())
				{
					Assert.AreNotSame(context, ambient);
					ContextFlow.EnsureAmbient(ambient);
					Assert.AreSame(ContextFlow.Current, ambient);
				}
				Assert.AreSame(context, ContextFlow.Current);
			}
		}		
	}
}
