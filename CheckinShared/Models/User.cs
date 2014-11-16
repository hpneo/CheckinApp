using System;

using SQLite;

using Parse;

namespace CheckinShared.Models
{
	[Table ("User")]
	public class User
	{
		[PrimaryKey, AutoIncrement, Column ("_id")]
		public int Id { get; set; }

		[Column ("Facebook")] 
		public string Facebook { get; set; }

		[Column ("Twitter")] 
		public string Twitter { get; set; }

		[Column ("ParseId")] 
		public string ParseId { get; set; }

		public User ()
		{
		}

		async public void SaveToParse ()
		{
			ParseObject user = new ParseObject ("Usuario");

			user ["Usuario_facebook"] = this.Facebook;
			user ["Usuario_twitter"] = this.Twitter;

			await user.SaveAsync ();

			this.ParseId = user.ObjectId;
			UserDB userDB = new UserDB ();
			userDB.Update (this);
		}
	}
}

