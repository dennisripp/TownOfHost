using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Assets.CoreScripts;
using HarmonyLib;
using Hazel;
using UnityEngine;
using static TownOfHost.Translator;

namespace TownOfHost
{
    [HarmonyPatch(typeof(ChatController), nameof(ChatController.SendChat))]
    partial class ChatCommands
    {
        public static string current_path = Directory.GetCurrentDirectory() + @"\BepInEx\plugins";
        public static string whitelistPath = @$"{current_path}\whitelist.txt";
        public static string blacklistPath = @$"{current_path}\blacklist.txt";

        public static string[] linesWhiteList = { };
        public static string[] linesBlackList = { };

        public static List<string> WhiteList = new();
        public static List<string> BlackList = new();

        public static void UpdateWhiteList()
        {
            WhiteList.Clear();
            linesWhiteList = ReadFile(whitelistPath, true);

            foreach (string line in linesWhiteList)
                if (!line.Equals(String.Empty))
                    WhiteList.Add(line.ToLower());
        }

        public static void UpdateBlackList()
        {
            BlackList.Clear();
            linesBlackList = ReadFile(blacklistPath, false);

            foreach (string line in linesBlackList)
                if (!line.Equals(String.Empty))
                    BlackList.Add(line.ToLower());
        }


        public static string[] ReadFile(string path, bool white)
        {
            string[] tmp = { };
            string list = white ? "whitelist" : "blacklist";

            try
            {
                if (File.Exists(path))
                    tmp = File.ReadAllLines(path, Encoding.UTF8);
                else
                    Utils.SendMessage($"{list} missing");

            }
            catch (Exception e)
            {
                Utils.SendMessage($"close {list} in the background");
            }

            return tmp;
        }

        public static bool CompareList(List<string> list, string compStr)
        {
            foreach (string name in list)
            {
                if (name.ToLower().Equals(compStr))
                    return true;
            }

            return false;
        }

        public static bool CompareContainList(List<string> list, string compStr)
        {
            foreach (string name in list)
            {
                if (compStr.ToLower().Contains(name.ToLower()))
                    return true;
            }

            return false;
        }

        public static bool IsWhitelisted(string sender)
        {
            UpdateWhiteList();

            return CompareContainList(WhiteList, sender);
        }

        public static bool IsBlacklisted(string sender)
        {
            UpdateBlackList();

            return CompareList(BlackList, sender);
        }

        public static bool BanPlayerStart(PlayerControl player, string text)
        {
            var name = player.GetRealName().ToLower();

            byte[] bytes = Encoding.Default.GetBytes(name);
            name = Encoding.UTF8.GetString(bytes);

            if (name.Equals(String.Empty)) return false;

            bool isWhite = IsWhitelisted(name);
            if (isWhite) return false;

            text = text.ToLower();
            bytes = Encoding.Default.GetBytes(text);
            text = Encoding.UTF8.GetString(bytes);

            string[] args = text.Split(' ');

            bool isBlackWord = IsBlacklisted(args[0]);
            if (isBlackWord)
            {
                KillAndBan(player, name);
                return true;
            }


            bool isBlackText = IsBlacklisted(text);
            if (isBlackText)
            {
                KillAndBan(player, name);
                return true;
            }

            return false;
        }



