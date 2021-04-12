using Rocket.API;

namespace UncreatedDeaths
{
    public class Config : IRocketPluginConfiguration
    {
        public string RelativeConfigPathToRocketFolder;
        public string ColorOfTheTextToSendWhenTextIsSentAfterAPlayerDiesOrGetsShotToDeath;
        public bool LogDeathMessages;
        public bool DisableVanillaUnturnedDeathLogging;
        public bool EnableUncreatedMainCampingOverride;
        public string MainCampingDeathEnum;
        public string MainCampingKey;
        public void LoadDefaults()
        {
            RelativeConfigPathToRocketFolder = @"\Plugins\UncreatedDeaths";
            ColorOfTheTextToSendWhenTextIsSentAfterAPlayerDiesOrGetsShotToDeath = "#ffffff";
            LogDeathMessages = false;
            DisableVanillaUnturnedDeathLogging = false;
            EnableUncreatedMainCampingOverride = false;
            MainCampingDeathEnum = "ARENA";
            MainCampingKey = "MAINCAMP";
        }
    }
}
