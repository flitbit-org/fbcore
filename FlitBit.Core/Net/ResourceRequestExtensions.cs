#region COPYRIGHT© 2009-2013 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using System.Diagnostics.Contracts;
using System.Dynamic;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using FlitBit.Core.Parallel;
using FlitBit.Core.Properties;
using FlitBit.Core.Xml;
using Newtonsoft.Json;
using Formatting = Newtonsoft.Json.Formatting;

namespace FlitBit.Core.Net
{
	/// <summary>
	///   Extensions for resource oriented HTTP requests.
	/// </summary>
	public static class ResourceRequestExtensions
	{
		const string DefaultAcceptHeader =
			"application/json, text/json;q=0.9, application/xml;q=0.8, text/xml;q=0.7, text/plain;q=0.6";

		/// <summary>
		///   String used as the Client in HTTP requests.
		/// </summary>
		public static readonly string ResourceClientString = String
			.Concat(Resources.ResourceClientName, Assembly.GetExecutingAssembly()
																										.GetName()
																										.Version.ToString());

		/// <summary>
		///   Deserializes a response to an object of type T.
		/// </summary>
		/// <typeparam name="T">typeof T</typeparam>
		/// <param name="response">the web resposne</param>
		/// <returns>an instance of type T</returns>
		public static T DeserializeResponse<T>(this HttpWebResponse response)
		{
			Contract.Requires(response != null, "response cannot be null");

			var data = default(T);
			if (response.ContentType.Contains("/json"))
			{
				var body = response.GetResponseBodyAsString();
				if (!String.IsNullOrEmpty(body))
				{
					data = JsonConvert.DeserializeObject<T>(body);
				}
			}
			else if (response.ContentType.Contains("/xml"))
			{
				var body = response.GetResponseBodyAsString();
				if (!String.IsNullOrEmpty(body))
				{
					return body.XmlToDynamic();
				}
			}
			return data;
		}

		/// <summary>
		///   Deserializes a response body as a dynamic object.
		/// </summary>
		/// <param name="response">the web response</param>
		/// <returns>a dynamic object over the response body</returns>
		public static dynamic DeserializeResponseAsDynamic(this HttpWebResponse response)
		{
			Contract.Requires(response != null, "response cannot be null");

			if (response.ContentType.Contains("/json"))
			{
				var body = response.GetResponseBodyAsString();
				if (!String.IsNullOrEmpty(body))
				{
					return body.JsonToDynamic();
				}
			}
			else if (response.ContentType.Contains("/xml"))
			{
				var body = response.GetResponseBodyAsString();
				if (!String.IsNullOrEmpty(body))
				{
					return body.XmlToDynamic();
				}
			}
			return new ExpandoObject();
		}

		/// <summary>
		///   Gets the response body from a web response.
		/// </summary>
		/// <param name="response">the web response</param>
		/// <returns>the full web response as text</returns>
		public static string GetResponseBodyAsString(this HttpWebResponse response)
		{
			if (response != null)
			{
				using (var stream = response.GetResponseStream())
				{
					if (stream != null)
					{
						using (var reader = new StreamReader(stream))
						{
							return reader.ReadToEnd();
						}
					}
				}
			}
			return null;
		}

		/// <summary>
		///   Performs an HTTP POST against a URI as JSON.
		/// </summary>
		/// <param name="req">the http request</param>
		/// <param name="after">an action to be called upon completion</param>
		public static void HttpGet(this HttpWebRequest req, Action<Exception, HttpWebResponse> after)
		{
			Contract.Requires(req != null);
			Contract.Requires(after != null);

			ExecuteHttpVerb(req, "GET", after);
		}

		/// <summary>
		///   Performs an HTTP POST against a URI as JSON.
		/// </summary>
		/// <typeparam name="TBody">body type B</typeparam>
		/// <param name="req">the http request</param>
		/// <param name="body">the post body (will be serialized as JSON)</param>
		/// <param name="after">an action to be called upon completion</param>
		public static void HttpPostJson<TBody>(this HttpWebRequest req, TBody body, Action<Exception, HttpWebResponse> after)
		{
			HttpPostJson(req, body, Encoding.UTF8, after);
		}

