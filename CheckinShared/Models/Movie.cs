using System;

using SQLite;

using Parse;

namespace CheckinShared.Models
{
	[Table ("Movies")]
	public class Movie
	{
		[PrimaryKey, AutoIncrement, Column ("_id")]
		public int Id { get; set; }

		[Column ("Title")] 
		public string Title { get; set; }

		[Column ("PosterPath")] 
		public string PosterPath { get; set; }

		[Column ("Year")] 
		public string Year { get; set; }

		[Column ("ApiId")] 
		public string ApiId { get; set; }

		[Column ("Overview")] 
		public string Overview { get; set; }

		[Column ("Director")] 
		public string Director { get; set; }

		[Column ("Cast")] 
		public string Cast { get; set; }

		[Column ("ParseId")] 
		public string ParseId { get; set; }

		[Ignore]
		public object Poster { get; set; }

		public Movie ()
		{
		}

		async public void SaveToParse ()
		{
			if (this.ParseId + "" == "") {
				ParseObject movie = new ParseObject ("Pelicula");

				movie ["Nombre"] = this.Title;
				movie ["Nombre_Original"] = this.Title;
				movie ["Anio"] = int.Parse (this.Year);
				movie ["Director"] = this.Director;
				movie ["ID_TMDB"] = this.ApiId;
				movie ["Descripcion"] = this.Overview;

				await movie.SaveAsync ();

				this.ParseId = movie.ObjectId;
				MovieDB movieDB = new MovieDB ();
				movieDB.Update (this);
			}
		}
	}
}