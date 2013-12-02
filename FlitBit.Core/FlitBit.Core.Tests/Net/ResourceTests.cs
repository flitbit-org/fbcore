using System;
using System.Net;
using System.Text;
using FlitBit.Core.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FlitBit.Core.Tests.Net
{
	[TestClass]
	public class ResourceTests
	{
		[TestMethod]
		public void HttpGetDynamic_CanCallApiRequiringBasicAuth()
		{
			// This resource is a Couch DB...
			const string db = "https://flitbit.cloudant.com/trubl";
			var testdb = new Uri(String.Concat(db, "/_all_docs?include_docs=true"));
			Exception unexpected = null;

			// Create request and associate creds using basic auth...
			var req = AttachTestCredentialsUsingBasicAuth(testdb.MakeResourceRequest());

			using (var completion = req.ParallelGet(res => res.DeserializeResponseAsDynamic())
																.ContinueWithCompletion(
																											 (e, d) =>
																											 {
																												if ((unexpected = e) == null)
																												{
																													try
																													{
																														if (d.total_rows > 0)
																														{
																															foreach (var row in d.rows)
																															{
																																var value = row.doc;
																																Console.WriteLine(String.Concat("timestamp: ", value.timestamp, ", machine_name: ",
																																	value.machine_name, ", id: ", row.id, ", info: ", value.info));
																															}
																														}
																													}
																													catch (Exception assertionFailure)
																													{
																														unexpected = assertionFailure;
																													}
																												}
																											 }))
			{
				completion.Wait(TimeSpan.FromSeconds(5));
				Assert.IsTrue(completion.IsCompleted);
			}

			Assert.IsNull(unexpected);
		}

		[TestMethod]
		public void HttpGetDynamic_CanGetJsonFromWellKnownResource()
		{
			var hollywood = new Uri("http://search.twitter.com/search.json?q=hollywood");
			var supreme = new Uri("http://search.twitter.com/search.json?q=supreme%20court");
			var erlang = new Uri("http://search.twitter.com/search.json?q=erlang");

			using (var h = hollywood.ParallelGet(res => res.DeserializeResponseAsDynamic()).ContinueWithCompletion(
																																																						 (e, d) =>
																																																						 {
																																																							foreach (var tweet in d.results)
																																																							{
																																																								Console.WriteLine(String.Concat(tweet.from_user_name, " says: ",
																																																									tweet.text));
																																																							}
																																																						 }))
			{
				Assert.IsFalse(h.IsCompleted);
				Assert.IsFalse(h.IsFaulted);

				using (var s = supreme.ParallelGet(res => res.DeserializeResponseAsDynamic()).ContinueWithCompletion(
																																																						 (e, d) =>
																																																						 {
																																																							foreach (var tweet in d.results)
																																																							{
																																																								Console.WriteLine(String.Concat(tweet.from_user_name, " says: ",
																																																									tweet.text));
																																																							}
																																																						 }))
				{
					Assert.IsFalse(s.IsCompleted);
					Assert.IsFalse(s.IsFaulted);

					using (var erl = erlang.ParallelGet(res => res.DeserializeResponseAsDynamic()).ContinueWithCompletion(
																																																							 (e, d) =>
																																																							 {
																																																								foreach (var tweet in d.results)
																																																								{
																																																									Console.WriteLine(String.Concat(tweet.from_user_name,
																																																										" says: ", tweet.text));
																																																								}
																																																							 }))
					{
						Assert.IsFalse(erl.IsCompleted);
						Assert.IsFalse(erl.IsFaulted);

						h.Wait(TimeSpan.FromSeconds(5));
						s.Wait(TimeSpan.FromSeconds(5));
						erl.Wait(TimeSpan.FromSeconds(5));

						// In MTA I'd rather:
						// 
						// var waitHandles = new WaitHandle[] { h.WaitHandle, s.WaitHandle, erl.WaitHandle };
						// WaitHandle.WaitAll(waitHandles);
					}
				}
			}
		}

		[TestMethod]
		public void HttpGetDynamic_CanPerformHttpDelete()
		{
			// This resource is a Couch DB...
			const string db = "https://flitbit.cloudant.com/trubl";
			var testdb = new Uri(String.Concat(db, "/_all_docs"));

			Exception unexpected = null;

			var req = AttachTestCredentialsUsingBasicAuth(testdb.MakeResourceRequest());

			using (var completion = req.ParallelGet(res => res.DeserializeResponseAsDynamic())
																.ContinueWithCompletion((e, d) =>
																{
																	if ((unexpected = e) == null)
																	{
																		try
																		{
																			if (d.total_rows > 0)
																			{
																				foreach (var row in d.rows)
																				{
																					var doc = new Uri(String.Concat(db, "/", row.id, "?rev=", row.value.rev));
																					var delReq = AttachTestCredentialsUsingBasicAuth(doc.MakeResourceRequest());
																					Console.WriteLine(String.Concat("Deleting: ", doc));

																					var deleteResult = delReq.ParallelDelete(res => res.DeserializeResponseAsDynamic())
																																	.ContinueWithCompletion((ee, dd) =>
																																	{
																																		if ((unexpected = ee) == null)
																																		{
																																			try
																																			{
																																				Console.WriteLine(String.Concat("Success deleting: ", doc));
																																				Assert.IsTrue(dd.ok);
																																			}
																																			catch (Exception assertionFailure)
																																			{
																																				unexpected = assertionFailure;
																																			}
																																		}
																																	});
																					if (deleteResult.Wait(TimeSpan.FromSeconds(5)))
																					{
																						Assert.IsTrue(deleteResult.IsCompleted);
																					}
																				}
																			}
																		}
																		catch (Exception assertionFailure)
																		{
																			unexpected = assertionFailure;
																		}
																	}
																}))
			{
				if (completion.Wait(TimeSpan.FromSeconds(10)))
				{
					Assert.IsTrue(completion.IsCompleted);
				}
			}
			Assert.IsNull(unexpected);
		}

		[TestMethod]
		public void HttpGetDynamic_CanPerformHttpPutAgainstApiRequiringBasicAuth()
		{
			// This resource is a Couch DB...
			var rand = new Random(Environment.TickCount);
			var dataGen = new DataGenerator();

			var docid = Guid.NewGuid().ToString("N");
			var data =
				new
				{
					timestamp = DateTime.UtcNow,
					info = dataGen.GetWords(rand.Next(8, 80)),
					machine_name = Environment.MachineName
				};

			var testdb = new Uri(String.Concat("https://flitbit.cloudant.com/trubl/", docid));
			Exception unexpected = null;

			var req = AttachTestCredentialsUsingBasicAuth(testdb.MakeResourceRequest());

			var json = data.ToJson();
			var postBody = Encoding.UTF8.GetBytes(json);

			using (var completion = req.ParallelPut(postBody, "application/json", res => res.DeserializeResponseAsDynamic())
																.ContinueWithCompletion(
																											 (e, d) =>
																											 {
																												if ((unexpected = e) == null)
																												{
																													try
																													{
																														Assert.IsTrue(d.ok);
																														Assert.AreEqual(docid, d.id);
																													}
																													catch (Exception assertionFailure)
																													{
																														unexpected = assertionFailure;
																													}
																												}
																											 }))
			{
				completion.Wait(TimeSpan.FromSeconds(5));
				Assert.IsTrue(completion.IsCompleted);
			}

			Assert.IsNull(unexpected);
		}

		[TestMethod]
		public void HttpGetDynamic_Sequential()
		{
			GetTweetsAndPrint(new Uri("http://api.openweathermap.org/data/2.5/weather?q=London,uk"));
			GetTweetsAndPrint(new Uri("http://api.openweathermap.org/data/2.5/weather?q=Seattle,wa"));
			GetTweetsAndPrint(new Uri("http://api.openweathermap.org/data/2.5/weather?q=Salt,ut"));
		}

		void GetTweetsAndPrint(Uri uri)
		{
			var request = uri.MakeResourceRequest();
			request.HttpGet((e, r) =>
			{
				var json = r.DeserializeResponseAsDynamic();
				Console.WriteLine(String.Concat(json.name, " weather: ", json.weather[0].description));
			});
		}

		static HttpWebRequest AttachTestCredentialsUsingBasicAuth(HttpWebRequest req)
		{
			return req.WithBasicAuth("nedlyincessicturandstolu", "bnoIEkLiuSLyHobSaOxcUL4o");
		}
	}
}
