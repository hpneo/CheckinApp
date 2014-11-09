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

		private string AuthorizationHeader(string url, string status) {
			string oauthVersion = "1.0";
			string oauthSignatureMethod = "HMAC-SHA1";

			string oauthNonce = Convert.ToBase64String(new ASCIIEncoding().GetBytes(DateTime.Now.Ticks.ToString()));
			System.TimeSpan timeSpan = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc));
			string oauthTimestamp = Convert.ToInt64(timeSpan.TotalSeconds).ToString();

			string baseFormat = "oauth_consumer_key={0}&oauth_nonce={1}&oauth_signature_method={2}" +
				"&oauth_timestamp={3}&oauth_token={4}&oauth_version={5}&status={6}";

			string baseString = string.Format(
				baseFormat, 
				AppToken, 
				oauthNonce, 
				oauthSignatureMethod, 
				oauthTimestamp,
				UserToken,
				oauthVersion, 
				Uri.EscapeDataString(status)
			);

			string oauth_signature = null;
			using (HMACSHA1 hasher = new HMACSHA1(ASCIIEncoding.ASCII.GetBytes(Uri.EscapeDataString(AppSecret) + "&" + Uri.EscapeDataString(UserSecret)))) {
				oauth_signature = Convert.ToBase64String(hasher.ComputeHash(ASCIIEncoding.ASCII.GetBytes("POST&" + Uri.EscapeDataString(url) + "&" + Uri.EscapeDataString(baseString))));
			}

			// create the request header
			string authorizationFormat = "OAuth oauth_consumer_key=\"{0}\", oauth_nonce=\"{1}\", " + "oauth_signature=\"{2}\", oauth_signature_method=\"{3}\", " +
				"oauth_timestamp=\"{4}\", oauth_token=\"{5}\", " + "oauth_version=\"{6}\"";

			string authorizationHeader = string.Format(
				authorizationFormat, 
				Uri.EscapeDataString(AppToken), 
				Uri.EscapeDataString(oauthNonce),
				Uri.EscapeDataString(oauth_signature), 
				Uri.EscapeDataString(oauthSignatureMethod), 
				Uri.EscapeDataString(oauthTimestamp), 
				Uri.EscapeDataString(UserToken), 
				Uri.EscapeDataString(oauthVersion)
			);

			return authorizationHeader;
		}

		async private Task<string> Request(string url, string method, object parameters) {
			WebRequest request = WebRequest.Create ("https://api.twitter.com/1.1" + url + ".json");
			request.UseDefaultCredentials = true;

			request.Method = method;

			Task<string> response = null;

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
					stringBuilder.AppendFormat ("{0}={1}&", parameter.Key, parameter.Value);
				}

				if (stringBuilder.Length > 0) {
					stringBuilder.Length--;
				}

				content = stringBuilder.ToString ();

				Console.WriteLine (url);
				Console.WriteLine (content);

				byte[] byteContent = Encoding.UTF8.GetBytes(content);

				request.Headers.Add ("Authorization", AuthorizationHeader("https://api.twitter.com/1.1" + url + ".json", parametersDictionary["status"]));
				request.ContentType = "application/x-www-form-urlencoded";
				request.ContentLength = byteContent.Length;

				System.IO.Stream stream = await request.GetRequestStreamAsync ().ConfigureAwait (false);
				stream.Write (byteContent, 0, byteContent.Length);
				stream.Close ();
			}

			try {
				WebResponse webResponse = await request.GetResponseAsync ();
				System.IO.StreamReader requestHeader = new System.IO.StreamReader (webResponse.GetResponseStream ());
				response = requestHeader.ReadToEndAsync ();
				webResponse.Close ();
			} catch (WebException ex) {
				HttpWebResponse httpResponse = (HttpWebResponse)ex.Response;
				Console.WriteLine (WebExceptionStatus.ProtocolError);
				Console.WriteLine (httpResponse.StatusCode);
				Console.WriteLine (ex.Status);
				Console.WriteLine (ex.Message);
			}

			return response.Result;
		}

		async private Task<string> Request(string url, string method) {
			return await Request (url, method, null);
		}

		async public Task<string> UpdateStatus(object parameters) {
			return await Request ("/statuses/update", "POST", parameters);
		}
	}
}