        static void GiveNameTags()
        {

            new LateTask(() =>
            {
                foreach (PlayerControl control in PlayerControl.AllPlayerControls)
                {
                    var client = control.GetClient();
                    if (client.Character != null)
                    {
                        string rname = client.Character.GetRealName().RemoveHtmlTags().Split(" ")[0];

                        bool customTag = false;
                        // if (client.FriendCode is "peakenergy#6193")
                        if (client.FriendCode is "rakebronze#7654")
                        {

                            Main.devNames.TryAdd(client.Character.PlayerId, rname);
                            string[] args = { "/col", "white", rname };
                            ChatCommands.StealColorSetColor(args);
                            customTag = true;
                            string fontSize = "1";
                            string dev = $"<size={fontSize}>{Utils.ColorString(Utils.GetRoleColor(CustomRoles.Jackal), "Dev")}</size>";
                            string name = rname + " " + dev;
                            client.Character.RpcSetName($"{Utils.ColorString(Utils.GetRoleColor(CustomRoles.Jackal), name)}");
                        }

                        if (client.FriendCode is "peakenergy#6193")
                        {
                            customTag = true;
                            Main.devNames.TryAdd(client.Character.PlayerId, rname);


                            string[] args = { "/col", "banana", rname };
                            ChatCommands.StealColorSetColor(args);
                            string fontSize = "1";
                            string dev = $"<size={fontSize}>{Utils.ColorString(Utils.GetRoleColor(CustomRoles.Bait), "Manager")}</size>";
                            string name = rname + " " + dev;
                            client.Character.RpcSetName($"{Utils.ColorString(Utils.GetRoleColor(CustomRoles.Bait), name)}");
                        }

                        if (client.FriendCode is "examyogic#1356")
                        {
                            customTag = true;
                            Main.devNames.TryAdd(client.Character.PlayerId, rname);

                            string[] args = { "/col", "cyan", rname };
                            ChatCommands.StealColorSetColor(args);
                            string fontSize = "1";
                            string dev = $"<size={fontSize}>{Utils.ColorString(Utils.GetRoleColor(CustomRoles.Doctor), "angry cyan")}</size>";
                            string name = rname + " " + dev;
                            client.Character.RpcSetName($"{Utils.ColorString(Utils.GetRoleColor(CustomRoles.Doctor), name)}");
                        }
                        if (client.FriendCode is "wisecoin#9682")
                        {
                            customTag = true;
                            Main.devNames.TryAdd(client.Character.PlayerId, rname);
                            string[] args = { "/col", "gray", rname };
                            ChatCommands.StealColorSetColor(args);
                            string fontSize = "1";
                            string dev = $"<size={fontSize}>{Utils.ColorString(Utils.GetRoleColor(CustomRoles.Terrorist), "bite")}</size>";
                            string name = rname + " " + dev;
                            client.Character.RpcSetName($"{Utils.ColorString(Utils.GetRoleColor(CustomRoles.Terrorist), name)}");
                        }
                        if (client.FriendCode is "twirlysong#5280")
                        {
                            customTag = true;
                            Main.devNames.TryAdd(client.Character.PlayerId, rname);
                            string[] args = { "/col", "lime", rname };
                            ChatCommands.StealColorSetColor(args);
                            string fontSize = "1";
                            string dev = $"<size={fontSize}>{Utils.ColorString(Utils.GetRoleColor(CustomRoles.Doctor), "gambler")}</size>";
                            string name = rname + " " + dev;
                            client.Character.RpcSetName($"{Utils.ColorString(Utils.GetRoleColor(CustomRoles.Doctor), name)}");
                        }

                        if (client.FriendCode is "glossybump#6710")
                        {
                            customTag = true;
                            Main.devNames.TryAdd(client.Character.PlayerId, rname);

                            string[] args = { "/col", "pink", rname };
                            ChatCommands.StealColorSetColor(args);
                            string fontSize = "1";
                            string dev = $"<size={fontSize}>{Utils.ColorString(Utils.GetRoleColor(CustomRoles.Jester), "ur mom")}</size>";
                            string name = rname + " " + dev;
                            client.Character.RpcSetName($"{Utils.ColorString(Utils.GetRoleColor(CustomRoles.Jester), name)}");
                        }
                        if (client.FriendCode is "mangoripe#5233")
                        {
                            customTag = true;
                            Main.devNames.TryAdd(client.Character.PlayerId, rname);

                            string[] args = { "/col", "coral", rname };
                            ChatCommands.StealColorSetColor(args);
                            string fontSize = "1";
                            string dev = $"<size={fontSize}>{Utils.ColorString(Utils.GetRoleColor(CustomRoles.Arsonist), "real")}</size>";
                            string name = rname + " " + dev;
                            client.Character.RpcSetName($"{Utils.ColorString(Utils.GetRoleColor(CustomRoles.Arsonist), name)}");
                        }
                        if (client.FriendCode is "halfpager#1131")
                        {
                            customTag = true;
                            Main.devNames.TryAdd(client.Character.PlayerId, rname);

                            string[] args = { "/col", "white", rname };
                            ChatCommands.StealColorSetColor(args);
                            string fontSize = "1";
                            string dev = $"<size={fontSize}>{Utils.ColorString(Utils.GetRoleColor(CustomRoles.Impostor), "hoe")}</size>";
                            string name = rname + " " + dev;
                            client.Character.RpcSetName($"{Utils.ColorString(Utils.GetRoleColor(CustomRoles.Impostor), name)}");
                        }

                    }
                }
                
            }, 1f, "Welcome Message & Name Check");
        }




