using System;
using System.IO;
using SQLite;
using CheckinShared.Models;

namespace CheckinShared
{
	public class UserDB
	{
		static object locker = new object ();
		SQLiteConnection database;

		string DatabasePath {
			get { 
				var sqliteFilename = "movies.db3";
				#if __IOS__
				string documentsPath = Environment.GetFolderPath (Environment.SpecialFolder.Personal); // Documents folder
				string libraryPath = Path.Combine (documentsPath, "..", "Library"); // Library folder
				var path = Path.Combine(libraryPath, sqliteFilename);
				#else
				#if __ANDROID__
				string documentsPath = Environment.GetFolderPath (Environment.SpecialFolder.Personal); // Documents folder
				var path = Path.Combine(documentsPath, sqliteFilename);
				#else
				// WinPhone
				var path = Path.Combine(ApplicationData.Current.LocalFolder.Path, sqliteFilename);;
				#endif
				#endif
				return path;
			}
		}

		public UserDB ()
		{
			database = new SQLiteConnection (DatabasePath);

			database.CreateTable<User> ();
		}

		public User Insert(User user) {
			lock (locker) {
				database.Insert (user);
				return user;
			}
		}

		public bool Update(User user) {
			lock (locker) {
				int flag = database.Update (user);

				if (flag == 1) {
					return true;
				} else {
					return false;
				}
			}
		}

		public bool Delete(User user) {
			lock (locker) {
				int flag = database.Delete<User> (user.Id);

				if (flag == 1) {
					return true;
				} else {
					return false;
				}
			}
		}

		public User Get(int id) {
			lock (locker) {
				return database.Get<User> (id);
			}
		}

		public int Count() {
			lock (locker) {
				return database.Table<User> ().Count ();
			}
		}

		public TableQuery<User> All() {
			lock (locker) {
				return database.Table<User> ();
			}
		}
	}
}

