using System.Collections.Generic;
using System.Linq;
using FlitBit.Core.Xml;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FlitBit.Core.Tests.Xml
{
	[TestClass]
	public class XDynamicTests
	{
		[TestMethod]
		public void XDynamic_CanParseXmlResultingInDynamic()
		{
			const string source = @"<People>
	<Person id='1347'><Name>Gilbert Aldibrand</Name></Person>
	<Person id='1828'><Name>Gertrude Schmidt</Name></Person>
	<Person id='2994'><Name>Wilbur Brandoff</Name></Person>
</People>";

			var xdyn = XDynamic.Parse(source);

			Assert.IsNotNull(xdyn);
			Assert.IsNotNull(xdyn.Person);
			Assert.AreEqual(3, xdyn.Person.Count);

			var peeps = xdyn.Person as IEnumerable<dynamic>;

			var gilbert = (from p in peeps
										where p.Name.Contains("Gilbert")
										select p).Single();
			Assert.IsNotNull(gilbert);
			Assert.AreEqual("1347", gilbert.id);
		}
	}
}