using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ark.Models
{
    public class DisplayService
    {
        private string lyricToDisplay;
        public string LyricToDisplay 
        { 
            get => lyricToDisplay; 
            set 
            { 
                lyricToDisplay = value;
                NotifyDataChanged(); 
            } 
        }

        private string bibleVerseToDisplay;
        public string BibleVerseToDisplay
        {
            get => bibleVerseToDisplay;
            set
            {
                bibleVerseToDisplay = value;
                NotifyDataChanged();
            }
        }

        private string bibleBookInfoToDisplay;
        public string BibleBookInfoToDisplay
        {
            get => bibleBookInfoToDisplay;
            set
            {
                bibleBookInfoToDisplay = value;
                NotifyDataChanged();
            }
        }

        private bool isBible;
        public bool IsBible
        {
            get => isBible;
            set
            {
                isBible = value;
                NotifyDataChanged();
            }
        }


        public event Action OnChange;
        private void NotifyDataChanged() => OnChange?.Invoke();
    }
}
