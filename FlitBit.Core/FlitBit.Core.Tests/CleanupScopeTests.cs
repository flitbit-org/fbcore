using System;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FlitBit.Core.Tests
{
	[TestClass]
	public class CleanupScopeTests
	{
		class DisposableTracking : IDisposable
		{
			public static int OrdinalSeed;

			public DisposableTracking()
			{
				this.Ordinal = Interlocked.Increment(ref OrdinalSeed);
			}

			public bool IsDisposed { get; private set; }
			public int Ordinal { get; private set; }
			public void Dispose()
			{
				IsDisposed = true;
			}
		}

		[TestMethod]
		public void CleanupScope_DisposablePassedInConstructorHasDisposeCalledDuringCleanup()
		{
			var disposable = new DisposableTracking();
			using (var scope = new CleanupScope(disposable))
			{
				Assert.IsFalse(disposable.IsDisposed);
			}
			Assert.IsTrue(disposable.IsDisposed);
		}

		[TestMethod]
		public void CleanupScope_DisposableAddedWithinScopeHasDisposeCalledDuringCleanup()
		{
			DisposableTracking disposable;
			using (var scope = new CleanupScope())
			{
				disposable = scope.Add(new DisposableTracking());
				Assert.IsFalse(disposable.IsDisposed);
			}
			Assert.IsTrue(disposable.IsDisposed);
		}

		[TestMethod]
		public void CleanupScope_ItemsAddedToScopeAreCleanedUpInReverseOrder()
		{
			DisposableTracking disposable = null;
			int count = 0;
			using (var scope = new CleanupScope(new Action(() =>
			{
				Assert.IsTrue(disposable.IsDisposed);
				Assert.AreEqual(2, count++);
			})))
			{
				disposable = scope.Add(new DisposableTracking());
				scope.AddAction(new Action(() =>
				{
					Assert.AreEqual(1, count++);
					Assert.IsFalse(disposable.IsDisposed);
				}));
				scope.AddAction(new Action(() =>
				{
					Assert.AreEqual(0, count++);
					Assert.IsFalse(disposable.IsDisposed);
				}));
			}
			Assert.AreEqual(3, count);
		}
	}
}
