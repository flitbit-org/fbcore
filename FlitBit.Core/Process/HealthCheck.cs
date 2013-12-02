using System;
using System.Dynamic;
using System.Reflection;
using FlitBit.Core.Log;
using Newtonsoft.Json;

namespace FlitBit.Core.Process
{
	/// <summary>
	/// Base health check object.
	/// </summary>
	public class HealthCheck
	{
		const string Failed = "Failed";
		const string Succeeded = "Succeeded";
		static readonly ILogSink Log = typeof(HealthCheck).GetLogSink();

		/// <summary>
		/// Gets the health as a dynamic status object.
		/// </summary>
		/// <returns></returns>
		public dynamic GetStatus()
		{
#if DEBUG
			return GetStatus(true);
#else
			return GetStatus(false);
#endif
		}

		/// <summary>
		/// Gets the health as a dynamic status object.
		/// </summary>
		/// <param name="showStackTrace"></param>
		/// <returns></returns>
		public dynamic GetStatus(bool showStackTrace)
		{
			dynamic result = new ExpandoObject();
			try
			{
				result.ProcessIdentity = ProcessIdentity.MakeProcessIdentity();
				result.SpecializedHealthCheckKind = this.GetType()
																								.FullName;
				result.HealthCheck = PerformGetStatus(result) ? Succeeded : Failed;
			}
			catch (Exception e)
			{
				result.HealthCheck = Failed;
				result.SpecializedHealthCheck = Failed;
				result.ErrorInfo = Core.Extensions.FormatForLogging(e, showStackTrace);
				Log.Error(new
				{
					UnexpectedException = Core.Extensions.FormatForLogging(e)
				});
			}
			return result;
		}

		/// <summary>
		/// Allows specializations to contribute their own status elements.
		/// </summary>
		/// <param name="status"></param>
		/// <returns></returns>
		protected virtual bool PerformGetStatus(dynamic status)
		{
			return true;
		}

		/// <summary>
		/// Creates JSON from the provided status object.
		/// </summary>
		/// <param name="status"></param>
		/// <returns></returns>
		public static string FromStatusToJson(dynamic status)
		{
			return JsonConvert.SerializeObject(status, Formatting.Indented);
		}

		/// <summary>
		/// Indicates whether the status check was successful.
		/// </summary>
		/// <param name="testResult"></param>
		/// <returns></returns>
		public static bool IsSuccess(dynamic testResult)
		{
			return testResult != null
				&& !String.IsNullOrEmpty(testResult.HealthCheck)
				&& String.Equals(Succeeded, testResult.HealthCheck, StringComparison.OrdinalIgnoreCase);
		}
	}
}