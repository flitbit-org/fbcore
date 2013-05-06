using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FlitBit.Core.Tests.Factory
{
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

		[TestMethod]
		public void Factory_Can_Construct_Interface_With_DefaultImplementationAttribute()
		{
			var factory = FactoryProvider.Factory;
			Assert.IsNotNull(factory);
			Assert.IsTrue(factory.CanConstruct<IImplementedByDefault>());
			
			var impl = factory.CreateInstance<Implemented>();
			Assert.IsNotNull(impl);
			Assert.IsNotNull(impl.Foo);
		}

		[TestMethod]
		public void Factory_Can_Construct_When_Implemented_By_Attribute()
		{
			var factory = FactoryProvider.Factory;
			Assert.IsNotNull(factory);
			
			Assert.IsTrue(factory.CanConstruct<IAmImplementedByAttribute>());

			var impl = factory.CreateInstance<IAmImplementedByAttribute>();
			Assert.IsNotNull(impl);
			Assert.IsNull(impl.NuthinMuch);
			impl.NuthinMuch = "your name here";
			Assert.IsNotNull(impl.NuthinMuch);

		}
	}
}
