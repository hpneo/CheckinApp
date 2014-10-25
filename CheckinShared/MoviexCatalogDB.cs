using System;
using System.IO;
using SQLite;
using CheckinShared.Models;

namespace CheckinShared
{
	public class MoviexCatalogDB
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

		public MoviexCatalogDB ()
		{
			database = new SQLiteConnection (DatabasePath);

			database.CreateTable<MoviexCatalog> ();
		}

		public MoviexCatalog Insert(MoviexCatalog moviexcatalog) {
			lock (locker) {
				database.Insert (moviexcatalog);
				return moviexcatalog;
			}
		}

		public bool Update(MoviexCatalog moviexcatalog) {
			lock (locker) {
				int flag = database.Update (moviexcatalog);

				if (flag == 1) {
					return true;
				} else {
					return false;
				}
			}
		}

		public bool Delete(MoviexCatalog moviexcatalog) {
			lock (locker) {
				int flag = database.Delete<MoviexCatalog> (moviexcatalog.Id);

				if (flag == 1) {
					return true;
				} else {
					return false;
				}
			}
		}

		public MoviexCatalog Get(int id) {
			lock (locker) {
				return database.Get<MoviexCatalog> (id);
			}
		}

		public int Count() {
			lock (locker) {
				return database.Table<MoviexCatalog> ().Count ();
			}
		}

		public TableQuery<MoviexCatalog> All() {
			lock (locker) {
				return database.Table<MoviexCatalog> ();
			}
		}
	}
}

