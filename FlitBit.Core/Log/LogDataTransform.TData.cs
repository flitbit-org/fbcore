#region COPYRIGHT© 2009-2012 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;

namespace FlitBit.Core.Log
{
	/// <summary>
	///   Static class used by logging to transform runtime types into textual data.
	/// </summary>
	/// <typeparam name="TData">data type TData</typeparam>
	public static class LogDataTransform<TData>
	{
		/// <summary>
		///   Transforms data into textual form suitable for the log.
		/// </summary>
		/// <param name="data">the data</param>
		/// <returns>a string representation</returns>
		public static string Transform(TData data)
		{
			// TODO: generate an inspector class with the ability to sanitize the data being logged.
			if (Equals(default(TData), data))
			{
				return String.Concat("null data of type: ", typeof(TData).GetReadableSimpleName());
			}
			return data.ToJson();
		}
	}
}