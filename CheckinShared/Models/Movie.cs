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
		[Column("Year")] 
		public string Year { get; set; }
		[Column("ApiId")] 
		public string ApiId { get; set; }
		[Column("Overview")] 
		public string Overview { get; set; }
		[Column("Director")] 
		public string Director { get; set; }
		[Ignore]
		public object Poster { get; set; }

		public Movie ()
		{
		}
	}
}