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
        public const string BibleDatabase = "Bible.db";
        public const SQLiteOpenFlags Flags = SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.SharedCache;

        public static string SongDbPath
        {
            get
            {
                var basePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                return basePath + SongDatabase;
            }
        }
        public static string BibleDbPath
        {
            get
            {
                var basePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                return basePath + BibleDatabase;
            }
        }

        public static string webAPI = "https://ark.welchengine.com";
    }
}
