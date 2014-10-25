using System;

using SQLite;

namespace CheckinShared.Models
{
	[Table("Checkins")]
	public class Checkin
	{
		[PrimaryKey, AutoIncrement, Column("_id")]
		public int Id { get; set; }
		[Column("MovieId")] 
		public int MovieId { get; set; }

		public Checkin ()
		{
		}

		public Movie Movie {
			get {
				if (this.MovieId == null) {
					return null;
				} else {
					MovieDB movieDB = new MovieDB ();
					return movieDB.Get (this.MovieId);
				}
			}
		}
	}
}