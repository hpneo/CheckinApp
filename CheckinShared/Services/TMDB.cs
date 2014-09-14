using System;

using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace CheckinShared.Services
{
	public class TMDB
	{
		private string API_KEY;
		public TMDB ()
		{
			API_KEY = "fdf3c94669f3cc0906fddc99e5cd8208";
		}

		public async Task<object> searchMovies(string query) {
			var httpClient = new WebClient ();
			Task<string> contentTask = httpClient.DownloadStringTaskAsync (new Uri ("https://api.themoviedb.org/3/search/movie?api_key=" + API_KEY +"&query=" + query));

			string contentJSON = await contentTask;

			var content = Newtonsoft.Json.JsonConvert.DeserializeObject (contentJSON);

			return content;
		}
	}
}

