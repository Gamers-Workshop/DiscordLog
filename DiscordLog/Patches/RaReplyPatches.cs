using Exiled.API.Features;
using HarmonyLib;
using NorthwoodLib.Pools;
using RemoteAdmin;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Threading.Tasks;
using System.Linq;

using static HarmonyLib.AccessTools;

namespace DiscordLog
{
    [HarmonyPatch(typeof(PlayerCommandSender), nameof(PlayerCommandSender.RaReply))]
    public class RaReplyPatches
    {
        public static void Prefix(string text, bool success, bool logToConsole, string overrideDisplay)
        {
            if (!text.Contains("REQUEST_DATA:PLAYER_LIST"))
                DiscordLog.Instance.LOGStaff += $"{text}\n";
        }
    }


    //[HarmonyPatch(typeof(CommandProcessor), nameof(CommandProcessor.ProcessQuery))]
    internal class CommandLogging
    {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            Harmony.DEBUG = true;
            List<CodeInstruction> newInstructions = ListPool<CodeInstruction>.Shared.Rent(instructions);
            const int index = 0;

            newInstructions.InsertRange(index, new[]
            {
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldarg_1),
                new CodeInstruction(OpCodes.Call, Method(typeof(CommandLogging), nameof(LogCommand))),
            });

            for (int z = 0; z < newInstructions.Count; z++)
                yield return newInstructions[z];

