using System;
using System.Collections;
using System.Collections.Generic;

using System.Net;
using System.Threading.Tasks;
using System.Text;

namespace CheckinShared
{
	public class Facebook
	{
		public string AppToken { get; set; }
		public string AppSecret { get; set; }
		public string UserToken { get; set; }

		public Facebook (string appToken, string appSecret)
		{
			AppToken = appToken;
			AppSecret = appSecret;
		}

		async public Task<string> Me() {
			string url = "https://graph.facebook.com/v2.1/me?access_token=" + UserToken;

			WebRequest request = WebRequest.Create (url);
			request.UseDefaultCredentials = true;

			request.Method = "GET";

			Task<string> response = null;

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

		async public Task<string> PublishFeed(object parameters) {
			var parametersDictionary = parameters as IDictionary<string, string>;

			if (parametersDictionary == null) {
				if (parameters == null) {
					parametersDictionary = new Dictionary<string, string>();
				}
			}

			string url = "https://graph.facebook.com/v2.1/me/feed?access_token=" + UserToken;
			string content = "access_token=" + UserToken;

			var stringBuilder = new StringBuilder ();
			stringBuilder.AppendFormat ("{0}={1}&", "access_token", UserToken);

			foreach (var parameter in parametersDictionary) {
				stringBuilder.AppendFormat ("{0}={1}&", parameter.Key, parameter.Value);
			}

			if (stringBuilder.Length > 0) {
				stringBuilder.Length--;
			}

//			content = "message=" + parametersDictionary["message"];
//			content += "&link=https://www.themoviedb.org/movie/10658-howard-the-duck";
//			content += "&picture=https://image.tmdb.org/t/p/original/gEaC5qL3Q6LDb9XS0Rp27hwoglm.jpg";
//			content += "&name=Howard the Duck";
//			content += "&caption=1986";
//			content += "&description=A scientific experiment unknowingly brings extraterrestrial life forms to the Earth through a laser beam. First is the cigar smoking drake Howard from the duck's planet. A few kids try to keep him from the greedy scientists and help him back to his planet. But then a much less friendly being arrives through the beam...";
//			content += "&privacy[value]=SELF";

			content = stringBuilder.ToString ();

			Console.WriteLine (url);
			Console.WriteLine (content);

			byte[] byteContent = Encoding.UTF8.GetBytes(content);

			WebRequest request = WebRequest.Create (url);
			request.UseDefaultCredentials = true;

			request.Method = "POST";
			request.ContentType = "application/x-www-form-urlencoded";
			request.ContentLength = byteContent.Length;

			Task<string> response = null;

			try {
				System.IO.Stream stream = await request.GetRequestStreamAsync ().ConfigureAwait (false);
				stream.Write (byteContent, 0, byteContent.Length);
				stream.Close ();

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
	}
}

