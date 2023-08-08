using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ark.Models.Songs
{
    public class Song
    {
        [PrimaryKey, AutoIncrement] public int ID { get; set; }
        public int Number { get; set; }
        public string Language { get; set; } = "";
        public string Title { get; set; } = "";
        public string Author { get; set; } = "";
        public string RawLyrics { get; set; } = "";
        public string Sequence { get; set; } = "";
        public string Tags { get; set; } = "";
        [Ignore] public List<Lyric> Lyrics { get; set; }
        [Ignore] public string LyricHighlighted { get; set; }
        [Ignore] public string TitleHighlighted { get; set; }
    }

    public class SongFts : Song { }
}
