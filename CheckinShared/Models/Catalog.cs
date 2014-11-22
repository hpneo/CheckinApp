using System;
using System.Collections.Generic;

using SQLite;

using Parse;

namespace CheckinShared.Models
{
	[Table ("Catalog")]
	public class Catalog
	{
		[PrimaryKey, AutoIncrement, Column ("_id")]
		public int Id { get; set; }

		[Column ("Name")] 
		public string Name { get; set; }

		[Column ("UserId")] 
		public int UserId { get; set; }

		[Column ("Quantity")] 
		public int Quantity { get; set; }

		[Column ("ParseId")] 
		public string ParseId { get; set; }

		public User User {
			get {
				if (((int)this.UserId) == 0) {
					return null;
				} else {
					UserDB userDB = new UserDB ();
					return userDB.Get (this.UserId);
				}
			}
		}

		public Catalog ()
		{
		}

		async public void SaveToParse ()
		{
			ParseObject catalog;
			if (this.ParseId == null || this.ParseId == "") {
				catalog = new ParseObject ("Catalogo");
			} else {
				ParseQuery<ParseObject> query = ParseObject.GetQuery ("Catalogo");
				catalog = await query.GetAsync (this.ParseId);
			}

			IList<string> movies = new List<string> ();
			MoviexCatalogDB moviexCatalogDB = new MoviexCatalogDB ();
			foreach (MoviexCatalog moviexCatalog in moviexCatalogDB.All ().Where (mxc => mxc.IdCatalog.Equals (this.Id))) {
				moviexCatalog.SaveToParse ();
				movies.Add (moviexCatalog.ParseId);
			}

			if (this.User != null) {
				catalog ["Usuario"] = this.User.ParseId;
			}
			catalog ["Peliculas"] = movies;

			await catalog.SaveAsync ().ContinueWith (t => {
				this.ParseId = catalog.ObjectId;
				Console.WriteLine("Saved Catalog in Parse: " + this.ParseId);
				CatalogDB catalogDB = new CatalogDB ();
				catalogDB.Update (this);
			});
		}
	}
}