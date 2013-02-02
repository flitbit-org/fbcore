
namespace FlitBit.Core.Factory
{
	/// <summary>
	/// Interface for classes that construct other classes. 
	/// </summary>
	/// <remarks>
	/// This interface is used by the frameworks to decouple components from the underlying IoC container.
	/// While the FlitBit.IoC's IContainer is-a IFactory, it is a trivial matter to build adapters to
	/// other IoC containers by implementing IFactory to delegate to the IoC of your choice.
	/// </remarks>
	public interface IFactory
	{
		/// <summary>
		/// Creates a new instance of type T.
		/// </summary>
		/// <typeparam name="T">type T</typeparam>
		/// <returns>a new instance</returns>
		T CreateInstance<T>();
	}
}
