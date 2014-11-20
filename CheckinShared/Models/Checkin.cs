using System;

using SQLite;

using Parse;

namespace CheckinShared.Models
{
	[Table ("Checkins")]
	public class Checkin
	{
		[PrimaryKey, AutoIncrement, Column ("_id")]
		public int Id { get; set; }

		[Column ("MovieId")] 
		public int MovieId { get; set; }

		[Column ("UserId")] 
		public int UserId { get; set; }

		[Column ("CreatedAt")] 
		public DateTime CreatedAt { get; set; }

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

		public Checkin ()
		{
		}

		public Movie Movie {
			get {
				if (((int)this.MovieId) == 0) {
					return null;
				} else {
					MovieDB movieDB = new MovieDB ();
					return movieDB.Get (this.MovieId);
				}
			}
		}

		async public void SaveToParse ()
		{
			ParseObject checkin = new ParseObject ("Checkin");

			checkin ["Pelicula"] = this.Movie.ParseId;
			checkin ["Usuario"] = this.User.ParseId;

			await checkin.SaveAsync ();

			this.ParseId = checkin.ObjectId;
			CheckinDB checkinDB = new CheckinDB ();
			checkinDB.Update (this);
		}
	}
}