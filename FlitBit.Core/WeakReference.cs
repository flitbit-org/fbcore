#region COPYRIGHT© 2009-2013 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using System.Runtime.Serialization;

namespace FlitBit.Core
{
	/// <summary>
	///   Strongly typed weak reference.
	/// </summary>
	/// <typeparam name="T">referenced type T</typeparam>
	[Serializable]
	public class WeakReference<T> : WeakReference
	{
		/// <summary>
		///   Creates a new instance.
		/// </summary>
		/// <param name="target">a reference target.</param>
		public WeakReference(T target)
			: base(target) { }

		/// <summary>
		///   Creates a new instance (from serialization)
		/// </summary>
		/// <param name="info">serialization info</param>
		/// <param name="context">serialization context</param>
		protected WeakReference(SerializationInfo info, StreamingContext context)
			: base(info, context) { }

		/// <summary>
		///   Gets the referenced target as type T.
		/// </summary>
		public T StrongTarget
		{
			get { return (T) this.Target; }
		}
	}
}