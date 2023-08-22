using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace Ark.Models
{
    public class SettingsService
    {

        public Window secondWindow;
        public DeviceOrientationPartialClass DeviceOrientation;

        // Escape: Display blank do not close Window : TRUE
        // Single Window Mode                        : FALSE(Desktop), TRUE(Mobile)
        // Display to Main Monitor                   : FALSE
        // ( Mobile ) Auto landscape                 : TRUE
        // 

        private string chosenMonitor;
        public string ChosenMonitor
        {
            get { return chosenMonitor; }
            set { chosenMonitor = value; }
        }

        private string devPass = "arkDeveloper005";
        public string DevPass { get; set; }

        public bool DeveloperMode()
        {
            bool isMatch = DevPass == devPass;
            return isMatch;
        }

        private bool trueBlackBackground;
        public bool TrueBlackBackground
        {
            get { return trueBlackBackground; }
            set { trueBlackBackground = value; }
        }

        private bool bibleShowPresentor;
        public bool BibleShowPresentor
        {
            get { return bibleShowPresentor; }
            set { bibleShowPresentor = value; }
        }

        private bool songShowPresentor;
        public bool SongShowPresentor
        {
            get { return songShowPresentor; }
            set { songShowPresentor = value; }
        }

        private bool displayBlankInsteadOfClose;
        public bool DisplayBlankInsteadOfClose
        {
            get { return displayBlankInsteadOfClose; }
            set { displayBlankInsteadOfClose = value; }
        }




        public SettingsService()
        {
            secondWindow = new Window();
            DeviceOrientation = new DeviceOrientationPartialClass();
        }


        //BIBLE SETTINGS
        private bool alwaysEnglishBooks;
        public bool AlwaysEnglishBooks
        {
            get { return alwaysEnglishBooks; }
            set { alwaysEnglishBooks = value; }
        }

    }
}
