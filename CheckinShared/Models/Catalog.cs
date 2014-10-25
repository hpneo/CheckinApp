using System;

using SQLite;

namespace CheckinShared.Models
{
	[Table("Catalog")]
	public class Catalog
	{
		[PrimaryKey, AutoIncrement, Column("_id")]
		public int Id { get; set; }
		[Column("Name")] 
		public string Name { get; set; }
		[Column("Quantity")] 
		public int Quantity { get; set; }

		public Catalog ()
		{
		}
	}
}