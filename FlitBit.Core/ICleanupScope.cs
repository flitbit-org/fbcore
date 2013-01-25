#region COPYRIGHT© 2009-2013 Phillip Clark. All rights reserved.
// For licensing information see License.txt (MIT style licensing).
#endregion

using System;

namespace FlitBit.Core
{
	/// <summary>
	/// Deliniates a cleanup scope.
	/// </summary>
	public interface ICleanupScope : IDisposable
	{
		/// <summary>
		/// Adds a disposable item to the scope. When the scope
		/// is disposed all added items are guaranteed to also be
		/// disposed.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="item"></param>
		/// <returns></returns>
		T Add<T>(T item) where T: IDisposable;

		/// <summary>
		/// Adds an action to be performed upon scope
		/// completion (on dispose).
		/// </summary>
		/// <param name="action"></param>
		void AddAction(Action action);

		/// <summary>
		/// Shares the scope; suitable for sharing across threads.
		/// This call should be wrapped in a 'using clause' to
		/// ensure proper cleanup of both the shared and the original
		/// scopes.
		/// </summary>
		/// <returns>An equivalent scope.</returns>
		ICleanupScope ShareScope();
	}
}
