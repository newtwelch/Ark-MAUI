using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ark
{
    public static class Constants
    {
        public const string SongDatabase = "Songs.db";
        public const SQLiteOpenFlags Flags = SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.SharedCache;

        public static string SongDbPath
        {
            get
            {
                var basePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                return FileSystem.AppDataDirectory + SongDatabase;
            }
        }

        public static string SongIndexPath
        {
            get
            {
                var basePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                return Path.Combine(basePath, "songIndex");
            }
        }
        public static string webAPI = "https://ark.welchengine.com";
    }
}
