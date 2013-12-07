using System.Threading;
using System.Threading.Tasks;
using FlitBit.Core.Parallel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FlitBit.Core.Tests.Parallel
{
	[TestClass]
	public class ContextFlowTests
	{
		[TestMethod]
		public void ContextFlow_ContextFlowsWhenNonEmptyContext()
		{
      // To test whether context is captured, use CleanupScope since internally
      // it is coded to flow with the context (registers an IContextFlowProvider).
      using (var scope = CleanupScope.NewOrSharedScope())
			{
			  var outer = scope;
				var context = ContextFlow.Current;

			  var task = Task.Factory.StartNew(ContextFlow.Capture(() =>
			  {
			    var ambient = ContextFlow.Current;
			    Assert.AreNotSame(context, ambient);
          Assert.AreSame(outer, CleanupScope.Current);
			  }));
				
				// detect shenanigans with current context
        Assert.AreSame(context, ContextFlow.Current);

			  task.Wait();
			  Assert.IsFalse(task.IsFaulted);

        Assert.AreSame(context, ContextFlow.Current);
			}
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