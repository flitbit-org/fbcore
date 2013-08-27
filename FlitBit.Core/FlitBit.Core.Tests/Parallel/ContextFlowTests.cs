using FlitBit.Core.Parallel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FlitBit.Core.Tests.Parallel
{
	[TestClass]
	public class ContextFlowTests
	{
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

		[TestMethod]
		public void ContextFlow_EmptyContextWhenNoContext()
		{
			var ambient = System.Threading.Tasks.Task.Factory.StartNew(() => ContextFlow.ForkAmbient()).Result;
			Assert.IsNull(ambient);
		}

		[TestMethod]
		public void ContextFlow_FlowsWithParallelContinuations()
		{
			ICleanupScope capturedScope = null;
			Continuation it =
				(e) =>
				{
					Assert.AreEqual(capturedScope, CleanupScope.Current);
					Assert.IsFalse(capturedScope.IsDisposed);
				};

			// Scopes participate in context flow...
			using (var scope = CleanupScope.NewOrSharedScope())
			{
				capturedScope = scope;
				var completion = new Completion(this);
				completion.Continue(it);
				completion.MarkCompleted();
			}
		}
	}
}