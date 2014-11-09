using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;

using System.Net;
using System.Threading.Tasks;
using System.Text;

namespace CheckinShared
{
	public class Twitter
	{
		public string AppToken { get; set; }
		public string AppSecret { get; set; }
		public string UserToken { get; set; }
		public string UserSecret { get; set; }

		public Twitter (string appToken, string appSecret)
		{
			AppToken = appToken;
			AppSecret = appSecret;
		}

		private string AuthorizationHeader(string status) {
			var oauth_token = UserToken;
			var oauth_token_secret = UserSecret;
			var oauth_consumer_key = AppToken;
			var oauth_consumer_secret = AppSecret;

			var oauth_version          = "1.0";
			var oauth_signature_method = "HMAC-SHA1";
			var oauth_nonce            = Convert.ToBase64String(
				new ASCIIEncoding().GetBytes(
					DateTime.Now.Ticks.ToString()));
			var timeSpan               = DateTime.UtcNow
				- new DateTime(1970, 1, 1, 0, 0, 0, 0,
					DateTimeKind.Utc);
			var oauth_timestamp        = Convert.ToInt64(timeSpan.TotalSeconds).ToString();
			var resource_url           = "https://api.twitter.com/1.1/statuses/update.json";

			var baseFormat = "oauth_consumer_key={0}&oauth_nonce={1}&oauth_signature_method={2}" +
				"&oauth_timestamp={3}&oauth_token={4}&oauth_version={5}&status={6}";

			var baseString = string.Format(baseFormat,
				oauth_consumer_key,
				oauth_nonce,
				oauth_signature_method,
				oauth_timestamp,
				oauth_token,
				oauth_version,
				Uri.EscapeDataString(status)
			);

			baseString = string.Concat("POST&", Uri.EscapeDataString(resource_url), 
				"&", Uri.EscapeDataString(baseString));

			var compositeKey = string.Concat(Uri.EscapeDataString(oauth_consumer_secret),
				"&",  Uri.EscapeDataString(oauth_token_secret));

			string oauth_signature;
			using (HMACSHA1 hasher = new HMACSHA1(ASCIIEncoding.ASCII.GetBytes(compositeKey)))
			{
				oauth_signature = Convert.ToBase64String(
					hasher.ComputeHash(ASCIIEncoding.ASCII.GetBytes(baseString)));
			}

			var headerFormat = "OAuth oauth_nonce=\"{0}\", oauth_signature_method=\"{1}\", " +
				"oauth_timestamp=\"{2}\", oauth_consumer_key=\"{3}\", " +
				"oauth_token=\"{4}\", oauth_signature=\"{5}\", " +
				"oauth_version=\"{6}\"";

			var authHeader = string.Format(headerFormat,
				Uri.EscapeDataString(oauth_nonce),
				Uri.EscapeDataString(oauth_signature_method),
				Uri.EscapeDataString(oauth_timestamp),
				Uri.EscapeDataString(oauth_consumer_key),
				Uri.EscapeDataString(oauth_token),
				Uri.EscapeDataString(oauth_signature),
				Uri.EscapeDataString(oauth_version)
			);

			return authHeader;
		}

		async private Task<string> Request(string url, string method, object parameters) {
			WebRequest request = WebRequest.Create ("https://api.twitter.com/1.1" + url + ".json");
			// request.UseDefaultCredentials = true;
			ServicePointManager.Expect100Continue = false;

			request.Method = method;

			string response = "";

			if (request.Method == "POST") {
				Dictionary<string, string> parametersDictionary = new Dictionary<string, string> ();

				Console.WriteLine ("parameters: " + parameters);

				if (parameters != null) {
					var properties = parameters.GetType ().GetProperties ();

					for (int i = 0; i < properties.Length; i++) {
						Console.WriteLine (properties [i].Name);
						parametersDictionary.Add (properties [i].Name, properties [i].GetValue (parameters, null).ToString ());
					}
				}

				Console.WriteLine ("parameters: " + parameters);
				Console.WriteLine ("parametersDictionary: " + parametersDictionary);

				Console.WriteLine (parametersDictionary.Keys.Count + " keys");
				Console.WriteLine (parametersDictionary.Values.Count + " values");

				string content = "";

				var stringBuilder = new StringBuilder ();

				foreach (var parameter in parametersDictionary) {
					stringBuilder.AppendFormat ("{0}={1}&", parameter.Key, Uri.EscapeDataString(parameter.Value));
				}

				if (stringBuilder.Length > 0) {
					stringBuilder.Length--;
				}

				content = stringBuilder.ToString ();

				Console.WriteLine (url);
				Console.WriteLine (content);

				Console.WriteLine ("Params: " + content);

				byte[] byteContent = Encoding.UTF8.GetBytes(content);

				request.Headers.Add ("Authorization", AuthorizationHeader(parametersDictionary["status"]));
				request.ContentType = "application/x-www-form-urlencoded;charset=UTF-8";
				request.ContentLength = byteContent.Length;

				System.IO.Stream stream = await request.GetRequestStreamAsync ().ConfigureAwait (false);
				stream.Write (byteContent, 0, byteContent.Length);
				stream.Close ();
			}

			try {
				WebResponse webResponse = await request.GetResponseAsync ();
				System.IO.StreamReader requestHeader = new System.IO.StreamReader (webResponse.GetResponseStream ());
				response = await requestHeader.ReadToEndAsync ();
				webResponse.Close ();
			} catch (WebException ex) {
				HttpWebResponse httpResponse = (HttpWebResponse)ex.Response;
				Console.WriteLine (WebExceptionStatus.ProtocolError);
				Console.WriteLine ("StatusCode: " + httpResponse.StatusCode);
				Console.WriteLine ("Status: " + ex.Status);
				Console.WriteLine ("Message: " + ex.Message);
			}

			return response;
		}

		private Task<string> Request(string url, string method) {
			return Request (url, method, null);
		}

		public Task<string> UpdateStatus(object parameters) {
			return Request ("/statuses/update", "POST", parameters);
		}
	}
}

