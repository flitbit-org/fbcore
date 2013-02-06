using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FlitBit.Core.Tests
{
	[TestClass]
	public class UtilTests
	{
		[TestMethod]
		public void InternIt()
		{
			string it = "it".InternIt();
			Assert.ReferenceEquals("it", it);
		}


	}
}
