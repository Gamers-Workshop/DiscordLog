﻿using Exiled.API.Features;
using HarmonyLib;
using NorthwoodLib.Pools;
using GameCore;
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
        public static void Prefix(string text)
        {
            if (!text.Contains("REQUEST_DATA:PLAYER_LIST"))
                DiscordLog.Instance.LOGStaff += $"{text}\n";
        }
    }

    [HarmonyPatch(typeof(ConsoleCommandSender), nameof(ConsoleCommandSender.RaReply))]
    public class GamCoreReplyPAtches
    {
        public static void Prefix(string text)
        {
            if (!text.Contains("REQUEST_DATA:PLAYER_LIST"))
                DiscordLog.Instance.LOGStaff += $"{text}\n";
        }
    }

    [HarmonyPatch(typeof(CommandProcessor), nameof(CommandProcessor.ProcessQuery))]
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
                if (args.Count() == 0) return;
                if (args[0].ToUpperInvariant() == "REQUEST_DATA")
                    return;

                Player player = sender is PlayerCommandSender playerCommandSender ? Player.Get(playerCommandSender) : Server.Host;

                switch (args[0])
                {
                    case "jail":
                        {
                            {
                                Player Jailed = Player.Get(args[1]);
                                DiscordLog.Instance.LOGStaff += $":keyboard: {Extensions.LogPlayer(player)} a jail {Extensions.LogPlayer(Jailed)}.\n";
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
                                    Receiver += $"\n - {Extensions.LogPlayer(ply)}";
                                }
                                DiscordLog.Instance.LOGStaff += $":keyboard: {Extensions.LogPlayer(player)} a changé en {(RoleType)Role} : {Receiver}\n";
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
                                    Receiver += $"\n - {Extensions.LogPlayer(ply)}\n";
                                }
                                DiscordLog.Instance.LOGStaff += $":keyboard: {Extensions.LogPlayer(player)} a donné : {(ItemType)Item} {Receiver}\n";
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
                                Receiver += $"\n - {Extensions.LogPlayer(ply)}";
                            }
                            if (args[1] == "0")
                            {
                                DiscordLog.Instance.LOGStaff += $":keyboard: {Extensions.LogPlayer(player)} à enlever l'overwatch : {Receiver}\n";
                            }
                            else if (args[1] == "1")
                            {
                                DiscordLog.Instance.LOGStaff += $":keyboard: {Extensions.LogPlayer(player)} à mis l'overwatch : {Receiver}\n";
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
                                Receiver += $"\n - {Extensions.LogPlayer(ply)}";
                            }
                            DiscordLog.Instance.LOGStaff += $":keyboard: {Extensions.LogPlayer(player)} à tp les joueurs sur lui : {Receiver}\n";
                        }
                        return;
                    case "goto":
                        {
                            Player player2 = Player.Get(args[1]);
                            DiscordLog.Instance.LOGStaff += $":keyboard: {Extensions.LogPlayer(player)} se tp à ``{player2.Nickname}`` ({player2.UserId}).\n";
                        }
                        return;
                    case "request_data":
                        {
                            Player player2 = Player.Get(args[2]);
                            DiscordLog.Instance.LOGStaff += $":keyboard: {Extensions.LogPlayer(player)} a demandé les donnée de {Extensions.LogPlayer(player2)} : {args[1]}\n";
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
                                Receiver += $"\n - {Extensions.LogPlayer(ply)}";
                            }
                            DiscordLog.Instance.LOGStaff += $":keyboard: {Extensions.LogPlayer(player)} a envoyé {args[2]} : {Receiver}\n";
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
                                Receiver += $"\n - {Extensions.LogPlayer(ply)}";
                            }
                            DiscordLog.Instance.LOGStaff += $":keyboard: {Extensions.LogPlayer(player)} à {args[0]} : {Receiver}\n";
                        }
                        return;
                    default:
                        {
                            string str1 = null;
                            foreach (string str2 in args)
                                str1 += $"{str2} ";
                            DiscordLog.Instance.LOGStaff += $":keyboard: {Extensions.LogPlayer(player)} a envoyé ``{str1.TrimEnd(' ')}``.\n";
                            return;
                        }
                }
            }
            catch (System.Exception ex)
            {
                Exiled.API.Features.Log.Error($"Error In LogCommand : {ex}");
            }
            Harmony.DEBUG = false;
        }
    }
}
