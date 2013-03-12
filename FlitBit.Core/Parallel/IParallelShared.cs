namespace FlitBit.Core.Parallel
{
	/// <summary>
	///   Framework interface used to share objects across threads using context-flow.
	/// </summary>
	public interface IParallelShared
	{
		/// <summary>
		///   Prepares the instance for sharing across threads.
		///   This call should be wrapped in a 'using clause' to
		///   ensure proper cleanup of both the shared and the original.
		/// </summary>
		/// <returns>An equivalent instance.</returns>
		object ParallelShare();
	}
}