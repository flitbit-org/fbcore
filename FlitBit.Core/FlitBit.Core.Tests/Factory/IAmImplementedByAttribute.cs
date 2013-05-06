using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FlitBit.Core.Factory;
using FlitBit.Core.Meta;

namespace FlitBit.Core.Tests.Factory
{
	public class YoAmImplemented : AutoImplementedAttribute
	{
		/// <summary>
		///   Gets the implementation for type
		/// </summary>
		/// <param name="factory">the factory from which the type was requested.</param>
		/// <param name="type">the target types</param>
		/// <param name="complete">callback invoked when the implementation is available</param>
		/// <returns>
		///   <em>true</em> if implemented; otherwise <em>false</em>.
		/// </returns>
		/// <exception cref="ArgumentException">thrown if <paramref name="type"/> is not eligible for implementation</exception>
		/// <remarks>
		///   If the <paramref name="complete" /> callback is invoked, it must be given either an implementation type
		///   assignable to type T, or a factory function that creates implementations of type T.
		/// </remarks>
		public override bool GetImplementation(IFactory factory, Type type, Action<Type, Func<object>> complete)
		{
			if (type == typeof(IAmImplementedByAttribute))
			{
				complete(typeof(ImplementedByAttribute), null);
				return true;
			}
			return false;
		}
	}

	[YoAmImplemented]
	public interface IAmImplementedByAttribute
	{
		string NuthinMuch { get; set; }
	}

	public class ImplementedByAttribute: IAmImplementedByAttribute
	{
		public string NuthinMuch { get; set; }
	}
}
