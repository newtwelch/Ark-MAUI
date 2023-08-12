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
        // Escape: Display blank do not close Window : TRUE
        // Single Window Mode                        : FALSE(Desktop), TRUE(Mobile)
        // Display to Main Monitor                   : FALSE
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

        // Use true black background on Projector    : TRUE
        private bool trueBlackBackground;
        public bool TrueBlackBackground
        {
            get { return trueBlackBackground; }
            set { trueBlackBackground = value; }
        }

        // ( Mobile ) Auto landscape                 : TRUE
        // 

    }
}
