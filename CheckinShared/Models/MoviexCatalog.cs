using System;

using SQLite;

using Parse;

namespace CheckinShared.Models
{
	[Table ("MoviexCatalog")]
	public class MoviexCatalog
	{
		[PrimaryKey, AutoIncrement, Column ("_id")]
		public int Id { get; set; }

		[Column ("IdMovie")] 
		public int IdMovie { get; set; }

		[Column ("IdCatalog")] 
		public int IdCatalog { get; set; }

		public string PhotoPath { get; set; }

		[Ignore]
		public object Photo { get; set; }

		[Column ("ParseId")]
		public string ParseId { get; set; }

		public MoviexCatalog ()
		{
		}

		public Movie Movie {
			get {
				if (((int)this.IdMovie) == 0) {
					return null;
				} else {
					MovieDB movieDB = new MovieDB ();
					return movieDB.Get (this.IdMovie);
				}
			}
		}

		async public void SaveToParse ()
		{
			if (this.ParseId + "" == "") {
				ParseObject moviexCatalog = new ParseObject ("Catalogo_detalle");

				moviexCatalog ["Pelicula"] = this.Movie.ParseId;

				await moviexCatalog.SaveAsync ();

				this.ParseId = moviexCatalog.ObjectId;
				MoviexCatalogDB moviexCatalogDB = new MoviexCatalogDB ();
				moviexCatalogDB.Update (this);
			}
		}
	}
}