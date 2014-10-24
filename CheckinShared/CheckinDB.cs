using System;
using System.IO;
using SQLite;
using CheckinShared.Models;

namespace CheckinShared
{
	public class CheckinDB
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

		public CheckinDB ()
		{
			database = new SQLiteConnection (DatabasePath);

			database.CreateTable<Checkin> ();
		}

		public Checkin Insert(Checkin checkin) {
			lock (locker) {
				database.Insert (checkin);
				return checkin;
			}
		}

		public bool Update(Checkin checkin) {
			lock (locker) {
				int flag = database.Update (checkin);

				if (flag == 1) {
					return true;
				} else {
					return false;
				}
			}
		}

		public bool Delete(Checkin checkin) {
			lock (locker) {
				int flag = database.Delete<Checkin> (checkin.Id);

				if (flag == 1) {
					return true;
				} else {
					return false;
				}
			}
		}

		public Checkin Get(int id) {
			lock (locker) {
				return database.Get<Checkin> (id);
			}
		}

		public int Count() {
			lock (locker) {
				return database.Table<Checkin> ().Count ();
			}
		}

		public TableQuery<Checkin> All() {
			lock (locker) {
				return database.Table<Checkin> ();
			}
		}
	}
}