		/// <summary>
		///   Performs an HTTP POST against a URI as JSON.
		/// </summary>
		/// <typeparam name="TBody">body type B</typeparam>
		/// <param name="req">the http request</param>
		/// <param name="body">the post body (will be serialized as JSON)</param>
		/// <param name="encoding">the content encoding</param>
		/// <param name="after">an action to be called upon completion</param>
		public static void HttpPostJson<TBody>(this HttpWebRequest req, TBody body, Encoding encoding,
			Action<Exception, HttpWebResponse> after)
		{
			Contract.Requires(req != null);
			Contract.Requires(after != null);

			var lclEncoding = encoding ?? Encoding.UTF8;

			var bodyAsString = JsonConvert.SerializeObject(body, Formatting.None,
																										new JsonSerializerSettings
																										{
																											MissingMemberHandling = MissingMemberHandling.Ignore
																										});
			var buffer = lclEncoding.GetBytes(bodyAsString);

			ExecuteHttpVerbWithPostBody(req, buffer, "application/json", "POST", after);
		}

		/// <summary>
		///   Performs an HTTP POST against a URI as XML.
		/// </summary>
		/// <param name="req">
		///   the http request
		/// </param>
		/// <param name="body">
		///   the post body
		/// </param>
		/// <param name="after">
		///   an action to be called upon completion
		/// </param>
		public static void HttpPostXml(this HttpWebRequest req, XmlDocument body, Action<Exception, HttpWebResponse> after)
		{
			HttpPostXml(req, body, Encoding.UTF8, after);
		}

		/// <summary>
		///   Performs an HTTP POST against a URI as XML.
		/// </summary>
		/// <param name="req">
		///   the http request
		/// </param>
		/// <param name="body">
		///   the post body
		/// </param>
		/// <param name="encoding">
		///   the content encoding
		/// </param>
		/// <param name="after">
		///   an action to be called upon completion
		/// </param>
		public static void HttpPostXml(this HttpWebRequest req, XmlDocument body, Encoding encoding,
			Action<Exception, HttpWebResponse> after)
		{
			Contract.Requires(req != null);
			Contract.Requires(body != null);
			Contract.Requires(after != null);

			var enc = encoding ?? Encoding.UTF8;

			var bodyAsString = body.OuterXml;
			var buffer = enc.GetBytes(bodyAsString);

			ExecuteHttpVerbWithPostBody(req, buffer, "application/xml", "POST", after);
		}

		/// <summary>
		///   Performs an HTTP PUT against a URI as JSON.
		/// </summary>
		/// <typeparam name="TBody">body type B</typeparam>
		/// <param name="req">the http request</param>
		/// <param name="body">the post body (will be serialized as JSON)</param>
		/// <param name="after">an action to be called upon completion</param>
		public static void HttpPutJson<TBody>(this HttpWebRequest req, TBody body, Action<Exception, HttpWebResponse> after)
		{
			HttpPutJson(req, body, Encoding.UTF8, after);
		}

		/// <summary>
		///   Performs an HTTP PUT against a URI as JSON.
		/// </summary>
		/// <typeparam name="TBody">body type B</typeparam>
		/// <param name="req">the http request</param>
		/// <param name="body">the post body (will be serialized as JSON)</param>
		/// <param name="encoding">the content encoding</param>
		/// <param name="after">an action to be called upon completion</param>
		public static void HttpPutJson<TBody>(this HttpWebRequest req, TBody body, Encoding encoding,
			Action<Exception, HttpWebResponse> after)
		{
			Contract.Requires(req != null);
			Contract.Requires(after != null);

			var lclEncoding = encoding ?? Encoding.UTF8;

			var bodyAsString = JsonConvert.SerializeObject(body, Formatting.None,
																										new JsonSerializerSettings
																										{
																											MissingMemberHandling = MissingMemberHandling.Ignore
																										});
			var buffer = lclEncoding.GetBytes(bodyAsString);

			ExecuteHttpVerbWithPostBody(req, buffer, "application/json", "PUT", after);
		}

