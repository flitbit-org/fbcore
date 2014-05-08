#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

namespace FlitBit.Core.Process
{
	/// <summary>
	///   Interface for a process identity. Used when a process self-identifies.
	/// </summary>
	public interface IProcessIdentity
	{
		/// <summary>
		///   Indicates the software component represented by the process.
		/// </summary>
		string Component { get; }

		/// <summary>
		///   Identifies the environment in which the process is operating.
		/// </summary>
		string Environment { get; }

		/// <summary>
		///   Identifies the machine name where the process is located.
		/// </summary>
		string MachineName { get; }

		/// <summary>
		///   Indicates the process' operating system ID.
		/// </summary>
		int ProcessID { get; }

		/// <summary>
		///   Indicates the name of the process.
		/// </summary>
		string ProcessName { get; }
	}
}