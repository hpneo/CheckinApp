using System;

using SQLite;

namespace CheckinShared.Models
{
	[Table("MoviexCatalog")]
	public class MoviexCatalog
	{
		[PrimaryKey, AutoIncrement, Column("_id")]
		public int Id { get; set; }
		[Column("IdMovie")] 
		public int IdMovie { get; set; }
		[Column("IdCatalog")] 
		public int IdCatalog { get; set; }
		public string PhotoPath { get; set; }

		public MoviexCatalog ()
		{
		}
	}
}