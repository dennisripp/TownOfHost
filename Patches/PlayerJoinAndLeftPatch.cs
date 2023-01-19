using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AmongUs.Data;
using AmongUs.GameOptions;
using HarmonyLib;
using Hazel;
using InnerNet;
using TownOfHost.Modules;
using static TownOfHost.Translator;

namespace TownOfHost
{
    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnGameJoined))]
    class OnGameJoinedPatch
    {
        public static void Postfix(AmongUsClient __instance)
        {
            while (!Options.IsLoaded) System.Threading.Tasks.Task.Delay(1);
            Logger.Info($"{__instance.GameId}に参加", "OnGameJoined");
            Main.playerVersion = new Dictionary<byte, PlayerVersion>();
            RPC.RpcVersionCheck();

            SoundManager.Instance.ChangeAmbienceVolume(DataManager.Settings.Audio.AmbienceVolume);
            Main.devNames = new Dictionary<byte, string>();

            ChatUpdatePatch.DoBlockChat = false;
            GameStates.InGame = false;
            NameColorManager.Begin();
            ErrorText.Instance.Clear();
            if (AmongUsClient.Instance.AmHost) //以下、ホストのみ実行
            {
                if (Options.AutoShareLobbyCode.GetBool())
                    Utils.ExecuteDiscordBot(Utils.DiscordCommand.LOBBYSTART, Utils.GetLastResult());

                if (Main.NormalOptions.KillCooldown == 0f)
                    Main.NormalOptions.KillCooldown = Main.LastKillCooldown.Value;

                AURoleOptions.SetOpt(Main.NormalOptions.Cast<IGameOptions>());
                if (AURoleOptions.ShapeshifterCooldown == 0f)
                    AURoleOptions.ShapeshifterCooldown = Main.LastShapeshifterCooldown.Value;
                new LateTask(() =>
                {
                    string rname = PlayerControl.LocalPlayer.GetClient().Character.Data.PlayerName;
                    Main.devNames.Add(PlayerControl.LocalPlayer.GetClient().Character.PlayerId, rname);

                    string fontSize = "1";
                    //  string neww = PlayerControl.LocalPlayer.FriendCode == "rakebronze#7654" ? $"<size={fontSize}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.Vulture), " + ServerBooster")}</size>" : "";
                    string dev = $"<size={fontSize}>{Utils.ColorString(Utils.GetRoleColor(CustomRoles.Opportunist), "Host")}</size>";
                    string name = rname + " " + dev;
                    PlayerControl.LocalPlayer.GetClient().Character.RpcSetName($"{Utils.ColorString(Utils.GetRoleColor(CustomRoles.Crewmate), name)}");

                }, 3f, "Host Join");

                if (Main.NormalOptions.KillCooldown == 0.1f)
                    GameOptionsManager.Instance.normalGameHostOptions.KillCooldown = Main.LastKillCooldown.Value;
            }
        }
    }
    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnPlayerJoined))]
    class OnPlayerJoinedPatch
    {

        public static string current_path = Directory.GetCurrentDirectory() + @"\BepInEx\plugins";
        public static string welcomePath = @$"{current_path}\welcome.txt";
        public static string defaultWelcomeString = @"This is a modded lobby, called Town of Host.";

        public static string ReadFile(string path)
        {
            string[] tmp = { };

            try
            {
                if (File.Exists(path))
                    tmp = File.ReadAllLines(path, Encoding.UTF8);
                else
                    return defaultWelcomeString;
            }
            catch (Exception e)
            {
                Utils.SendMessage($"text file corrupted");
            }

            StringBuilder stringBuilder = new StringBuilder();

            foreach (string str in tmp) stringBuilder.Append(str);

            return stringBuilder.ToString();
        }


        public static void Postfix(AmongUsClient __instance, [HarmonyArgument(0)] ClientData client)
        {
            Logger.Info($"{client.PlayerName}(ClientID:{client.Id})が参加", "Session");
            if (DestroyableSingleton<FriendsListManager>.Instance.IsPlayerBlockedUsername(client.FriendCode) && AmongUsClient.Instance.AmHost)
            {
                AmongUsClient.Instance.KickPlayer(client.Id, true);
                Logger.Info($"ブロック済みのプレイヤー{client?.PlayerName}({client.FriendCode})をBANしました。", "BAN");
            }
            BanManager.CheckBanPlayer(client);
            BanManager.CheckDenyNamePlayer(client);
            Main.playerVersion = new Dictionary<byte, PlayerVersion>();
            RPC.RpcVersionCheck();

            if (AmongUsClient.Instance.AmHost)
            {
                new LateTask(() =>
                {
                    if (client.Character != null)
                    {
                        string msg = $"Welcome {client.PlayerName} to Town of Host modded Lobby!";
                        string msg3 = ReadFile(welcomePath);
                        //  string msg2 = $"Type /myrole to get your role explained mid game. Or type /e jackal to get a role explained" + Utils.GetGameCode();
                        Logger.Info($"welcomed {client.PlayerName}", "Denni");
                        Utils.SendMessage($"{msg}\n{msg3}", client.Character.PlayerId);
                    }
                    else
                    {
                        Logger.Info($"client character equals null", "Denni");

                    }
                }, 3f, "welcome message");
            }


            new LateTask(() =>
            {
                ChatCommands.NameTagFilter(client);

            }, 3f, "Welcome Message & Name Check");
        }
    }


    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnPlayerLeft))]
    class OnPlayerLeftPatch
    {
        public static void Postfix(AmongUsClient __instance, [HarmonyArgument(0)] ClientData data, [HarmonyArgument(1)] DisconnectReasons reason)
        {

            if (GameStates.IsInGame)
            {
                if (data.Character.Is(CustomRoles.TimeThief))
                    data.Character.ResetVotingTime();
                if (data.Character.Is(CustomRoles.Lovers) && !data.Character.Data.IsDead)
                    foreach (var lovers in Main.LoversPlayers.ToArray())
                    {
                        Main.isLoversDead = true;
                        Main.LoversPlayers.Remove(lovers);
                        Main.PlayerStates[lovers.PlayerId].RemoveSubRole(CustomRoles.Lovers);
                    }
                if (data.Character.Is(CustomRoles.Executioner) && Executioner.Target.ContainsKey(data.Character.PlayerId))
                    Executioner.ChangeRole(data.Character);
                if (Executioner.Target.ContainsValue(data.Character.PlayerId))
                    Executioner.ChangeRoleByTarget(data.Character);
                if (Main.PlayerStates[data.Character.PlayerId].deathReason == PlayerState.DeathReason.etc) //死因が設定されていなかったら
                {
                    Main.PlayerStates[data.Character.PlayerId].deathReason = PlayerState.DeathReason.Disconnected;
                    Main.PlayerStates[data.Character.PlayerId].SetDead();
                }
                AntiBlackout.OnDisconnect(data.Character.Data);
                PlayerGameOptionsSender.RemoveSender(data.Character);
            }
            Logger.Info($"{data.PlayerName}(ClientID:{data.Id})が切断(理由:{reason}, ping:{AmongUsClient.Instance.Ping})", "Session");
        }
    }
    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.CreatePlayer))]
    class CreatePlayerPatch
    {
        public static void Postfix(AmongUsClient __instance, [HarmonyArgument(0)] ClientData client)
        {
            if (AmongUsClient.Instance.AmHost)
            {
                new LateTask(() =>
                {
                    if (client.Character == null) return;
                    if (AmongUsClient.Instance.IsGamePublic) Utils.SendMessage(string.Format(GetString("Message.AnnounceUsingTOH"), Main.PluginVersion), client.Character.PlayerId);
                    TemplateManager.SendTemplate("welcome", client.Character.PlayerId, true);
                }, 3f, "Welcome Message");
                if (Options.AutoDisplayLastResult.GetBool() && Main.PlayerStates.Count != 0 && Main.clientIdList.Contains(client.Id))
                {
                    new LateTask(() =>
                    {
                        if (!AmongUsClient.Instance.IsGameStarted && client.Character != null)
                        {
                            Main.isChatCommand = true;
                            Utils.ShowLastResult(client.Character.PlayerId);
                        }
                    }, 3f, "DisplayLastRoles");
                }
            }
        }
    }
}