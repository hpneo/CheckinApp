using System;
using System.IO;
using SQLite;
using CheckinShared.Models;

namespace CheckinShared
{
	public class CatalogDB
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

		public CatalogDB ()
		{
			database = new SQLiteConnection (DatabasePath);

			database.CreateTable<Catalog> ();
		}

		public Catalog Insert(Catalog catalog) {
			lock (locker) {
				database.Insert (catalog);
				return catalog;
			}
		}

		public bool Update(Catalog catalog) {
			lock (locker) {
				int flag = database.Update (catalog);

				if (flag == 1) {
					return true;
				} else {
					return false;
				}
			}
		}

		public bool Delete(Catalog catalog) {
			lock (locker) {
				int flag = database.Delete<Catalog> (catalog.Id);

				if (flag == 1) {
					return true;
				} else {
					return false;
				}
			}
		}

		public Catalog Get(int id) {
			lock (locker) {
				return database.Get<Catalog> (id);
			}
		}

		public int Count() {
			lock (locker) {
				return database.Table<Catalog> ().Count ();
			}
		}

		public TableQuery<Catalog> All() {
			lock (locker) {
				return database.Table<Catalog> ();
			}
		}
	}
}

