using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FlitBit.Core.Tests
{
	[TestClass]
	public class DynamicJsonTests
	{
		[TestMethod]
		public void JsonToDynamic_EmptyAndWhiteSpaceJsonResultsInNull()
		{
			Assert.IsNull(String.Empty.JsonToDynamic());
			Assert.IsNull(Extensions.JsonToDynamic(null));
			Assert.IsNull(" ".JsonToDynamic());
			Assert.IsNull("\t".JsonToDynamic());
			Assert.IsNull("\r".JsonToDynamic());
		}

		[TestMethod]
		public void JsonToDynamic_ConvertsEmptyArray()
		{
			var json = "[]".JsonToDynamic();
			Assert.IsNotNull(json);
			Assert.AreEqual(0, json.Count);
		}

		[TestMethod]
		public void JsonToDynamic_ConvertsArrayWithEmptyItem()
		{
			var json = "[{}]".JsonToDynamic();
			Assert.IsNotNull(json);
			Assert.AreEqual(1, json.Count);
			Assert.IsNotNull(json[0]);
		}

		[TestMethod]
		public void JsonToDynamic_ConvertsArrayWithSimpleItem()
		{
			var json = "[{\"name\":\"me\"}]".JsonToDynamic();
			Assert.IsNotNull(json);
			Assert.AreEqual(1, json.Count);
			Assert.IsNotNull(json[0]);
			Assert.AreEqual("me", json[0].name);
		}

		[TestMethod]
		public void JsonToDynamic_ConvertsSimpleItem()
		{
			var json = "{\"name\":\"me\"}".JsonToDynamic();
			Assert.IsNotNull(json);
			Assert.AreEqual("me", json.name);
		}

		[TestMethod]
		public void JsonToDynamic_ConvertsNestedItem()
		{
			var json = "{\"names\":{\"first\":\"me\",\"last\":\"self\",\"middle\":[\"my\"]}}".JsonToDynamic();
			Assert.IsNotNull(json);
			Assert.IsNotNull(json.names);
			Assert.AreEqual("me", json.names.first);
			Assert.AreEqual("self", json.names.last);
			Assert.AreEqual("my", json.names.middle[0]);
		}
	}
}
