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

        public event Action OnChange;

        private void NotifyDataChanged() => OnChange?.Invoke();
    }
}