            ListPool<CodeInstruction>.Shared.Return(newInstructions);
            Harmony.DEBUG = false;
        }

        private static void LogCommand(string query, CommandSender sender)
        {
            Harmony.DEBUG = true;
            try
            {
                string[] args = query.Trim().Split(QueryProcessor.SpaceArray, 512, StringSplitOptions.RemoveEmptyEntries);
                if (args[0].ToUpperInvariant() == "REQUEST_DATA")
                    return;

                Player player = sender is PlayerCommandSender playerCommandSender ? Player.Get(playerCommandSender) : Server.Host;

                switch (args[0])
                {
                    case "oban":
                        {

                        }
                        return;
                    case "jail":
                        {
                            {
                                Player Jailed = Player.Get(args[1]);
                                DiscordLog.Instance.LOGStaff += $":keyboard: ``{player.Nickname}`` ({EventHandlers.ConvertID(player.UserId)}) a jail ``{Jailed.Nickname}`` ({EventHandlers.ConvertID(Jailed.UserId)}).\n";
                            }
                        }
                        return;
                    case "forceclass":
                        {
                            if (int.TryParse(args[2], out int Role))
                            {
                                string Receiver = string.Empty;

                                string[] Users = args[1].Split('.');
                                List<Player> PlyList = new List<Player>();
                                foreach (string s in Users)
                                {
                                    if (int.TryParse(s, out int id) && Player.Get(id) != null)
                                        PlyList.Add(Player.Get(id));
                                    else if (Player.Get(s) != null)
                                        PlyList.Add(Player.Get(s));
                                }
                                foreach (Player ply in PlyList)
                                {
                                    Receiver += $"\n - ``{ply.Nickname}`` ({EventHandlers.ConvertID(ply.UserId)})";
                                }
                                DiscordLog.Instance.LOGStaff += $":keyboard: ``{player.Nickname}`` ({EventHandlers.ConvertID(player.UserId)}) a changé en {(RoleType)Role} : {Receiver}\n";
                            }
                        }
                        return;
                    case "give":
                        {
                            if (int.TryParse(args[2], out int Item))
                            {
                                string Receiver = string.Empty;

                                string[] Users = args[1].Split('.');
                                List<Player> PlyList = new List<Player>();
                                foreach (string s in Users)
                                {
                                    if (int.TryParse(s, out int id) && Player.Get(id) != null)
                                        PlyList.Add(Player.Get(id));
                                    else if (Player.Get(s) != null)
                                        PlyList.Add(Player.Get(s));
                                }
                                foreach (Player ply in PlyList)
                                {
                                    Receiver += $"\n - ``{ply.Nickname}`` ({EventHandlers.ConvertID(ply.UserId)})\n";
                                }
                                DiscordLog.Instance.LOGStaff += $":keyboard: ``{player.Nickname}`` ({EventHandlers.ConvertID(player.UserId)}) a donné : {(ItemType)Item} {Receiver}\n";
                            }
                        }
                        return;
                    case "overwatch":
                        {
                            string Receiver = string.Empty;

                            string[] Users = args[1].Split('.');
                            List<Player> PlyList = new List<Player>();
                            foreach (string s in Users)
                            {
                                if (int.TryParse(s, out int id) && Player.Get(id) != null)
                                    PlyList.Add(Player.Get(id));
                                else if (Player.Get(s) != null)
                                    PlyList.Add(Player.Get(s));
                            }
                            foreach (Player ply in PlyList)
                            {
                                Receiver += $"\n - ``{ply.Nickname}`` ({EventHandlers.ConvertID(ply.UserId)})";
                            }
                            if (args[1] == "0")
                            {
                                DiscordLog.Instance.LOGStaff += $":keyboard: ``{player.Nickname}`` ({EventHandlers.ConvertID(player.UserId)}) à enlever l'overwatch : {Receiver}\n";
                            }
                            else if (args[1] == "1")
                            {
                                DiscordLog.Instance.LOGStaff += $":keyboard: ``{player.Nickname}`` ({EventHandlers.ConvertID(player.UserId)}) à mis l'overwatch : {Receiver}\n";
                            }
                        }
                        return;
                    case "bring":
                        {
                            string Receiver = string.Empty;

                            string[] Users = args[1].Split('.');
                            List<Player> PlyList = new List<Player>();
                            foreach (string s in Users)
                            {
                                if (int.TryParse(s, out int id) && Player.Get(id) != null)
                                    PlyList.Add(Player.Get(id));
                                else if (Player.Get(s) != null)
                                    PlyList.Add(Player.Get(s));
                            }
                            foreach (Player ply in PlyList)
                            {
                                Receiver += $"\n - ``{ply.Nickname}`` ({EventHandlers.ConvertID(ply.UserId)})";
                            }
                            DiscordLog.Instance.LOGStaff += $":keyboard: ``{player.Nickname}`` ({EventHandlers.ConvertID(player.UserId)}) à tp les joueurs sur lui : {Receiver}\n";
                        }
                        return;
                    case "goto":
                        {
                            Player player2 = Player.Get(args[1]);
                            DiscordLog.Instance.LOGStaff += $":keyboard: ``{player.Nickname}`` ({EventHandlers.ConvertID(player.UserId)}) se tp à ``{player2.Nickname}`` ({player2.UserId}).\n";
                        }
                        return;
                    case "request_data":
                        {
                            Player player2 = Player.Get(args[2]);
                            DiscordLog.Instance.LOGStaff += $":keyboard: ``{player.Nickname}`` ({EventHandlers.ConvertID(player.UserId)}) a demandé les donnée de ``{player2.Nickname}`` ({EventHandlers.ConvertID(player2.UserId)}) : {args[1]}\n";
                        }
                        return;
                    case "effect":
                        {
                            string Receiver = string.Empty;

                            string[] Users = args[1].Split('.');
                            List<Player> PlyList = new List<Player>();
                            foreach (string s in Users)
                            {
                                if (int.TryParse(s, out int id) && Player.Get(id) != null)
                                    PlyList.Add(Player.Get(id));
                                else if (Player.Get(s) != null)
                                    PlyList.Add(Player.Get(s));
                            }
                            foreach (Player ply in PlyList)
                            {
                                Receiver += $"\n - ``{ply.Nickname}`` ({EventHandlers.ConvertID(ply.UserId)})";
                            }
                            DiscordLog.Instance.LOGStaff += $":keyboard: ``{player.Nickname}`` ({EventHandlers.ConvertID(player.UserId)}) a envoyé {args[2]} : {Receiver}\n";
                        }
                        return;
                    case "mute":
                    case "unmute":
                    case "imute":
                    case "iunmute":
                    case "disarm":
                    case "release":
                        {
                            string Receiver = string.Empty;

                            string[] Users = args[1].Split('.');
                            List<Player> PlyList = new List<Player>();
                            foreach (string s in Users)
                            {
                                if (int.TryParse(s, out int id) && Player.Get(id) != null)
                                    PlyList.Add(Player.Get(id));
                                else if (Player.Get(s) != null)
                                    PlyList.Add(Player.Get(s));
                            }
                            foreach (Player ply in PlyList)
                            {
                                Receiver += $"\n - ``{ply.Nickname}`` ({EventHandlers.ConvertID(ply.UserId)})";
                            }
                            DiscordLog.Instance.LOGStaff += $":keyboard: ``{player.Nickname}`` ({EventHandlers.ConvertID(player.UserId)}) à {args[0]} : {Receiver}\n";
                        }
                        return;
                    default:
                        {
                            DiscordLog.Instance.LOGStaff += $":keyboard: ``{player.Nickname}`` ({EventHandlers.ConvertID(player.UserId)}) a envoyé ``{string.Concat(args)}``.\n";
                            return;
                        }
                }
            }
            catch (System.Exception ex)
            {
                Log.Error($"Error In LogCommand : {ex}");
            }
            Harmony.DEBUG = false;
        }
    }
}
