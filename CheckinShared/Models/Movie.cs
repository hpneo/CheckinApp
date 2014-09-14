using System;

using SQLite;

namespace CheckinShared.Models
{
	[Table("Movies")]
	public class Movie
	{
		[PrimaryKey, AutoIncrement, Column("_id")]
		public int Id { get; set; }
		[Column("Title")] 
		public string Title { get; set; }
		[Column("PosterPath")] 
		public string PosterPath { get; set; }
		[Ignore]
		public object Poster { get; set; }

		public Movie ()
		{
		}
	}
}