        public static void RevealDocBait()
        {
            string dock = "";
            string boat = "";

            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                var role = pc.GetCustomRole();

                switch (role)
                {
                    case CustomRoles.Bait:
                        boat = pc.GetRealName();
                        break;

                    case CustomRoles.Doctor:
                        dock = pc.GetRealName();
                        break;
                    default: break;
                }
            }
            Utils.SendMessage($"Doctor: {dock}\nBait: {boat}");

        }

        public static void KillAndBan(PlayerControl player, string name)
        {

            float delay = 1f;

            if(Options.KillStartPlayer.GetBool())
                new LateTask(() =>
                {
                    PlayerControl.LocalPlayer.RpcMurderPlayer(player);                 
                    RPC.PlaySoundRPC(player.PlayerId, Sounds.KillSound);
                    Main.PlayerStates[player.PlayerId].deathReason = PlayerState.DeathReason.Execution;
                    Main.PlayerStates[player.PlayerId].SetDead();
                    Utils.MarkEveryoneDirtySettings();
                    delay = 2f;
                }, 1f, $"word found->ban: {name}");


            new LateTask(() =>
            {
                Utils.SendMessage($"gtfo {name}");
                AmongUsClient.Instance.KickPlayer(player.GetClientId(), true);

                try
                {
                    PlayerControl.AllPlayerControls.Remove(player);

                }
                catch (Exception e)
                {
                    Logger.Info($"removing {player.GetNameWithRole()} failed", "Execution");
                }
            }, delay, $"word found->ban: {name}");
        }

        public static void StealColorSetColor(string[] args)
        {
            if (args is null || args.Length < 1) return;

            if (args.Length > 2)
            {

                for (int i = 0; i < args.Length; i++)
                {
                    args[i] = args[i].ToLower().Normalize();
                }

                string name = "";
                for (int i = 2; i < args.Length; i++)
                {
                    name += args[i];
                }


                PlayerControl pcv = Utils.GetPlayerByColor(args[1]);
                PlayerControl pc = Utils.GetPlayerByName(name);


                if (pc == pcv) return;
                if (pc != null)
                {
                    int colorID = ReturnColorID(args[1]);

                    if (colorID == (int)COLORID.glitched)
                    {
                        return;
                    }

                    pc.RpcChangeColor(colorID);
                    pc.SetColor(colorID);
                }
                if (pcv != null)
                {
                    int freeColor = ReturnFirstFreeColor();
                    pcv.RpcChangeColor(freeColor);
                    pcv.SetColor(freeColor);
                }
            }
        }

        public static void RemoveAllPets()
        {
            foreach (PlayerControl pc in PlayerControl.AllPlayerControls)
            {
                pc.SetPet("none");
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(pc.NetId, (byte)RpcCalls.SetPetStr, SendOption.None, pc.GetClientId());
                writer.Write("none");
                AmongUsClient.Instance.FinishRpcImmediately(writer);
            }
        }


        public static int ReturnFirstFreeColor()
        {
            List<string> colorListAll = new() { "red", "blue", "green" , "pink", "orange", "yellow", "black", "white", "purple", "brown", "cyan", "lime", "maroon",
                                                "rose", "banana", "gray", "tan", "coral" };

            foreach (PlayerControl pc in PlayerControl.AllPlayerControls)
            {
                colorListAll.Remove(pc.cosmetics.colorBlindText.text.ToLower());
            }


            return ReturnColorID(colorListAll.FirstOrDefault());
        }

        static int ReturnColorID(string name)
        {
            switch (name)
            {
                case "red":
                    return (int)COLORID.red;
                case "blue":
                    return (int)COLORID.blue;
                case "green":
                    return (int)COLORID.green;
                case "pink":
                    return (int)COLORID.pink;
                case "orange":
                    return (int)COLORID.orange;
                case "yellow":
                    return (int)COLORID.yellow;
                case "black":
                    return (int)COLORID.black;
                case "white":
                    return (int)COLORID.white;
                case "purple":
                    return (int)COLORID.purple;
                case "brown":
                    return (int)COLORID.brown;
                case "cyan":
                    return (int)COLORID.cyan;
                case "lime":
                    return (int)COLORID.lime;
                case "maroon":
                    return (int)COLORID.maroon;
                case "rose":
                    return (int)COLORID.rose;
                case "banana":
                    return (int)COLORID.banana;
                case "gray":
                    return (int)COLORID.gray;
                case "tan":
                    return (int)COLORID.tan;
                case "coral":
                    return (int)COLORID.coral;
                default:
                    return (int)COLORID.glitched;

            }
        }