		/// <summary>Performs an HTTP PUT against a URI as text.</summary>
		/// <param name="req">The request.</param>
		/// <param name="body">The body.</param>
		/// <param name="after">The continuation to be executed after.</param>
		public static void HttpPutText(this HttpWebRequest req, string body, Action<Exception, HttpWebResponse> after)
		{
			HttpPutText(req, body, Encoding.UTF8, after);
		}

		/// <summary>Performs an HTTP PUT against a URI as text.</summary>
		/// <param name="req">The request.</param>
		/// <param name="body">The body.</param>
		/// <param name="encoding">The content encoding.</param>
		/// <param name="after">The continuation to be executed after.</param>
		public static void HttpPutText(this HttpWebRequest req, string body, Encoding encoding,
			Action<Exception, HttpWebResponse> after)
		{
			var lclEncoding = encoding ?? Encoding.UTF8;
			var buffer = lclEncoding.GetBytes(body);

			ExecuteHttpVerbWithPostBody(req, buffer, "text/plain", "PUT", after);
		}

		/// <summary>
		///   Performs an HTTP PUT against a URI as XML.
		/// </summary>
		/// <param name="req">the http request</param>
		/// <param name="body">the post body</param>
		/// <param name="after">an action to be called upon completion</param>
		public static void HttpPutXml(this HttpWebRequest req, XElement body, Action<Exception, HttpWebResponse> after)
		{
			HttpPutXml(req, body, Encoding.UTF8, after);
		}

		/// <summary>
		///   Performs an HTTP PUT against a URI as XML.
		/// </summary>
		/// <param name="req">the http request</param>
		/// <param name="body">the post body</param>
		/// <param name="encoding">the content encoding</param>
		/// <param name="after">an action to be called upon completion</param>
		public static void HttpPutXml(this HttpWebRequest req, XElement body, Encoding encoding,
			Action<Exception, HttpWebResponse> after)
		{
			Contract.Requires(req != null);
			Contract.Requires(body != null);
			Contract.Requires(after != null);

			var enc = encoding ?? Encoding.UTF8;

			var bodyAsString = body.ToString();
			var buffer = enc.GetBytes(bodyAsString);

			ExecuteHttpVerbWithPostBody(req, buffer, "application/xml", "PUT", after);
		}

		/// <summary>
		///   Determines if an HTTP status code indicates succes (within the 200 range).
		/// </summary>
		/// <param name="code"></param>
		/// <returns></returns>
		public static bool IsSuccess(this HttpStatusCode code)
		{
			var c = (int) code;
			return c >= 200 && c < 300;
		}

		/// <summary>
		///   Given a URI, makes a web request.
		/// </summary>
		/// <param name="uri">the URI</param>
		/// <returns>a web request</returns>
		public static HttpWebRequest MakeResourceRequest(this Uri uri)
		{
			return MakeResourceRequest(uri, false);
		}

		/// <summary>
		///   Given a URI, makes a web request.
		/// </summary>
		/// <param name="uri">the URI</param>
		/// <param name="keepAlive">indicates whether keepalive should be set for the connection</param>
		/// <returns>a web request</returns>
		public static HttpWebRequest MakeResourceRequest(this Uri uri, bool keepAlive)
		{
			Contract.Requires<ArgumentNullException>(uri != null);
			Contract.Requires<ArgumentException>(uri.Scheme != null && uri.Scheme.StartsWith("http"),
																					"URI must be http(s) scheme");
			Contract.Ensures(Contract.Result<HttpWebRequest>() != null);

			var req = (HttpWebRequest) WebRequest.Create(uri);
			if (!keepAlive)
			{
				// Default to keepAlive == false
				req.KeepAlive = false;
			}
			if (String.IsNullOrEmpty(req.UserAgent))
			{
				req.UserAgent = ResourceClientString;
			}
			if (String.IsNullOrEmpty(req.Accept))
			{
				req.Accept = DefaultAcceptHeader;
			}
			return req;
		}

		/// <summary>
		///   Perfroms an HTTP DELETE.
		/// </summary>
		/// <param name="req">the web request on which to perform the DELETE.</param>
		/// <param name="responseHandler">a response handler that will read/interpret the response</param>
		/// <returns>a completion</returns>
		public static Completion<T> ParallelDelete<T>(this HttpWebRequest req,
			Func<HttpWebResponse, T> responseHandler)
		{
			Contract.Requires(req != null);
			Contract.Ensures(Contract.Result<Completion<T>>() != null);

			return ParallelExecuteHttpVerb(req, "DELETE", responseHandler);
		}

