using System;
using System.Net;
using FlitBit.Core.Net;
using FlitBit.Core.Parallel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FlitBit.Core.Tests.Parallel
{
	[TestClass]
	public class NotificationTests
	{
		[TestMethod]
		public void Monkey()
		{
			var hollywood = new Uri("http://search.twitter.com/search.json?q=hollywood");
			var req = hollywood.MakeResourceRequest();
			using (var future = new Future<WebResponse>())
			{																						
				var ar = req.BeginGetResponse(null, null);
				Notification.Instance.ContinueWith(ar, () =>
				{
					future.MarkCompleted(req.EndGetResponse(ar));
				});
				var res = (HttpWebResponse)future.Value;

				Console.WriteLine(res.GetResponseBodyAsString());
			}

		}
	}
}
