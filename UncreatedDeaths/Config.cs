using Rocket.API;

namespace UncreatedDeaths
{
    public class Config : IRocketPluginConfiguration
    {
        public string RelativeConfigPathToRocketFolder;
        public string ColorOfText;
        public string ColorOfVicitm;
        public string ColorOfMuder;
        public bool LogDeathMessages;
        public bool DisableVanillaUnturnedDeathLogging;
        public bool EnableUncreatedMainCampingOverride;
        public string MainCampingDeathEnum;
        public string MainCampingKey;
        public void LoadDefaults()
        {
            RelativeConfigPathToRocketFolder = @"\Plugins\UncreatedDeaths";
            ColorOfText = "#ffffff";
            // 橙色 orange
            ColorOfVicitm = "#ffa500";
            // 海军色 navy
            ColorOfMuder = "#000080";
            LogDeathMessages = false;
            DisableVanillaUnturnedDeathLogging = false;
            EnableUncreatedMainCampingOverride = false;
            MainCampingDeathEnum = "ARENA";
            MainCampingKey = "MAINCAMP";
        }
    }
}
