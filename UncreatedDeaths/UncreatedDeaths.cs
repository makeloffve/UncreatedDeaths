using Rocket.API;
using Rocket.Core;
using Rocket.Core.Logging;
using Rocket.Core.Plugins;
using Rocket.Unturned;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using SDG.Unturned;
using Steamworks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UncreatedDeaths
{
    public class Deaths : RocketPlugin<Config>
    {
        public const string translationDescription = "Translations | Key, space, value with unlimited spaces. Formatting: Dead player name, Murderer name when applicable, Limb, Gun name when applicable, distance when applicable.";
        public string translationname { get { return System.IO.Directory.GetCurrentDirectory() + Configuration.Instance.RelativeConfigPathToRocketFolder + @"\translations.txt"; } }
        public static Deaths Instance;
        public Dictionary<string, string> translations = new Dictionary<string, string>();
        readonly Dictionary<string, string> DefTranslations = new Dictionary<string, string> {
            { "ACID", "{0} was burned by acid." },
            { "ANIMAL", "{0} was attacked by an animal." },
            { "ARENA", "{0} stepped outside the arena boundary." },
            { "BLEEDING", "{0} bled out from {1}." },
            { "BLEEDING_SUICIDE", "{0} bled out." },
            { "BONES", "{0} fell to their death." },
            { "BOULDER", "{0} was crushed by a mega zombie." },
            { "BREATH", "{0} asphyxiated." },
            { "BURNER", "{0} was burned by a mega zombie." },
            { "BURNING", "{0} burned to death." },
            { "CHARGE", "{0} was blown up by {1}'s demolition charge." },
            { "CHARGE_SUICIDE", "{0} was blown up by their own demolition charge." },
            { "FOOD", "{0} starved to death." },
            { "FREEZING", "{0} froze to death." },
            { "GRENADE", "{0} was blown up by {1}'s grenade." },
            { "GRENADE_SUICIDE", "{0} blew themselves up with a grenade." },
            { "GUN", "{0} was shot by {1} in the {2} with a {3} from {4} away." },
            { "GUN_UNKNOWN", "{0} was shot by {1} in the {2} from {5} away." },
            { "GUN_SUICIDE_UNKNOWN", "{0} shot themselves in the {2}." },
            { "GUN_SUICIDE", "{0} shot themselves with a {3} in the {2}." },
            { "INFECTION", "{0} got infected." },
            { "KILL", "{0} was killed by an admin, {1}." },
            { "KILL_SUICIDE", "{0} killed themselves as an admin." },
            { "LANDMINE", "{0} got blown up by a landmine." },
            { "MELEE", "{0} was meleed by {1} with a {3} in the {2}." },
            { "MELEE_UNKNOWN", "{0} was meleed by {1} in the {2}." },
            { "MISSILE", "{0} was blown up by {1}'s {3} from {4} away." },
            { "MISSILE_UNKNOWN", "{0} was blown up by {1}'s missile from {4} away." },
            { "MISSILE_SUICIDE_UNKNOWN", "{0} blew themselves up." },
            { "MISSILE_SUICIDE", "{0} blew themselves up with a {3}." },
            { "PUNCH", "{0} was punched by {1}." },
            { "ROADKILL", "{0} was ran over by {1}." },
            { "SENTRY", "{0} was killed by a sentry." },
            { "SHRED", "{0} was shredded by barbed wire." },
            { "SPARK", "{0} was shocked by a mega zombie." },
            { "SPIT", "{0} was killed by a spitter zombie." },
            { "SPLASH", "{0} died to splash damage by {1} with a {3}." },
            { "SPLASH_UNKNOWN", "{0} died to splash damage by {1}." },
            { "SPLASH_SUICIDE_UNKNOWN", "{0} killed theirself with splash damage." },
            { "SPLASH_SUICIDE", "{0} killed theirself with splash damage from a {3}." },
            { "SUICIDE", "{0} committed suicide." },
            { "VEHICLE", "{0} was killed by a vehicle." },
            { "WATER", "{0} dehydrated." },
            { "ZOMBIE", "{0} was killed by a zombie." }
        };
        readonly Dictionary<ELimb, string> NiceLimbs = new Dictionary<ELimb, string> {
            { ELimb.LEFT_ARM, "Left Arm" },
            { ELimb.LEFT_BACK, "Left Back" },
            { ELimb.LEFT_FOOT, "Left Foot" },
            { ELimb.LEFT_FRONT, "Left Front" },
            { ELimb.LEFT_HAND, "Left Hand" },
            { ELimb.LEFT_LEG, "Left Leg" },
            { ELimb.RIGHT_ARM, "Right Arm" },
            { ELimb.RIGHT_BACK, "Right Back" },
            { ELimb.RIGHT_FOOT, "Right Foot" },
            { ELimb.RIGHT_FRONT, "Right Front" },
            { ELimb.RIGHT_HAND, "Right Hand" },
            { ELimb.RIGHT_LEG, "Right Leg" },
            { ELimb.SKULL, "Head" },
            { ELimb.SPINE, "Spine" }
        };
        protected override void Load()
        {
            Instance = this;
            if(!System.IO.Directory.Exists(System.IO.Directory.GetCurrentDirectory() + Configuration.Instance.RelativeConfigPathToRocketFolder))
            {
                System.IO.Directory.CreateDirectory(System.IO.Directory.GetCurrentDirectory() + Configuration.Instance.RelativeConfigPathToRocketFolder);
            }
            Logger.Log("UncreatedDeaths by BlazingFlame#0001 loaded, attempting to read translations.");
            R.Plugins.OnPluginsLoaded += Plugins_OnPluginsLoaded;
            CheckForFileAndLoadDefault();
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
        }
        protected override void Unload()
        {
            R.Plugins.OnPluginsLoaded -= Plugins_OnPluginsLoaded;
            try
            {
                Rocket.Unturned.Events.UnturnedPlayerEvents.OnPlayerDeath -= UnturnedPlayerEvents_OnPlayerDeath;
            }
            finally
            {
                Logger.Log("Unloaded Death Messages.");
            }
            base.Unload();
        }
        private void Plugins_OnPluginsLoaded()
        {
            Rocket.Unturned.Events.UnturnedPlayerEvents.OnPlayerDeath += UnturnedPlayerEvents_OnPlayerDeath;
        }
        private void UnturnedPlayerEvents_OnPlayerDeath(UnturnedPlayer player, EDeathCause cause, ELimb limb, CSteamID murderer)
        {
            UnturnedPlayer murdererPlayer = player;
            string MurdererName = "Unapplicable";
            try
            {
                murdererPlayer = UnturnedPlayer.FromCSteamID(murderer);
                MurdererName = murdererPlayer.DisplayName;
            }
            catch (Exception ex) { Logger.Log(ex); Logger.Log(murderer.m_SteamID.ToString()); Logger.Log(Provider.server.m_SteamID.ToString()); }
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
            if (player.CSteamID == murderer)
            {
                key += "_SUICIDE";
            }
            if ((cause == EDeathCause.GUN || cause == EDeathCause.MELEE || cause == EDeathCause.MISSILE || cause == EDeathCause.SPLASH) && MurdererName != "Unapplicable")
            {
                if (translations.ContainsKey(heldGunID.ToString()))
                {
                    string message = translations[heldGunID.ToString()];
                    try
                    {
                        ChatManager.say(String.Format(message, player.DisplayName, MurdererName, NiceLimbs[limb], HeldGun, Math.Round(distance).ToString() + "m"), Configuration.Instance.ColorOfTheTextToSendWhenTextIsSentAfterAPlayerDiesOrGetsShotToDeath.Hex());
                        Logger.Log(String.Format(message, player.DisplayName, MurdererName, NiceLimbs[limb], HeldGun, Math.Round(distance).ToString() + "m"));
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
                                ChatManager.say(String.Format(message, player.DisplayName, MurdererName, NiceLimbs[limb], HeldGun, Math.Round(distance).ToString() + "m"), Configuration.Instance.ColorOfTheTextToSendWhenTextIsSentAfterAPlayerDiesOrGetsShotToDeath.Hex());
                                Logger.Log(String.Format(message, player.DisplayName, MurdererName, NiceLimbs[limb], HeldGun, Math.Round(distance).ToString() + "m"));
                            }
                            catch
                            {
                                Logger.Log(message + " is too long, sending default message instead.");
                                if (DefTranslations.ContainsKey(key))
                                {
                                    message = DefTranslations[key];
                                    ChatManager.say(String.Format(message, player.DisplayName, MurdererName, NiceLimbs[limb], HeldGun, Math.Round(distance).ToString() + "m"), Configuration.Instance.ColorOfTheTextToSendWhenTextIsSentAfterAPlayerDiesOrGetsShotToDeath.Hex());
                                    Logger.Log(string.Format(String.Format(message, player.DisplayName, MurdererName, NiceLimbs[limb], HeldGun, Math.Round(distance).ToString() + "m")));
                                }
                                else
                                {
                                    ChatManager.say(key + $" ({player.DisplayName}, {murderer.m_SteamID}, {limb}, {HeldGun}, {distance.ToString() + "m"})", Configuration.Instance.ColorOfTheTextToSendWhenTextIsSentAfterAPlayerDiesOrGetsShotToDeath.Hex());
                                    Logger.Log(String.Format(message, player.DisplayName, MurdererName, NiceLimbs[limb], HeldGun, Math.Round(distance).ToString() + "m"));
                                }
                            }
                        }
                    }

                } else
                {
                    if (heldGunID == 0)
                        key += "_UNKNOWN";
                    if (translations.ContainsKey(key))
                    {
                        string message = translations[key];
                        try
                        {
                            ChatManager.say(String.Format(message, player.DisplayName, MurdererName, NiceLimbs[limb], HeldGun, Math.Round(distance).ToString() + "m"), Configuration.Instance.ColorOfTheTextToSendWhenTextIsSentAfterAPlayerDiesOrGetsShotToDeath.Hex());
                            Logger.Log(String.Format(message, player.DisplayName, MurdererName, NiceLimbs[limb], HeldGun, Math.Round(distance).ToString() + "m"));
                        } 
                        catch
                        {
                            Logger.Log(message + " is too long, sending default message instead.");
                            if (DefTranslations.ContainsKey(key))
                            {
                                message = DefTranslations[key];
                                ChatManager.say(String.Format(message, player.DisplayName, MurdererName, NiceLimbs[limb], HeldGun, Math.Round(distance).ToString() + "m"), Configuration.Instance.ColorOfTheTextToSendWhenTextIsSentAfterAPlayerDiesOrGetsShotToDeath.Hex());
                                Logger.Log(String.Format(message, player.DisplayName, MurdererName, NiceLimbs[limb], HeldGun, Math.Round(distance).ToString() + "m"));
                            }
                            else
                            {
                                ChatManager.say(key + $" ({player.DisplayName}, {murderer.m_SteamID}, {limb}, {HeldGun}, {distance.ToString() + "m"})", Configuration.Instance.ColorOfTheTextToSendWhenTextIsSentAfterAPlayerDiesOrGetsShotToDeath.Hex());
                                Logger.Log(String.Format(message, player.DisplayName, MurdererName, NiceLimbs[limb], HeldGun, Math.Round(distance).ToString() + "m"));
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
                        ChatManager.say(string.Format(message, player.DisplayName, MurdererName, NiceLimbs[limb], HeldGun, Math.Round(distance).ToString() + "m"), Configuration.Instance.ColorOfTheTextToSendWhenTextIsSentAfterAPlayerDiesOrGetsShotToDeath.Hex());
                        Logger.Log(string.Format(message, player.DisplayName, MurdererName, NiceLimbs[limb], HeldGun, Math.Round(distance).ToString() + "m"));
                    } catch
                    {
                        Logger.Log(message + " is too long, sending default message instead.");
                        if (DefTranslations.ContainsKey(key))
                        {
                            message = DefTranslations[key];
                            ChatManager.say(string.Format(message, player.DisplayName, MurdererName, NiceLimbs[limb], HeldGun, Math.Round(distance).ToString() + "m"), Configuration.Instance.ColorOfTheTextToSendWhenTextIsSentAfterAPlayerDiesOrGetsShotToDeath.Hex());
                            Logger.Log(string.Format(message, player.DisplayName, MurdererName, NiceLimbs[limb], HeldGun, Math.Round(distance).ToString() + "m"));
                        }
                        else
                        {
                            ChatManager.say(key + $" ({player.DisplayName}, {murderer.m_SteamID}, {limb}, {HeldGun}, {Math.Round(distance).ToString() + "m"})", Configuration.Instance.ColorOfTheTextToSendWhenTextIsSentAfterAPlayerDiesOrGetsShotToDeath.Hex());
                            Logger.Log(string.Format(key + $" ({player.DisplayName}, {murderer.m_SteamID}, {limb}, {HeldGun}, {Math.Round(distance).ToString() + "m"})", player.DisplayName, MurdererName, NiceLimbs[limb]));
                        }
                    }
                } else
                {
                    if(DefTranslations.ContainsKey(key))
                    {
                        string message = DefTranslations[key];
                        ChatManager.say(string.Format(message, player.DisplayName, MurdererName, NiceLimbs[limb], HeldGun, Math.Round(distance).ToString() + "m"), Configuration.Instance.ColorOfTheTextToSendWhenTextIsSentAfterAPlayerDiesOrGetsShotToDeath.Hex());
                        Logger.Log(string.Format(message, player.DisplayName, MurdererName, NiceLimbs[limb], HeldGun, Math.Round(distance).ToString() + "m"));
                    }
                    else
                    {
                        ChatManager.say(key + $" ({player.DisplayName}, {murderer.m_SteamID}, {limb}, {HeldGun}, {Math.Round(distance).ToString() + "m"})", Configuration.Instance.ColorOfTheTextToSendWhenTextIsSentAfterAPlayerDiesOrGetsShotToDeath.Hex());
                        Logger.Log(string.Format(key + $" ({player.DisplayName}, {murderer.m_SteamID}, {limb}, {HeldGun}, {Math.Round(distance).ToString() + "m"})", player.DisplayName, MurdererName, NiceLimbs[limb]));
                    }
                }
            }
        }
        public void LoadTranslations()
        {
            translations = LoadTLFromString(File.ReadAllText(translationname));
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
                        Logger.Log("Error parsing\n" + p);
                }
            }
            return rtn;
        }
        private void MakeTranslationFile(FileStream stream)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(translationDescription + '\n');
            stream.Write(bytes, 0, bytes.Length);
            foreach (string Key in DefTranslations.Keys)
            {
                byte[] Keybytes = Encoding.UTF8.GetBytes(Key);
                stream.Write(Keybytes, 0, Keybytes.Length);
                byte[] ValueBytes = Encoding.UTF8.GetBytes(' ' + DefTranslations[Key] + '\n');
                stream.Write(ValueBytes, 0, ValueBytes.Length);
            }

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
                ChatManager.say(((UnturnedPlayer)caller).CSteamID, "Reloaded Death Message Traslations.", Deaths.Instance.Configuration.Instance.ColorOfTheTextToSendWhenTextIsSentAfterAPlayerDiesOrGetsShotToDeath.Hex(), false);
            }
        }
    }
    public static class EXT
    {
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
