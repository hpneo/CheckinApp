using System;
using System.IO;
using SQLite;
using CheckinShared.Models;

namespace CheckinShared
{
	public class MovieDB
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

		public MovieDB ()
		{
			database = new SQLiteConnection (DatabasePath);

			database.CreateTable<Movie> ();
		}

		public Movie Insert(Movie movie) {
			lock (locker) {
				database.Insert (movie);
				return movie;
			}
		}

		public bool Update(Movie movie) {
			lock (locker) {
				int flag = database.Update (movie);

				if (flag == 1) {
					return true;
				} else {
					return false;
				}
			}
		}

		public bool Delete(Movie movie) {
			lock (locker) {
				int flag = database.Delete<Movie> (movie.Id);

				if (flag == 1) {
					return true;
				} else {
					return false;
				}
			}
		}

		public Movie Get(int id) {
			lock (locker) {
				return database.Get<Movie> (id);
			}
		}

		public int Count() {
			lock (locker) {
				return database.Table<Movie> ().Count ();
			}
		}

		public TableQuery<Movie> All() {
			lock (locker) {
				return database.Table<Movie> ();
			}
		}
	}
}

