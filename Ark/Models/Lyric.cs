using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ark.Models
{
    public class Lyric
    {
        public int ID { get; set; }
        public string Line { get; set; }
        public string Text { get; set; }
        public LyricType Type { get; set; }
    }

    public enum LyricType { Bridge, Stanza, Chorus }
}
