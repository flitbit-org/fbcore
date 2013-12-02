#region COPYRIGHT© 2009-2012 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System.Configuration;

namespace FlitBit.Core.Process
{
	/// <summary>
	///   Configuration section for log settings.
	/// </summary>
	public sealed class ProcessIdentifyConfigurationSection : ConfigurationSection
	{
		/// <summary>
		///   Default environment string
		/// </summary>
		public const string CDefaultEnvironment = "dev";

		/// <summary>
		///   Default tenant string
		/// </summary>
		public const string CDefaultTenant = "default";

		/// <summary>
		///   Property name for component.
		/// </summary>
		public const string PropertyNameComponent = "component";

		/// <summary>
		///   Property name for environment.
		/// </summary>
		public const string PropertyNameEnvironment = "environment";

		
		const string CUnknownValue = "unknown";

		/// <summary>
		///   Configuration section name for trace settings.
		/// </summary>
		public static readonly string SectionName = "flitbit.processid";

		/// <summary>
		///   Indicates the name of the component that the current application represents.
		///   The meaning of "component" is up to the user but in general indicates a
		///   role that an application performs within a system.
		/// </summary>
		[ConfigurationProperty(PropertyNameComponent, DefaultValue = CUnknownValue)]
		public string Component { get { return (string) this[PropertyNameComponent]; } set { this[PropertyNameComponent] = value; } }

		/// <summary>
		///   Indicates the name of the environment in which the application is executing.
		///   The meaning of "environment" is up to the user but in general indicates an
		///   environment such as: { dev | test | stage | prod }. In cases where
		///   events in one environment can be heard by journalers in another environment
		///   the presence of this value in an event helps with filtering.
		/// </summary>
		[ConfigurationProperty(PropertyNameEnvironment, DefaultValue = CDefaultEnvironment)]
		public string Environment { get { return (string) this[PropertyNameEnvironment]; } set { this[PropertyNameEnvironment] = value; } }

		/// <summary>
		///   Gets the current configuration section.
		/// </summary>
		public static ProcessIdentifyConfigurationSection Current
		{
			get
			{
				var config = ConfigurationManager.GetSection(
																										 SectionName) as ProcessIdentifyConfigurationSection;
				return config ?? new ProcessIdentifyConfigurationSection();
			}
		}
	}
}