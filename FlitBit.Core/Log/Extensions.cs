#region COPYRIGHT© 2009-2012 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using System.Diagnostics.Contracts;

namespace FlitBit.Core.Log
{
	/// <summary>
	///   Utility class containing extensions for logging.
	/// </summary>
	public static class Extensions
	{				 
		/// <summary>
		///   Gets the type's log sink.
		/// </summary>
		/// <param name="type">the target type.</param>
		/// <returns>A log sink.</returns>
		public static ILogSink GetLogSink(this Type type)
		{
			Contract.Requires<ArgumentNullException>(type != null);

			return LogSinkManager.Singleton.GetLogSinkForType(type);
		}

		/// <summary>
		///   Gets the instance's log sink.
		/// </summary>
		/// <param name="item">the target instance.</param>
		/// <returns>A log sink.</returns>
		public static ILogSink GetLogSink<T>(this T item)
		{
// ReSharper disable CompareNonConstrainedGenericWithNull
			Contract.Requires<ArgumentNullException>(item != null);
// ReSharper restore CompareNonConstrainedGenericWithNull

			return item.GetType()
								.GetLogSink();
		}

		/// <summary>
		///   Gets the log source name for a type.
		/// </summary>
		/// <param name="type">the type</param>
		/// <returns>the log source name</returns>
		public static string GetLogSourceName(this Type type)
		{
			Contract.Requires<ArgumentNullException>(type != null);

			return type.Assembly.FullName;
		}
	}
}