		/// <summary>
		///   Performs an HTTP GET against a URI.
		/// </summary>
		/// <param name="uri">the uri</param>
		/// <param name="responseHandler">a response handler that will read/interpret the response</param>
		/// <returns>a completion</returns>
		public static Completion<T> ParallelGet<T>(this Uri uri, Func<HttpWebResponse, T> responseHandler)
		{
			Contract.Requires<ArgumentNullException>(uri != null);
			Contract.Ensures(Contract.Result<Completion<T>>() != null);
			Contract.Assert(uri.Scheme != null && uri.Scheme.StartsWith("http"), "URI must be http(s) scheme");

			return uri.MakeResourceRequest()
								.ParallelGet(responseHandler);
		}

		/// <summary>
		///   Perfroms an HTTP GET.
		/// </summary>
		/// <param name="req">the web request on which to perform the GET.</param>
		/// <param name="responseHandler">a response handler that will read/interpret the response</param>
		/// <returns>a completion</returns>
		public static Completion<T> ParallelGet<T>(this HttpWebRequest req, Func<HttpWebResponse, T> responseHandler)
		{
			Contract.Requires(req != null);
			Contract.Ensures(Contract.Result<Completion<T>>() != null);

			return ParallelExecuteHttpVerb(req, "GET", responseHandler);
		}

		/// <summary>
		///   Perfroms an HTTP POST.
		/// </summary>
		/// <param name="req">the web request on which to perform the POST.</param>
		/// <param name="postBody">array of bytes containing the post body</param>
		/// <param name="contentType">indicates the post body's content type</param>
		/// <param name="responseHandler">a response handler that will read/interpret the response</param>
		/// <returns>a completion</returns>
		public static Completion<T> ParallelPost<T>(this HttpWebRequest req,
			byte[] postBody,
			string contentType,
			Func<HttpWebResponse, T> responseHandler)
		{
			Contract.Requires<ArgumentNullException>(req != null);
			Contract.Requires<ArgumentNullException>(postBody != null);
			Contract.Requires<ArgumentNullException>(contentType != null);
			Contract.Requires<ArgumentException>(contentType.Length > 0);
			Contract.Ensures(Contract.Result<Completion<T>>() != null);

			return ParallelExecuteHttpVerbWithPostBody(req, postBody, contentType, "POST", responseHandler);
		}

		/// <summary>
		///   Perfroms an HTTP PUT.
		/// </summary>
		/// <param name="req">the web request on which to perform the PUT.</param>
		/// <param name="postBody">array of bytes containing the post body</param>
		/// <param name="contentType">indicates the post body's content type</param>
		/// <param name="responseHandler">a response handler that will read/interpret the response</param>
		/// <returns>a completion</returns>
		public static Completion<T> ParallelPut<T>(this HttpWebRequest req,
			byte[] postBody,
			string contentType,
			Func<HttpWebResponse, T> responseHandler)
		{
			Contract.Requires<ArgumentNullException>(req != null);
			Contract.Requires<ArgumentNullException>(postBody != null);
			Contract.Requires<ArgumentNullException>(contentType != null);
			Contract.Requires<ArgumentException>(contentType.Length > 0);
			Contract.Ensures(Contract.Result<Completion<T>>() != null);

			return ParallelExecuteHttpVerbWithPostBody(req, postBody, contentType, "PUT", responseHandler);
		}

		/// <summary>
		///   Adds HTTP Basic Auth to the request.
		/// </summary>
		/// <param name="req">the request</param>
		/// <param name="username">a username</param>
		/// <param name="password">a password</param>
		/// <returns></returns>
		public static HttpWebRequest WithBasicAuth(this HttpWebRequest req, string username, string password)
		{
			Contract.Requires<ArgumentNullException>(req != null);
			Contract.Requires<ArgumentNullException>(username != null);
			Contract.Requires<ArgumentException>(username.Length > 0);
			Contract.Requires<ArgumentNullException>(password != null);
			Contract.Requires<ArgumentException>(password.Length > 0);
			Contract.Ensures(Contract.Result<HttpWebRequest>() != null);

			var authorization = String.Concat(username, ':', password);
			req.Headers["Authorization"] =
				String.Concat("Basic ", Convert.ToBase64String(Encoding.UTF8.GetBytes(authorization)));
			return req;
		}