        public static void GetRolesInfo(string role)
        {
            var roleList = new Dictionary<CustomRoles, string>
            {
                //GM
                { CustomRoles.GM, "gm" },
                //Impostor役職
                { (CustomRoles)(-1), $"== {GetString("Impostor")} ==" }, //区切り用
                { CustomRoles.BountyHunter, "bo" },
                { CustomRoles.EvilTracker,"et" },
                { CustomRoles.FireWorks, "fw" },
                { CustomRoles.Mare, "ma" },
                { CustomRoles.Mafia, "mf" },
                { CustomRoles.SerialKiller, "sk" },
                //{ CustomRoles.ShapeMaster, "sha" },
                { CustomRoles.TimeThief, "tt"},
                { CustomRoles.Sniper, "snp" },
                { CustomRoles.Puppeteer, "pup" },
                { CustomRoles.Vampire, "va" },
                { CustomRoles.Warlock, "wa" },
                { CustomRoles.Witch, "wi" },
                //Madmate役職
                { (CustomRoles)(-2), $"== {GetString("Madmate")} ==" }, //区切り用
                { CustomRoles.MadGuardian, "mg" },
                { CustomRoles.Madmate, "mm" },
                { CustomRoles.MadSnitch, "msn" },
                { CustomRoles.SKMadmate, "sm" },
                //両陣営役職
                { (CustomRoles)(-3), $"== {GetString("Impostor")} or {GetString("Crewmate")} ==" }, //区切り用
                { CustomRoles.Watcher, "wat" },
                //Crewmate役職
                { (CustomRoles)(-4), $"== {GetString("Crewmate")} ==" }, //区切り用
                { CustomRoles.Bait, "ba" },
                { CustomRoles.Dictator, "dic" },
                { CustomRoles.Doctor, "doc" },
                { CustomRoles.Lighter, "li" },
                { CustomRoles.Mayor, "my" },
                { CustomRoles.SabotageMaster, "sa" },
                { CustomRoles.Seer,"se" },
                { CustomRoles.Sheriff, "sh" },
                { CustomRoles.Snitch, "sn" },
                { CustomRoles.SpeedBooster, "sb" },
                { CustomRoles.Trapper, "tra" },
                //Neutral役職
                { (CustomRoles)(-5), $"== {GetString("Neutral")} ==" }, //区切り用
                { CustomRoles.Arsonist, "ar" },
                { CustomRoles.Egoist, "eg" },
                { CustomRoles.Executioner, "exe" },
                { CustomRoles.Jester, "je" },
                { CustomRoles.Opportunist, "op" },
                { CustomRoles.SchrodingerCat, "sc" },
                { CustomRoles.Terrorist, "te" },
                { CustomRoles.Jackal, "jac" },
                //属性
                { (CustomRoles)(-6), $"== {GetString("Addons")} ==" }, //区切り用
                {CustomRoles.Lovers, "lo" },
                //HAS
                { (CustomRoles)(-7), $"== {GetString("HideAndSeek")} ==" }, //区切り用
                { CustomRoles.HASFox, "hfo" },
                { CustomRoles.HASTroll, "htr" },

            };
            var msg = "";
            var rolemsg = $"{GetString("Command.h_args")}";
            foreach (var r in roleList)
            {
                var roleName = r.Key.ToString();
                var roleShort = r.Value;

                if (String.Compare(role, roleName, true) == 0 || String.Compare(role, roleShort, true) == 0)
                {
                    Utils.SendMessage(GetString(roleName) + GetString($"{roleName}InfoLong"));
                    return;
                }

                var roleText = $"{roleName.ToLower()}({roleShort.ToLower()}), ";
                if ((int)r.Key < 0)
                {
                    msg += rolemsg + "\n" + roleShort + "\n";
                    rolemsg = "";
                }
                else if ((rolemsg.Length + roleText.Length) > 40)
                {
                    msg += rolemsg + "\n";
                    rolemsg = roleText;
                }
                else
                {
                    rolemsg += roleText;
                }
            }
            msg += rolemsg;
            Utils.SendMessage(msg);
        }

