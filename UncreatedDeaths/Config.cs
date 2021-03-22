using Rocket.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UncreatedDeaths
{
    public class Config : IRocketPluginConfiguration
    {
        public string RelativeConfigPathToRocketFolder;
        public string ColorOfTheTextToSendWhenTextIsSentAfterAPlayerDiesOrGetsShotToDeath;
        public bool UseDifferentMessagesForGunIDs;
        public void LoadDefaults()
        {
            RelativeConfigPathToRocketFolder = @"\UncreatedDeaths";
            ColorOfTheTextToSendWhenTextIsSentAfterAPlayerDiesOrGetsShotToDeath = "#ffffff";
            UseDifferentMessagesForGunIDs = true;
        }
    }
}
