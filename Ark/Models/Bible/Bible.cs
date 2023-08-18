using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ark.Models.Bible
{
    [Table("Bibles")]
    public class Bible
    {
        [PrimaryKey, AutoIncrement] public int ID { get; set; }
        public string Translation { get; set; }
        public string Abbreviation { get; set; }
        public string Description { get; set; }
        public string Lang { get; set; }
        public string Language { get; set; }
        public string Direction { get; set; }
        public string Encoding { get; set; }
        [Ignore] public Book[] Books { get; set; }
        public string Distribution_lcsh { get; set; }
        public string Distribution_version { get; set; }
        public string Distribution_version_date { get; set; }
        public string Distribution_abbreviation { get; set; }
        public string Distribution_about { get; set; }
        public string Distribution_license { get; set; }
        public string Distribution_sourcetype { get; set; }
        public string Distribution_source { get; set; }
        public string Distribution_versification { get; set; }
        [Ignore] public Distribution_History distribution_history { get; set; }
    }

    public class Distribution_History
    {
        public string history_14 { get; set; }
        public string history_13 { get; set; }
        public string history_12 { get; set; }
    }

    public class Book
    {
        public int nr { get; set; }
        public int ID { get; set; }
        public string Name { get; set; }
        public List<Chapter> Chapters { get; set; }
    }

    public class Chapter
    {
        public int chapter { get; set; }
        public string Name { get; set; }
        public List<Verse> Verses { get; set; }
    }

    public class Verse
    {
        public int chapter { get; set; }
        public int verse { get; set; }
        public string Name { get; set; }
        public string Text { get; set; }
    }

    public class ArkBible
    {
        public int ID { get; set; }
        public int Book { get; set; }
        public int Chapter { get; set; }
        public int Verse { get; set; }
        public string Text { get; set; }
    }
}
