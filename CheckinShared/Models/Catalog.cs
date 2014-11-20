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
			if (this.ParseId + "" == "") {
				IList<string> movies = new List<string> ();
				MoviexCatalogDB moviexCatalogDB = new MoviexCatalogDB ();
				foreach (MoviexCatalog moviexCatalog in moviexCatalogDB.All ().Where (mxc => mxc.IdCatalog.Equals (this.Id))) {
					moviexCatalog.SaveToParse ();
					movies.Add (moviexCatalog.ParseId);
				}

				ParseObject catalog = new ParseObject ("Catalogo");

				catalog ["Usuario"] = this.User.ParseId;
				catalog ["Peliculas"] = movies;

				await catalog.SaveAsync ();

				this.ParseId = catalog.ObjectId;
				CatalogDB catalogDB = new CatalogDB ();
				catalogDB.Update (this);
			}
		}
	}
}