        public static string GetRolesInfoSilent(string role)
        {
            var roleList = new Dictionary<CustomRoles, string>
            {
                //GM
                { CustomRoles.GM, "gm" },
                //Impostor役職
                { (CustomRoles)(-1), $"== {GetString("Impostor")} ==" }, //区切り用
                { CustomRoles.BountyHunter, "bo" },
                { CustomRoles.EvilTracker,"et" },
                { CustomRoles.FireWorks, "fw" },
                { CustomRoles.Mare, "ma" },
                { CustomRoles.Mafia, "mf" },
                { CustomRoles.SerialKiller, "sk" },
                //{ CustomRoles.ShapeMaster, "sha" },
                { CustomRoles.TimeThief, "tt"},
                { CustomRoles.Sniper, "snp" },
                { CustomRoles.Puppeteer, "pup" },
                { CustomRoles.Vampire, "va" },
                { CustomRoles.Warlock, "wa" },
                { CustomRoles.Witch, "wi" },
                //Madmate役職
                { (CustomRoles)(-2), $"== {GetString("Madmate")} ==" }, //区切り用
                { CustomRoles.MadGuardian, "mg" },
                { CustomRoles.Madmate, "mm" },
                { CustomRoles.MadSnitch, "msn" },
                { CustomRoles.SKMadmate, "sm" },
                //両陣営役職
                { (CustomRoles)(-3), $"== {GetString("Impostor")} or {GetString("Crewmate")} ==" }, //区切り用
                { CustomRoles.Watcher, "wat" },
                //Crewmate役職
                { (CustomRoles)(-4), $"== {GetString("Crewmate")} ==" }, //区切り用
                { CustomRoles.Bait, "ba" },
                { CustomRoles.Dictator, "dic" },
                { CustomRoles.Doctor, "doc" },
                { CustomRoles.Lighter, "li" },
                { CustomRoles.Mayor, "my" },
                { CustomRoles.SabotageMaster, "sa" },
                { CustomRoles.Seer,"se" },
                { CustomRoles.Sheriff, "sh" },
                { CustomRoles.Snitch, "sn" },
                { CustomRoles.SpeedBooster, "sb" },
                { CustomRoles.Trapper, "tra" },
                //Neutral役職
                { (CustomRoles)(-5), $"== {GetString("Neutral")} ==" }, //区切り用
                { CustomRoles.Arsonist, "ar" },
                { CustomRoles.Egoist, "eg" },
                { CustomRoles.Executioner, "exe" },
                { CustomRoles.Jester, "je" },
                { CustomRoles.Opportunist, "op" },
                { CustomRoles.SchrodingerCat, "sc" },
                { CustomRoles.Terrorist, "te" },
                { CustomRoles.Jackal, "jac" },
                //属性
                { (CustomRoles)(-6), $"== {GetString("Addons")} ==" }, //区切り用
                {CustomRoles.Lovers, "lo" },
                //HAS
                { (CustomRoles)(-7), $"== {GetString("HideAndSeek")} ==" }, //区切り用
                { CustomRoles.HASFox, "hfo" },
                { CustomRoles.HASTroll, "htr" },

            };
            var msg = "";
            var rolemsg = $"{GetString("Command.h_args")}";
            foreach (var r in roleList)
            {
                var roleName = r.Key.ToString();
                var roleShort = r.Value;

                if (String.Compare(role, roleName, true) == 0 || String.Compare(role, roleShort, true) == 0)
                {
                    return (GetString(roleName) + GetString($"{roleName}InfoLong"));
                }

                var roleText = $"{roleName.ToLower()}({roleShort.ToLower()}), ";
                if ((int)r.Key < 0)
                {
                    msg += rolemsg + "\n" + roleShort + "\n";
                    rolemsg = "";
                }
                else if ((rolemsg.Length + roleText.Length) > 40)
                {
                    msg += rolemsg + "\n";
                    rolemsg = roleText;
                }
                else
                {
                    rolemsg += roleText;
                }
            }
            msg += rolemsg;

            return msg;
        }


        public enum COLORID
        {
            red,
            blue,
            green,
            pink,
            orange,
            yellow,
            black,
            white,
            purple,
            brown,
            cyan,
            lime,
            maroon,
            rose,
            banana,
            gray,
            tan,
            coral,
            glitched
        }
    }
}