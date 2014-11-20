using System;
using System.Threading.Tasks;

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
			Task task;
			ParseObject user;
			if (this.ParseId == null || this.ParseId == "") {
				user = new ParseObject ("Usuario");
			} else {
				ParseQuery<ParseObject> query = ParseObject.GetQuery ("Usuario");
				user = await query.GetAsync (this.ParseId);
			}

			user ["Usuario_facebook"] = this.Facebook;
			user ["Usuario_twitter"] = this.Twitter;

			await user.SaveAsync ();

			this.ParseId = user.ObjectId;
			UserDB userDB = new UserDB ();
			userDB.Update (this);
		}
	}
}

