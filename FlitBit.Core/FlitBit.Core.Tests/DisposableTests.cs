using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FlitBit.Core.Tests
{
	internal class MyDispsable : Disposable
	{
		int _disposals;
		public Action<bool> OnDispose { get; set; }

		public int Disposals
		{
			get { return _disposals; }
		}

		protected override bool PerformDispose(bool disposing)
		{
			if (OnDispose != null)
			{
				OnDispose(disposing);
			}
			Assert.AreEqual(0, _disposals);
			_disposals++;
			return disposing;
		}
	}

	internal class MyDispsableRequiresTwo : Disposable
	{
		int _disposals;
		public Action<bool> OnDispose { get; set; }

		public int Disposals
		{
			get { return _disposals; }
		}

		protected override bool PerformDispose(bool disposing)
		{
			if (OnDispose != null)
			{
				OnDispose(disposing);
			}
			_disposals++;
			return _disposals == 2;
		}
	}

	[TestClass]
	public class DisposableTests
	{
		[TestMethod]
		public void Dispose_DoesDispose()
		{
			var numberOfCalls = 0;
			var my = new MyDispsable();
			my.OnDispose = (isDisposing) =>
				{
					numberOfCalls++;
					Assert.IsTrue(isDisposing);
					Assert.IsFalse(my.IsDisposed);
				};

			Assert.IsFalse(my.IsDisposed, "shouldn't be disposed yet");
			my.Dispose();
			try
			{
				my.Dispose();
			}
			catch (ObjectDisposedException)
			{
			}
			Assert.IsTrue(my.IsDisposed, "should be disposed");
			Assert.AreEqual(1, numberOfCalls, "Disposable should have only been called once");
			Assert.AreEqual(1, my.Disposals);
		}

		[TestMethod]
		public void Dispose_DoesntDisposeWhenPerformDisposeReturnsFalse()
		{
			var numberOfCalls = 0;
			var my = new MyDispsableRequiresTwo();
			my.OnDispose = (isDisposing) =>
				{
					numberOfCalls++;
					Assert.IsTrue(isDisposing);
					Assert.IsFalse(my.IsDisposed);
				};

			Assert.IsFalse(my.IsDisposed, "shouldn't be disposed yet");
			my.Dispose();
			Assert.IsFalse(my.IsDisposed, "shouldn't be disposed yet");
			my.Dispose();
			Assert.IsTrue(my.IsDisposed, "should be disposed");
			Assert.AreEqual(2, numberOfCalls, "Disposable should have only been called once");
			Assert.AreEqual(2, my.Disposals);
		}
	}
}