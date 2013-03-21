using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FlitBit.Core.Tests.Factory
{
	public interface IUnimplementedInterface
	{
		string Foo { get; set; }
	}

	public abstract class Unimplemented : IUnimplementedInterface
	{	
		public abstract string Foo { get; set; }
	}

	public class Implemented : Unimplemented
	{
		public Implemented()
		{
			this.Foo = new DataGenerator().GetWords(20);
		}

		public override string Foo { get; set; }
	}

	[TestClass]
	public class FactoryTests
	{
		[TestMethod]
		public void Factory_Cannot_Construct_Unimplemented_Interface()
		{
			var factory = FactoryProvider.Factory;
			Assert.IsNotNull(factory);
			Assert.IsFalse(factory.CanConstruct<IUnimplementedInterface>());
			try
			{
				factory.CreateInstance<IUnimplementedInterface>();
				Assert.Fail("Should have thrown InvalidOperationException!");
			}
			catch (InvalidOperationException)
			{	
			}
		}

		[TestMethod]
		public void Factory_Cannot_Construct_Unimplemented_Abstract()
		{
			var factory = FactoryProvider.Factory;
			Assert.IsNotNull(factory);
			Assert.IsFalse(factory.CanConstruct<Unimplemented>());
			try
			{
				factory.CreateInstance<Unimplemented>();
				Assert.Fail("Should have thrown InvalidOperationException!");
			}
			catch (InvalidOperationException)
			{
			}
		}

		[TestMethod]
		public void Factory_Can_Construct_Unimplemented_Interface_After_Registration()
		{
			var factory = FactoryProvider.Factory;
			Assert.IsNotNull(factory);
			Assert.IsFalse(factory.CanConstruct<IUnimplementedInterface>());
			try
			{
				factory.CreateInstance<IUnimplementedInterface>();
				Assert.Fail("Should have thrown InvalidOperationException!");
			}
			catch (InvalidOperationException)
			{
			}

			factory.RegisterImplementationType<IUnimplementedInterface, Implemented>();
			Assert.IsTrue(factory.CanConstruct<IUnimplementedInterface>());

			var impl = factory.CreateInstance<IUnimplementedInterface>();
			Assert.IsNotNull(impl);
			Assert.IsNotNull(impl.Foo);
		}

		[TestMethod]
		public void Factory_Can_Construct_Unimplemented_Abstract_After_Registration()
		{
			var factory = FactoryProvider.Factory;
			Assert.IsNotNull(factory);
			Assert.IsFalse(factory.CanConstruct<Unimplemented>());
			try
			{
				factory.CreateInstance<Unimplemented>();
				Assert.Fail("Should have thrown InvalidOperationException!");
			}
			catch (InvalidOperationException)
			{
			}

			factory.RegisterImplementationType<Unimplemented, Implemented>();
			Assert.IsTrue(factory.CanConstruct<Unimplemented>());

			var impl = factory.CreateInstance<Unimplemented>();
			Assert.IsNotNull(impl);
			Assert.IsNotNull(impl.Foo);
		}
	}
}
