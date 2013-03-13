using System;

namespace FlitBit.Core.Meta
{
	/// <summary>
	///   Enumeration of sensitivity kinds.
	/// </summary>
	public enum SensitiveInfoKind
	{
		/// <summary>
		///   Indicates no sensitivity.
		/// </summary>
		None = 0,

		/// <summary>
		///   Indicates the information should not be logged.
		/// </summary>
		NoLog = 1,

		/// <summary>
		///   Indicates the information is personally identifiable information.
		/// </summary>
		PersonallyIdentifying = 1 << 2 | NoLog,
	}

	/// <summary>
	///   Identifies a code element as Personally Identifiable Information
	/// </summary>
	[AttributeUsage(
		AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Interface | AttributeTargets.Class)]
	public class SensitiveInfoAttribute : Attribute
	{
		/// <summary>
		///   Creates a new instance.
		/// </summary>
		/// <param name="kind">the sensitivity kind</param>
		public SensitiveInfoAttribute(SensitiveInfoKind kind) { this.Kind = kind; }

		/// <summary>
		///   Gets the sensitivity kind.
		/// </summary>
		public SensitiveInfoKind Kind { get; private set; }
	}
}