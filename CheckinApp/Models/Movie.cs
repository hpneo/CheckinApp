using System;

using SQLite;

using Android.Graphics;

namespace CheckinApp
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
		public Bitmap Poster { get; set; }

		public Movie ()
		{
		}
	}
}

