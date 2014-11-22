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

		[Column ("Latitude")] 
		public double Latitude { get; set; }

		[Column ("Longitude")] 
		public double Longitude { get; set; }

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
			ParseObject checkin;
			if (this.ParseId == null || this.ParseId == "") {
				checkin = new ParseObject ("Checkin");
			} else {
				ParseQuery<ParseObject> query = ParseObject.GetQuery ("Checkin");
				checkin = await query.GetAsync (this.ParseId);
			}

			if (this.Movie != null) {
				checkin ["Pelicula"] = this.Movie.ParseId;
			}
			if (this.User != null) {
				checkin ["Usuario"] = this.User.ParseId;
			}
			checkin ["Coordenadas"] = new Parse.ParseGeoPoint (this.Latitude, this.Longitude);

			await checkin.SaveAsync ().ContinueWith (t => {
				this.ParseId = checkin.ObjectId;
				Console.WriteLine("Saved Checkin in Parse: " + this.ParseId);
				CheckinDB checkinDB = new CheckinDB ();
				checkinDB.Update (this);
			});
		}
	}
}