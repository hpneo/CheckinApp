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

		[Column ("MovieType")] 
		public string MovieType { get; set; }

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
			ParseObject moviexCatalog;
			if (this.ParseId == null || this.ParseId == "") {
				moviexCatalog = new ParseObject ("Catalogo_detalle");
			} else {
				ParseQuery<ParseObject> query = ParseObject.GetQuery ("Catalogo_detalle");
				moviexCatalog = await query.GetAsync (this.ParseId);
			}

			moviexCatalog ["Pelicula"] = this.Movie.ParseId;

			await moviexCatalog.SaveAsync ().ContinueWith (t => {
				this.ParseId = moviexCatalog.ObjectId;
				Console.WriteLine("Saved MoviexCatalog in Parse: " + this.ParseId);
				MoviexCatalogDB moviexCatalogDB = new MoviexCatalogDB ();
				moviexCatalogDB.Update (this);
			});
		}
	}
}