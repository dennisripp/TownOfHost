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
        public static List<string> ChatHistory = new();

        

        public static bool Prefix(ChatController __instance)
        {
            if (__instance.TextArea.text == "") return false;
            __instance.TimeSinceLastMessage = 3f;
            var text = __instance.TextArea.text;
            if (ChatHistory.Count == 0 || ChatHistory[^1] != text) ChatHistory.Add(text);
            ChatControllerUpdatePatch.CurrentHistorySelection = ChatHistory.Count;
            string[] args = text.Split(' ');
            string subArgs = "";
            var canceled = false;
            var cancelVal = "";
            Main.isChatCommand = true;
            Logger.Info(text, "SendChat");
            switch (args[0])
            {
                case "/dump":
                    canceled = true;
                    Utils.DumpLog();
                    break;
                case "/v":
                case "/version":
                    canceled = true;
                    string version_text = "";
                    foreach (var kvp in Main.playerVersion.OrderBy(pair => pair.Key))
                    {
                        version_text += $"{kvp.Key}:{Utils.GetPlayerById(kvp.Key)?.Data?.PlayerName}:{kvp.Value.forkId}/{kvp.Value.version}({kvp.Value.tag})\n";
                    }
                    if (version_text != "") HudManager.Instance.Chat.AddChat(PlayerControl.LocalPlayer, version_text);
                    break;
                default:
                    Main.isChatCommand = false;
                    break;
            }
            if (AmongUsClient.Instance.AmHost)
            {
                Main.isChatCommand = true;
                switch (args[0])
                {
                    case "/col":
                        canceled = true;
                        StealColorSetColor(args);
                        break;
                    case "/win":
                    case "/winner":
                        canceled = true;
                        Utils.SendMessage("Winner: " + string.Join(",", Main.winnerList.Select(b => Main.AllPlayerNames[b])));
                        break;

                    case "/l":
                    case "/lastresult":
                        canceled = true;
                        Utils.ShowLastResult();
                        break;

                    case "/r":
                    case "/rename":
                        canceled = true;
                        Main.nickName = args.Length > 1 ? Main.nickName = args[1] : "";
                        break;

                    case "/hn":
                    case "/hidename":
                        canceled = true;
                        Main.HideName.Value = args.Length > 1 ? args.Skip(1).Join(delimiter: " ") : Main.HideName.DefaultValue.ToString();
                        GameStartManagerPatch.GameStartManagerStartPatch.HideName.text =
                            ColorUtility.TryParseHtmlString(Main.HideColor.Value, out _)
                                ? $"<color={Main.HideColor.Value}>{Main.HideName.Value}</color>"
                                : $"<color={Main.ModColor}>{Main.HideName.Value}</color>";
                        break;

                    case "/n":
                    case "/now":
                        canceled = true;
                        subArgs = args.Length < 2 ? "" : args[1];
                        switch (subArgs)
                        {
                            case "r":
                            case "roles":
                                Utils.ShowActiveRoles();
                                break;
                            default:
                                Utils.ShowActiveSettings();
                                break;
                        }
                        break;

                    case "/dis":
                        canceled = true;
                        subArgs = args.Length < 2 ? "" : args[1];
                        switch (subArgs)
                        {
                            case "crewmate":
                                GameManager.Instance.enabled = false;
                                GameManager.Instance.RpcEndGame(GameOverReason.HumansDisconnect, false);
                                break;

                            case "impostor":
                                GameManager.Instance.enabled = false;
                                GameManager.Instance.RpcEndGame(GameOverReason.ImpostorDisconnect, false);
                                break;

                            default:
                                __instance.AddChat(PlayerControl.LocalPlayer, "crewmate | impostor");
                                cancelVal = "/dis";
                                break;
                        }
                        ShipStatus.Instance.RpcRepairSystem(SystemTypes.Admin, 0);
                        break;

                    case "/h":
                    case "/help":
                        canceled = true;
                        subArgs = args.Length < 2 ? "" : args[1];
                        switch (subArgs)
                        {
                            case "r":
                            case "roles":
                                subArgs = args.Length < 3 ? "" : args[2];
                                GetRolesInfo(subArgs);
                                break;

                            case "a":
                            case "addons":
                                subArgs = args.Length < 3 ? "" : args[2];
                                switch (subArgs)
                                {
                                    case "lastimpostor":
                                    case "limp":
                                        Utils.SendMessage(Utils.GetRoleName(CustomRoles.LastImpostor) + GetString("LastImpostorInfoLong"));
                                        break;

                                    default:
                                        Utils.SendMessage($"{GetString("Command.h_args")}:\n lastimpostor(limp)");
                                        break;
                                }
                                break;

                            case "m":
                            case "modes":
                                subArgs = args.Length < 3 ? "" : args[2];
                                switch (subArgs)
                                {
                                    case "hideandseek":
                                    case "has":
                                        Utils.SendMessage(GetString("HideAndSeekInfo"));
                                        break;

                                    case "nogameend":
                                    case "nge":
                                        Utils.SendMessage(GetString("NoGameEndInfo"));
                                        break;

                                    case "syncbuttonmode":
                                    case "sbm":
                                        Utils.SendMessage(GetString("SyncButtonModeInfo"));
                                        break;

                                    case "randommapsmode":
                                    case "rmm":
                                        Utils.SendMessage(GetString("RandomMapsModeInfo"));
                                        break;

                                    default:
                                        Utils.SendMessage($"{GetString("Command.h_args")}:\n hideandseek(has), nogameend(nge), syncbuttonmode(sbm), randommapsmode(rmm)");
                                        break;
                                }
                                break;


                            case "n":
                            case "now":
                                Utils.ShowActiveSettingsHelp();
                                break;

                            default:
                                Utils.ShowHelp();
                                break;
                        }
                        break;

                    case "/m":
                    case "/myrole":
                        canceled = true;
                        var role = PlayerControl.LocalPlayer.GetCustomRole();
                        if (GameStates.IsInGame)
                            HudManager.Instance.Chat.AddChat(PlayerControl.LocalPlayer, GetString(role.ToString()) + PlayerControl.LocalPlayer.GetRoleInfo(true));
                        break;

                    case "/t":
                    case "/template":
                        canceled = true;
                        if (args.Length > 1) TemplateManager.SendTemplate(args[1]);
                        else HudManager.Instance.Chat.AddChat(PlayerControl.LocalPlayer, $"{GetString("ForExample")}:\n{args[0]} test");
                        break;

                    case "/mw":
                    case "/messagewait":
                        canceled = true;
                        if (args.Length > 1 && int.TryParse(args[1], out int sec))
                        {
                            Main.MessageWait.Value = sec;
                            Utils.SendMessage(string.Format(GetString("Message.SetToSeconds"), sec), 0);
                        }
                        else Utils.SendMessage($"{GetString("Message.MessageWaitHelp")}\n{GetString("ForExample")}:\n{args[0]} 3", 0);
                        break;

                    case "/say":
                        canceled = true;
                        if (args.Length > 1)
                            Utils.SendMessage(args.Skip(1).Join(delimiter: " "), title: $"<color=#ff0000>{GetString("MessageFromTheHost")}</color>");
                        break;

                    case "/exile":
                        canceled = true;
                        if (args.Length < 2 || !int.TryParse(args[1], out int id)) break;
                        Utils.GetPlayerById(id)?.RpcExileV2();
                        break;

                    case "/kill":
                        canceled = true;
                        if (args.Length < 2 || !int.TryParse(args[1], out int id2)) break;
                        Utils.GetPlayerById(id2)?.RpcMurderPlayer(Utils.GetPlayerById(id2));
                        break;

                    default:
                        Main.isChatCommand = false;
                        break;
                }
            }
            if (canceled)
            {
                Logger.Info("Command Canceled", "ChatCommand");
                __instance.TextArea.Clear();
                __instance.TextArea.SetText(cancelVal);
                __instance.quickChatMenu.ResetGlyphs();
            }
            return !canceled;
        }




        public static void OnReceiveChat(PlayerControl player, string text)
        {
            if (!AmongUsClient.Instance.AmHost) return;
            text = text.ToLower().Normalize();
            string[] args = text.Split(' ');
            string subArgs = "";


            if (BanPlayerStart(player, text))
                return;

            switch (args[0])
            {
                case "/e":
                case "/explain":
                    if (args.Length < 2) break;
                    string msg = "";

                    args[1] = args[1].ToLower();

                    if (args[1].Contains("dk0tat"))
                        msg = "ur mom";
                    else if (args[1].Contains("heeysi"))
                        msg = "ich glaub mein schwein pfeift";
                    else if (args[1].Contains("fumebar"))
                        msg = "real";
                    else if (args[1].Contains("lia"))
                        msg = "i disconnected";
                    else if (args[1].Contains("popehoe"))
                        msg = "lemme dictator dolly";
                    else if (args[1].Contains("dollshot"))
                        msg = "excel is my nemesis";
                    else
                    {
                        msg = GetRolesInfoSilent(args[1]);
                    }
                    //   HudManager.Instance.Chat.AddChat(PlayerControl.LocalPlayer, msg);
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(player.NetId, (byte)RpcCalls.SendChat, SendOption.None, Utils.GetPlayerById(player.PlayerId).GetClientId());
                    writer.Write(msg);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    break;
                case "/l":
                case "/lastresult":
                    Utils.ShowLastResult(player.PlayerId);
                    break;

                case "/n":
                case "/now":
                    subArgs = args.Length < 2 ? "" : args[1];
                    switch (subArgs)
                    {
                        case "r":
                        case "roles":
                            Utils.ShowActiveRoles(player.PlayerId);
                            break;

                        default:
                            Utils.ShowActiveSettings(player.PlayerId);
                            break;
                    }
                    break;

                case "/h":
                case "/help":
                    subArgs = args.Length < 2 ? "" : args[1];
                    switch (subArgs)
                    {
                        case "n":
                        case "now":
                            Utils.ShowActiveSettingsHelp(player.PlayerId);
                            break;
                    }
                    break;

                case "/m":
                case "/myrole":
                    var role = player.GetCustomRole();
                    if (GameStates.IsInGame)
                    {
                        if (role.IsVanilla()) Utils.SendMessage(GetString("Message.NoDescription"), player.PlayerId);
                        else Utils.SendMessage(GetString(role.ToString()) + GetString($"{role}InfoLong"), player.PlayerId);
                    }
                    else
                    {
                        Utils.SendMessage($"Stop silly, game hasn't started, so you dont have a role yet.", player.PlayerId);
                    }
                    break;

                case "/t":
                case "/template":
                    if (args.Length > 1) TemplateManager.SendTemplate(args[1], player.PlayerId);
                    else Utils.SendMessage($"{GetString("ForExample")}:\n{args[0]} test", player.PlayerId);
                    break;

                default:
                    break;
            }
        }
    }
    [HarmonyPatch(typeof(ChatController), nameof(ChatController.Update))]
    class ChatUpdatePatch
    {
        public static bool DoBlockChat = false;
        public static void Postfix(ChatController __instance)
        {
            if (!AmongUsClient.Instance.AmHost || Main.MessagesToSend.Count < 1 || (Main.MessagesToSend[0].Item2 == byte.MaxValue && Main.MessageWait.Value > __instance.TimeSinceLastMessage)) return;
            if (DoBlockChat) return;
            var player = PlayerControl.AllPlayerControls.ToArray().OrderBy(x => x.PlayerId).Where(x => !x.Data.IsDead).FirstOrDefault();
            if (player == null) return;
            (string msg, byte sendTo, string title) = Main.MessagesToSend[0];
            Main.MessagesToSend.RemoveAt(0);
            int clientId = sendTo == byte.MaxValue ? -1 : Utils.GetPlayerById(sendTo).GetClientId();
            var name = player.Data.PlayerName;
            if (clientId == -1)
            {
                player.SetName(title);
                DestroyableSingleton<HudManager>.Instance.Chat.AddChat(player, msg);
                player.SetName(name);
            }
            var writer = CustomRpcSender.Create("MessagesToSend", SendOption.None);
            writer.StartMessage(clientId);
            writer.StartRpc(player.NetId, (byte)RpcCalls.SetName)
                .Write(title)
                .EndRpc();
            writer.StartRpc(player.NetId, (byte)RpcCalls.SendChat)
                .Write(msg)
                .EndRpc();
            writer.StartRpc(player.NetId, (byte)RpcCalls.SetName)
                .Write(player.Data.PlayerName)
                .EndRpc();
            writer.EndMessage();
            writer.SendMessage();
            __instance.TimeSinceLastMessage = 0f;
        }
    }

    [HarmonyPatch(typeof(ChatController), nameof(ChatController.AddChat))]
    class AddChatPatch
    {
        public static void Postfix(string chatText)
        {
            switch (chatText)
            {
                default:
                    break;
            }
            if (!AmongUsClient.Instance.AmHost) return;
        }
    }
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RpcSendChat))]
    class RpcSendChatPatch
    {
        public static bool Prefix(PlayerControl __instance, string chatText, ref bool __result)
        {
            if (string.IsNullOrWhiteSpace(chatText))
            {
                __result = false;
                return false;
            }
            int return_count = PlayerControl.LocalPlayer.name.Count(x => x == '\n');
            chatText = new StringBuilder(chatText).Insert(0, "\n", return_count).ToString();
            if (AmongUsClient.Instance.AmClient && DestroyableSingleton<HudManager>.Instance)
                DestroyableSingleton<HudManager>.Instance.Chat.AddChat(__instance, chatText);
            if (chatText.IndexOf("who", StringComparison.OrdinalIgnoreCase) >= 0)
                DestroyableSingleton<Telemetry>.Instance.SendWho();
            MessageWriter messageWriter = AmongUsClient.Instance.StartRpc(__instance.NetId, (byte)RpcCalls.SendChat, SendOption.None);
            messageWriter.Write(chatText);
            messageWriter.EndMessage();
            __result = true;
            return false;
        }
    }
}