		static void ExecuteHttpVerb(HttpWebRequest req, string verb, Action<Exception, HttpWebResponse> after)
		{
			Contract.Requires<ArgumentNullException>(req != null);
			Contract.Requires<ArgumentNullException>(after != null);

			WebResponse resp = null;
			try
			{
				req.Method = verb;
				try
				{
					resp = req.GetResponse();
				}
				catch (WebException wex)
				{
					if (resp == null)
					{
						after(wex, wex.Response as HttpWebResponse);
					}
				}
				after(null, (HttpWebResponse) resp);
			}
			finally
			{
				if (resp != null)
				{
					resp.Close();
				}
			}
		}

		static void ExecuteHttpVerbWithPostBody(HttpWebRequest req, byte[] postBody, string contentType, string verb,
			Action<Exception, HttpWebResponse> after)
		{
			Contract.Requires<ArgumentNullException>(req != null);
			Contract.Requires<ArgumentNullException>(postBody != null);
			Contract.Requires<ArgumentNullException>(contentType != null);
			Contract.Requires<ArgumentException>(contentType.Length > 0);
			Contract.Requires<ArgumentNullException>(after != null);

			WebResponse resp = null;
			try
			{
				req.Method = verb;
				req.ContentType = contentType;
				req.ContentLength = postBody.Length;

				using (var postData = req.GetRequestStream())
				{
					postData.Write(postBody, 0, postBody.Length);
				}
			}
			catch (Exception e)
			{
				after(e, null);
				return;
			}
			try
			{
				try
				{
					resp = req.GetResponse();
				}
				catch (WebException wex)
				{
					if (resp == null)
					{
						after(wex, wex.Response as HttpWebResponse);
					}
				}
				after(null, (HttpWebResponse) resp);
			}
			finally
			{
				if (resp != null)
				{
					resp.Close();
				}
			}
		}

		static Completion<T> ParallelExecuteHttpVerb<T>(this HttpWebRequest req, string httpVerb,
			Func<HttpWebResponse, T> responseHandler)
		{
			Contract.Requires<ArgumentNullException>(req != null);
			Contract.Ensures(Contract.Result<Completion<T>>() != null);

			req.Method = httpVerb;

			var completion = new Completion<T>(req);

			req.BeginGetResponse(completion.MakeAsyncCallback(req,
																												(ar, rq) =>
																												{
																													using (var res = (HttpWebResponse) rq.EndGetResponse(ar))
																													{
																														return (responseHandler != null) ? responseHandler(res) : default(T);
																													}
																												}
														), null);

			return completion;
		}

		static Completion<T> ParallelExecuteHttpVerbWithPostBody<T>(this HttpWebRequest req,
			byte[] postBody,
			string contentType,
			string httpVerb,
			Func<HttpWebResponse, T> responseHandler)
		{
			Contract.Requires<ArgumentNullException>(req != null);
			Contract.Requires<ArgumentNullException>(postBody != null);
			Contract.Requires<ArgumentNullException>(contentType != null);
			Contract.Requires<ArgumentException>(contentType.Length > 0);
			Contract.Ensures(Contract.Result<Completion<T>>() != null);

			req.Method = httpVerb;
			req.ContentType = contentType;
			req.ContentLength = postBody.Length;

			using (var postData = req.GetRequestStream())
			{
				postData.Write(postBody, 0, postBody.Length);
			}

			var completion = new Completion<T>(req);

			req.BeginGetResponse(completion.MakeAsyncCallback(req,
																												(ar, rq) =>
																												{
																													using (var res = (HttpWebResponse) rq.EndGetResponse(ar))
																													{
																														return (responseHandler != null) ? responseHandler(res) : default(T);
																													}
																												}
														), null);

			return completion;
		}
	}
}