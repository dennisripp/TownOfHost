using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using AmongUs.Data;
using UnityEngine;
using static TownOfHost.Translator;


namespace TownOfHost
{
    public static partial class Utils
    {
        static string GameCode = "";


        public enum DiscordCommand
        {
            DELETEOLD,
            POSTCODE,
            POSTRESULT,
            POSTENDNOTIFY,
            LOBBYSTART,
            LOBBYSTARTED
        }


        public static void ExecuteDiscordBot(DiscordCommand command, string param = "null", PlayerControl player = null)
        {
            string current_path = Directory.GetCurrentDirectory() + @"\BepInEx\plugins\discord";
            string discordBot = @$"{current_path}\DiscordAPI.exe";
            string name = "unknown";
            float delay = 0f;

            switch (command)
            {
                case DiscordCommand.LOBBYSTART:
                    delay = 1f;
                    break;
                default: break;
            }


            new LateTask(() =>
            {
                if (player is null) player = PlayerControl.LocalPlayer;

                if (player != null)
                    if (player.cosmetics != null)
                        name = player.cosmetics.name.RemoveHtmlTags().Split(" ")[0];

                string code = Utils.GetGameCode();

                if (code is null || code.Length == 0) code = "XXXXXX";

                ProcessStartInfo psi = new ProcessStartInfo();
                psi.FileName = discordBot;
                psi.UseShellExecute = false;
                psi.CreateNoWindow = true;
                int commandId = (int)command;
                string arguments = "";



                switch (command)
                {

                    case DiscordCommand.DELETEOLD:
                        arguments = @$"{commandId} {name} {code}";
                        break;
                    case DiscordCommand.POSTCODE:
                        arguments = @$"{commandId} {name} {code}";
                        break;
                    case DiscordCommand.POSTRESULT:
                        arguments = $"{commandId} {name} {code} \"{param}\"";
                        break;
                    case DiscordCommand.POSTENDNOTIFY:
                        arguments = @$"{commandId} {name} {code}";
                        break;
                    case DiscordCommand.LOBBYSTART:
                        arguments = $"{commandId} {name} {code} \"{param}\"";
                        break;
                    case DiscordCommand.LOBBYSTARTED:
                        arguments = $"{commandId} {name} {code}";
                        break;
                    default:
                        break;

                }

                psi.Arguments = arguments;
                Logger.Info(arguments, "discord command");


                Process proc = Process.Start(psi);
                // proc.WaitForExit();

            }, delay, $"DiscordBot: {command}");
        }

        public static void SetGameCode(string code)
        {
            GameCode = code;
        }

        public static string GetGameCode()
        {
            return GameCode;
        }

    

        public static string GetLastResult()
        {
            var text = GetString("LastResult") + ":";
            List<byte> cloneRoles = new(Main.PlayerStates.Keys);
            text += $"\n{SetEverythingUpPatch.LastWinsText}\n";
            foreach (var id in Main.winnerList)
            {
                text += $"\n★ " + EndGamePatch.SummaryText[id].RemoveHtmlTags();
                cloneRoles.Remove(id);
            }
            foreach (var id in cloneRoles)
            {
                text += $"\n　 " + EndGamePatch.SummaryText[id].RemoveHtmlTags();
            }
            return text;
        }

    }
}
