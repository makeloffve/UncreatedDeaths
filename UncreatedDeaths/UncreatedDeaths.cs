using Rocket.API;
using Rocket.Core.Logging;
using Rocket.Core.Plugins;
using Rocket.Unturned.Player;
using SDG.Unturned;
using Steamworks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace UncreatedDeaths
{
    public class Deaths : RocketPlugin<Config>
    {
        public const string translationDescription = "Translations | Key, space, value with unlimited spaces. Formatting: Dead player name, Murderer name when applicable, Limb, Gun name when applicable, distance when applicable. /deathreload to reload";
        public const string limbsDescriptionTransl = "Translations | Key, space, value with unlimited spaces. Must match SDG.Unturned.ELimb enumerator list <LEFT|RIGHT>_<ARM|LEG|BACK|FOOT|FRONT|HAND>, SPINE, SKULL. ex. LEFT_ARM, RIGHT_FOOT";
        public string translationname { get { return System.IO.Directory.GetCurrentDirectory() + Configuration.Instance.RelativeConfigPathToRocketFolder + @"\translations.txt"; } }
        public string limbtranslationname { get { return System.IO.Directory.GetCurrentDirectory() + Configuration.Instance.RelativeConfigPathToRocketFolder + @"\limbs_translations.txt"; } }
        public static Deaths Instance;
        public Dictionary<string, string> translations = new Dictionary<string, string>();
        readonly Dictionary<string, string> DefTranslations = new Dictionary<string, string> {
            { "ACID", "{0} 被酸液僵尸干死了." },
            { "ANIMAL", "{0} 被牲口干死了." },
            { "ARENA", "{0} 被安全区淘汰了." },
            { "BLEEDING", "{1} 把 {0} 弄的流血致死." },
            { "BLEEDING_SUICIDE", "{0} 流血致死." },
            { "BONES", "{0} 摔死了." },
            { "BOULDER", "{0} 被大僵尸砸死了." },
            { "BREATH", "{0} 缺氧致死." },
            { "BURNER", "{0} 被火娃烧死了." },
            { "BURNING", "{0} 想成为第二个奥拉夫, 但是失败了." },
            { "CHARGE", "{0} 被 {1} 的C4炸死." },
            { "CHARGE_SUICIDE", "{0} 被自己的C4炸死." },
            { "FOOD", "{0} 饿死了." },
            { "FREEZING", "{0} 冻死了." },
            { "GRENADE", "{0} 被 {1} 的手雷炸死了." },
            { "GRENADE_SUICIDE", "{0} 被自己的手雷炸死了." },
            { "GUN", "{0} 被 {1} 使用 {3} 击中了 {2} 距离: {4}." },
            { "GUN_UNKNOWN", "{0} 被 {1} 击中了 {2} 距离: {5}." },
            { "GUN_SUICIDE_UNKNOWN", "{0} 击中了自己的 {2} 然后嗝屁了." },
            { "GUN_SUICIDE", "{0} 使用 {3} 击中了自己的 {2} 然后嗝屁了." },
            { "INFECTION", "{0} 感染严重, 转变为僵尸." },
            { "KILL", "{0} 被OP kill, {1}." },
            { "KILL_SUICIDE", "{0} 被OP kill." },
            { "LANDMINE", "{0} 踩地雷炸死." },
            { "MELEE", "{0} 被 {1} 使用 {3} 击中了 {2}." },
            { "MELEE_UNKNOWN", "{0} 被 {1} 击中了 {2}." },
            { "MISSILE", "{0} 被 {1} 的 {3} 炸飞 距离: {4}." },
            { "MISSILE_UNKNOWN", "{0} 被 {1} 的 导弹 炸飞 距离:{4}." },
            { "MISSILE_SUICIDE_UNKNOWN", "{0} 被自己炸飞." },
            { "MISSILE_SUICIDE", "{0} 被自己的 {3} 炸飞." },
            { "PUNCH", "{0} 被 {1} 殴打致死." },
            { "ROADKILL", "{0} 被 {1} 创死了." },
            { "SENTRY", "{0} 倒在了哨兵枪下." },
            { "SHRED", "{0} 倒在了铁丝网上." },
            { "SPARK", "{0} 被大僵尸震死了." },
            { "SPIT", "{0} 被口水杀了." },
            { "SPLASH", "{0} 被 {1} 用 {3} 造成的溅射伤害致死." },
            { "SPLASH_UNKNOWN", "{0} 被 {1} 造成的溅射伤害致死." },
            { "SPLASH_SUICIDE_UNKNOWN", "{0} 被自己造成的溅射致死." },
            { "SPLASH_SUICIDE", "{0} 被自己的 {3} 溅射致死." },
            { "SUICIDE", "{0} 自杀了." },
            { "VEHICLE", "{0} 死于交通事故." },
            { "WATER", "{0} 溺死了." },
            { "ZOMBIE", "{0} 被僵尸殴打致死." },
            { "MAINCAMP", "{0} tried to main-camp {1} from {2} away and died." },
            { "1394", "{0} 被 {1} 使用 {3} 击中了 {2} 距离: {4}." } //HMG
        };
        public Dictionary<ELimb, string> NiceLimbs = new Dictionary<ELimb, string>();
        public readonly Dictionary<ELimb, string> NiceLimbsDef = new Dictionary<ELimb, string> {
            { ELimb.LEFT_ARM, "左臂" },
            { ELimb.LEFT_BACK, "左背" },
            { ELimb.LEFT_FOOT, "左脚" },
            { ELimb.LEFT_FRONT, "左胸" },
            { ELimb.LEFT_HAND, "左手" },
            { ELimb.LEFT_LEG, "左腿" },
            { ELimb.RIGHT_ARM, "右臂" },
            { ELimb.RIGHT_BACK, "右背" },
            { ELimb.RIGHT_FOOT, "右脚" },
            { ELimb.RIGHT_FRONT, "右胸" },
            { ELimb.RIGHT_HAND, "右手" },
            { ELimb.RIGHT_LEG, "右腿" },
            { ELimb.SKULL, "脑袋" },
            { ELimb.SPINE, "脊柱" }
        };
        public EDeathCause OverridedCauseForMainCamping = EDeathCause.ARENA;
        protected override void Load()
        {
            Instance = this;
            if(!System.IO.Directory.Exists(System.IO.Directory.GetCurrentDirectory() + Configuration.Instance.RelativeConfigPathToRocketFolder))
                System.IO.Directory.CreateDirectory(System.IO.Directory.GetCurrentDirectory() + Configuration.Instance.RelativeConfigPathToRocketFolder);
            if(Configuration.Instance.DisableVanillaUnturnedDeathLogging)
                CommandWindow.shouldLogDeaths = false;
            Logger.Log("UncreatedDeaths by BlazingFlame#0001 loaded, attempting to read translations.");
            Rocket.Unturned.Events.UnturnedPlayerEvents.OnPlayerDeath += UnturnedPlayerEvents_OnPlayerDeath;
            CheckForFileAndLoadDefault();
            if (Configuration.Instance.EnableUncreatedMainCampingOverride)
                if (!Enum.TryParse(Configuration.Instance.MainCampingDeathEnum.ToUpper(), out OverridedCauseForMainCamping))
                    Logger.LogError($"Couldn't parse {Configuration.Instance.MainCampingDeathEnum.ToUpper()} to EDeathCause. " +
                        $"Check the keys in the translations file for a list of them all. (Besides MAINCAMP if it's there)");
            base.Load();
        }
        public void CheckForFileAndLoadDefault()
        {
            if (!File.Exists(translationname))
            {
                Logger.Log("Creating translations file and adding default messages.");
                using (FileStream stream = File.Open(translationname, FileMode.OpenOrCreate, FileAccess.Write))
                {
                    MakeTranslationFile(stream);
                    stream.Close();
                    stream.Dispose();
                }
                translations = DefTranslations;
            }
            else
            {
                Logger.Log("Translations found, attempting to load.");
                LoadTranslations();
            }

            if (!File.Exists(limbtranslationname))
            {
                Logger.Log("Creating limb translations file and adding default messages.");
                using (FileStream stream = File.Open(limbtranslationname, FileMode.OpenOrCreate, FileAccess.Write))
                {
                    MakeLimbFile(stream);
                    stream.Close();
                    stream.Dispose();
                }
                NiceLimbs = NiceLimbsDef;
            }
            else
            {
                Logger.Log("Limb Translations found, attempting to load.");
                LoadLimbTranslations();
            }
        }
        protected override void Unload()
        {
            Rocket.Unturned.Events.UnturnedPlayerEvents.OnPlayerDeath -= UnturnedPlayerEvents_OnPlayerDeath;
            if (CommandWindow.shouldLogDeaths == false && Configuration.Instance.DisableVanillaUnturnedDeathLogging)
            {
                CommandWindow.shouldLogDeaths = true;
                Logger.Log("Re-enabled vanilla death logs, unloading.");
            }
            Logger.Log("Unloaded UncreatedDeaths");
            base.Unload();
        }
        private void UnturnedPlayerEvents_OnPlayerDeath(UnturnedPlayer player, EDeathCause cause, ELimb limb, CSteamID murderer)
        {
            UnturnedPlayer murdererPlayer = player;
            string MurdererName = "Unapplicable";
            try
            {
                murdererPlayer = UnturnedPlayer.FromCSteamID(murderer);
                MurdererName = murdererPlayer.DisplayName;
            } catch { }
            string key = cause.ToString();
            string HeldGun = murdererPlayer.GetHeldGunName(MurdererName);
            ushort heldGunID = murdererPlayer.GetHeldGunID(MurdererName);
            float distance = 0;
            if (murdererPlayer != null)
            {
                try
                {
                    distance = UnityEngine.Vector3.Distance(player.Position, murdererPlayer.Position);
                } catch { }
            }
            if (player.CSteamID == murderer && cause != EDeathCause.SUICIDE)
            {
                key += "_SUICIDE";
            }
            if(Configuration.Instance.EnableUncreatedMainCampingOverride && cause == OverridedCauseForMainCamping && translations.ContainsKey(Configuration.Instance.MainCampingKey))
            {
                key = Configuration.Instance.MainCampingKey;
            }
            if ((cause == EDeathCause.GUN || cause == EDeathCause.MELEE || cause == EDeathCause.MISSILE || cause == EDeathCause.SPLASH) && MurdererName != "Unapplicable")
            {
                if (translations.ContainsKey(heldGunID.ToString()))
                {
                    string message = translations[heldGunID.ToString()];
                    try
                    {
                        ChatManager.say(String.Format(message, WrapperVicitm(player.DisplayName), WrapperMurder(MurdererName), limb.GetLimbName(), HeldGun, Math.Round(distance).ToString() + "m"), Configuration.Instance.ColorOfText.Hex(), true);
                        LogDeath(String.Format(message, player.DisplayName, MurdererName, limb.GetLimbName(), HeldGun, Math.Round(distance).ToString() + "m"));
                        
                    }
                    catch
                    {
                        Logger.Log(message + " is too long, sending basic message instead.");
                        if(heldGunID == 0)
                            key += "_UNKNOWN";
                        if (translations.ContainsKey(key))
                        {
                            message = translations[key];
                            try
                            {
                                ChatManager.say(String.Format(message, WrapperVicitm(player.DisplayName), WrapperMurder(MurdererName), limb.GetLimbName(), HeldGun, Math.Round(distance).ToString() + "m"), Configuration.Instance.ColorOfText.Hex(), true);
                                LogDeath(String.Format(message, player.DisplayName, MurdererName, limb.GetLimbName(), HeldGun, Math.Round(distance).ToString() + "m"));
                            }
                            catch
                            {
                                Logger.Log(message + " is too long, sending default message instead.");
                                if (DefTranslations.ContainsKey(key))
                                {
                                    message = DefTranslations[key];
                                    ChatManager.say(String.Format(message, WrapperVicitm(player.DisplayName), WrapperMurder(MurdererName), limb.GetLimbName(), HeldGun, Math.Round(distance).ToString() + "m"), Configuration.Instance.ColorOfText.Hex(), true);
                                    LogDeath(string.Format(String.Format(message, player.DisplayName, MurdererName, limb.GetLimbName(), HeldGun, Math.Round(distance).ToString() + "m")));
                                }
                                else
                                {
                                    ChatManager.say(key + $" ({player.DisplayName}, {murderer.m_SteamID}, {limb}, {HeldGun}, {distance.ToString() + "m"})", Configuration.Instance.ColorOfText.Hex());
                                    LogDeath(key + $" ({player.DisplayName}, {murderer.m_SteamID}, {limb}, {HeldGun}, {distance.ToString() + "m"})");
                                }
                            }
                        }
                    }

                } else
                {
                    if (heldGunID == 0 && key != Configuration.Instance.MainCampingKey)
                        key += "_UNKNOWN";
                    if (translations.ContainsKey(key))
                    {
                        string message = translations[key];
                        try
                        {
                            ChatManager.say(String.Format(message, WrapperVicitm(player.DisplayName), WrapperMurder(MurdererName), limb.GetLimbName(), HeldGun, Math.Round(distance).ToString() + "m"), Configuration.Instance.ColorOfText.Hex(), true);
                            LogDeath(String.Format(message, player.DisplayName, MurdererName, limb.GetLimbName(), HeldGun, Math.Round(distance).ToString() + "m"));
                        } 
                        catch
                        {
                            Logger.Log(message + " is too long, sending default message instead.");
                            if (DefTranslations.ContainsKey(key))
                            {
                                message = DefTranslations[key];
                                ChatManager.say(String.Format(message, WrapperVicitm(player.DisplayName), WrapperMurder(MurdererName), limb.GetLimbName(), HeldGun, Math.Round(distance).ToString() + "m"), Configuration.Instance.ColorOfText.Hex(), true);
                                LogDeath(String.Format(message, player.DisplayName, MurdererName, limb.GetLimbName(), HeldGun, Math.Round(distance).ToString() + "m"));
                            }
                            else
                            {
                                ChatManager.say(key + $" ({player.DisplayName}, {murderer.m_SteamID}, {limb}, {HeldGun}, {distance.ToString() + "m"})", Configuration.Instance.ColorOfText.Hex());
                                LogDeath(key + $" ({player.DisplayName}, {murderer.m_SteamID}, {limb}, {HeldGun}, {distance.ToString() + "m"})");
                            }
                        }
                    }
                }
            } else
            {
                if (translations.ContainsKey(key))
                {
                    if(cause == EDeathCause.BLEEDING)
                    {
                        if (murderer == Provider.server)
                            key += "_SUICIDE";
                        else if (!murderer.m_SteamID.ToString().StartsWith("765"))
                            MurdererName = "a zombie.";
                    }
                    string message = translations[key];
                    try
                    {
                        ChatManager.say(String.Format(message, WrapperVicitm(player.DisplayName), WrapperMurder(MurdererName), limb.GetLimbName(), HeldGun, Math.Round(distance).ToString() + "m"), Configuration.Instance.ColorOfText.Hex(), true);
                        LogDeath(string.Format(message, player.DisplayName, MurdererName, limb.GetLimbName(), HeldGun, Math.Round(distance).ToString() + "m"));
                    } catch
                    {
                        Logger.Log(message + " is too long, sending default message instead.");
                        if (DefTranslations.ContainsKey(key))
                        {
                            message = DefTranslations[key];
                            ChatManager.say(String.Format(message, WrapperVicitm(player.DisplayName), WrapperMurder(MurdererName), limb.GetLimbName(), HeldGun, Math.Round(distance).ToString() + "m"), Configuration.Instance.ColorOfText.Hex(), true);
                            LogDeath(string.Format(message, player.DisplayName, MurdererName, limb.GetLimbName(), HeldGun, Math.Round(distance).ToString() + "m"));
                        }
                        else
                        {
                            ChatManager.say(key + $" ({player.DisplayName}, {murderer.m_SteamID}, {limb}, {HeldGun}, {Math.Round(distance).ToString() + "m"})", Configuration.Instance.ColorOfText.Hex());
                            LogDeath(string.Format(key + $" ({player.DisplayName}, {murderer.m_SteamID}, {limb}, {HeldGun}, {Math.Round(distance).ToString() + "m"})", player.DisplayName, MurdererName, NiceLimbs[limb]));
                        }
                    }
                } else
                {
                    if(DefTranslations.ContainsKey(key))
                    {
                        string message = DefTranslations[key];
                        ChatManager.say(String.Format(message, WrapperVicitm(player.DisplayName), WrapperMurder(MurdererName), NiceLimbs[limb], HeldGun, Math.Round(distance).ToString() + "m"), Configuration.Instance.ColorOfText.Hex(), true);
                        LogDeath(string.Format(message, player.DisplayName, MurdererName, NiceLimbs[limb], HeldGun, Math.Round(distance).ToString() + "m"));
                    }
                    else
                    {
                        ChatManager.say(key + $" ({player.DisplayName}, {murderer.m_SteamID}, {limb}, {HeldGun}, {Math.Round(distance).ToString() + "m"})", Configuration.Instance.ColorOfText.Hex());
                        LogDeath(string.Format(key + $" ({player.DisplayName}, {murderer.m_SteamID}, {limb}, {HeldGun}, {Math.Round(distance).ToString() + "m"})", player.DisplayName, MurdererName, NiceLimbs[limb]));
                    }
                }
            }
        }
        public void LoadTranslations()
        {
            translations = LoadTLFromString(File.ReadAllText(translationname));
        }
        public void LoadLimbTranslations()
        {
            NiceLimbs = LoadLimbTLFromString(File.ReadAllText(limbtranslationname));
        }
        private void LogDeath(string text)
        {
            if (Instance.Configuration.Instance.LogDeathMessages)
            {
                CommandWindow.Log(text);
            }
        }
        private Dictionary<ELimb, string> LoadLimbTLFromString(string s)
        {
            StringReader reader = new StringReader(s);
            Dictionary<ELimb, string> rtn = new Dictionary<ELimb, string>();
            while (true)
            {
                string p = reader.ReadLine();
                if (p == null)
                    break;
                if (p != Deaths.limbsDescriptionTransl)
                {
                    string[] data = p.Split(' ');
                    if (data.Length > 1)
                    {
                        if(Enum.TryParse(data[0], out ELimb result))
                            rtn.Add(result, data.ConcatStringArray(1, data.Length - 1));
                        else
                            Logger.Log("Invalid line, must match SDG.Unturned.ELimb enumerator list (LEFT|RIGHT)_(ARM|LEG|BACK|FOOT|FRONT|HAND), SPINE, SKULL. Line:\n" + p);
                    }
                    else
                        Logger.Log("Error parsing limb\n" + p);
                }
            }
            return rtn;
        }
        private Dictionary<string, string> LoadTLFromString(string s)
        {
            StringReader reader = new StringReader(s);
            Dictionary<string, string> rtn = new Dictionary<string, string>();
            while(true)
            {
                string p = reader.ReadLine();
                if (p == null)
                    break;
                if(p != Deaths.translationDescription)
                {
                    string[] data = p.Split(' ');
                    if (data.Length > 1)
                        rtn.Add(data[0], data.ConcatStringArray(1, data.Length - 1));
                    else
                        Logger.Log("Error parsing translation\n" + p);
                }
            }
            return rtn;
        }
        private void MakeTranslationFile(FileStream stream)
        {
            byte[] bytesTransl = Encoding.UTF8.GetBytes(translationDescription + '\n');
            stream.Write(bytesTransl, 0, bytesTransl.Length);
            foreach (string Key in DefTranslations.Keys)
            {
                byte[] Keybytes = Encoding.UTF8.GetBytes(Key);
                stream.Write(Keybytes, 0, Keybytes.Length);
                byte[] ValueBytes = Encoding.UTF8.GetBytes(' ' + DefTranslations[Key] + '\n');
                stream.Write(ValueBytes, 0, ValueBytes.Length);
            }
        }
        private void MakeLimbFile(FileStream stream)
        {
            byte[] bytesLimbs = Encoding.UTF8.GetBytes(limbsDescriptionTransl + '\n');
            stream.Write(bytesLimbs, 0, bytesLimbs.Length);
            foreach (ELimb Key in NiceLimbsDef.Keys)
            {
                byte[] Keybytes = Encoding.UTF8.GetBytes(Key.ToString());
                stream.Write(Keybytes, 0, Keybytes.Length);
                byte[] ValueBytes = Encoding.UTF8.GetBytes(' ' + NiceLimbsDef[Key] + '\n');
                stream.Write(ValueBytes, 0, ValueBytes.Length);
            }
        }
        public string WrapperVicitm(string name)
        {
            return $"<color={Deaths.Instance.Configuration.Instance.ColorOfVicitm}>{name}</color>";
        }
        public string WrapperMurder(string name)
        {
            return $"<color={Deaths.Instance.Configuration.Instance.ColorOfMuder}>{name}</color>";
        }
    }
    public class DeathReloadCommand : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Both;
        public string Name => "deathreload";
        public string Help => "Reloads translations for plugin without fully reloading it.";
        public string Syntax => "deathreload";
        public List<string> Aliases => new List<string>();
        public List<string> Permissions => new List<string> { "ud.reload" };
        public void Execute(IRocketPlayer caller, string[] command)
        {
            Deaths.Instance.CheckForFileAndLoadDefault();
            if (caller.Id == "Console")
            {
                Logger.Log("Attempted to reload Translations. Check for error messages above. If there are none than the reload was successful.");
            } else
            {
                ChatManager.say(((UnturnedPlayer)caller).CSteamID, "Reloaded Death Message Traslations.", Deaths.Instance.Configuration.Instance.ColorOfText.Hex(), false);
            }
        }
    }
    public static class EXT
    {
        public static string GetLimbName(this ELimb limb)
        {
            if (Deaths.Instance.NiceLimbs.ContainsKey(limb))
            {
                return Deaths.Instance.NiceLimbs[limb];
            }
            else
            {
                return Deaths.Instance.NiceLimbsDef[limb];
            }
        }
        public static string ConcatStringArray(this string[] array, int StartIndex, int EndIndex, char deliminator = ' ')
        {
            string rtn = string.Empty;
            for (int i = StartIndex; i <= EndIndex; i++)
            {
                rtn += array[i];
                if (i != EndIndex)
                    rtn += deliminator;
            }
            return rtn;
        }
        /// <summary>
        /// Convert Hex value or color name to a UnityEngine.Color.
        /// </summary>
        /// <param name="Hex">Color name or "#RRGGBB"/"#RRGGBBAA" hexadecimal color format.<br>Valid color names are: red, cyan, blue, darkblue, lightblue, purple, yellow, lime, fuchsia, white, silver, grey, black, orange, brown, maroon, green, olive, navy, teal, aqua, magenta</br></param>
        /// <returns>Either the correctly parsed color or white if the parse fails.</returns>
        public static UnityEngine.Color Hex(this string Hex)
        {

            if (UnityEngine.ColorUtility.TryParseHtmlString(Hex, out UnityEngine.Color color))
                return color;
            else
                return UnityEngine.Color.white;
        }
        public static string GetHeldGunName(this UnturnedPlayer player, string applicabletest)
        {
            if (player == null || applicabletest == "Unapplicable")
                return "Unknown weapon.";
            else
            {
                ushort HeldItem = player.Player.equipment.itemID;
                if(HeldItem == 1394 && player.IsInVehicle) //HMG
                {
                    VehicleAsset vAsset = null;
                    try
                    {
                        vAsset = (VehicleAsset)Assets.find(EAssetType.VEHICLE, player.CurrentVehicle.name);
                        if (vAsset != null)
                            return vAsset.vehicleName;
                    } catch
                    {
                    }
                }
                ItemAsset asset = null;
                try
                {
                    asset = (ItemAsset)Assets.find(EAssetType.ITEM, HeldItem);
                } catch
                {
                    return HeldItem.ToString();
                }
                if (asset == null)
                    return HeldItem.ToString();
                return asset.itemName;
            }
        }
        public static ushort GetHeldGunID(this UnturnedPlayer player, string applicabletest)
        {
            if (player == null || applicabletest == "Unapplicable")
                return 0;
            else
            {
                return player.Player.equipment.itemID;
            }
        }
    }
}
