using Exiled.API.Features;
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
            if (!text.StartsWith("$"))
                DiscordLog.Instance.LOGStaff += $"{text}\n";
        }
    }

    [HarmonyPatch(typeof(ConsoleCommandSender), nameof(ConsoleCommandSender.RaReply))]
    public class GamCoreReplyPAtches
    {
        public static void Prefix(string text)
        {
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
                List<string> args = query.Trim().Split(QueryProcessor.SpaceArray, 512, StringSplitOptions.RemoveEmptyEntries).ToList();
                if (args.IsEmpty()) return;
                if (query.StartsWith("$0 1"))
                    return;

                Player player = sender is PlayerCommandSender playerCommandSender ? Player.Get(playerCommandSender) : Server.Host;

                switch (args.ElementAtOrDefault(0).ToLower())
                {
                    case "jail":
                        DiscordLog.Instance.LOGStaff += $":keyboard: {Extensions.LogPlayer(player)} a jail {Extensions.LogPlayer(Player.Get(args.ElementAtOrDefault(1)))}.\n";
                        return;
                    case "forceclass":
                        if (Enum.TryParse(args.ElementAtOrDefault(2), out RoleType role))
                        {
                            string Receiver = string.Empty;

                            string[] Users = args.ElementAtOrDefault(1).Split('.');
                            List<Player> PlyList = new();
                            foreach (string s in Users)
                            {
                                if (int.TryParse(s, out int id) && Player.Get(id) is not null)
                                    PlyList.Add(Player.Get(id));
                                else if (Player.Get(s) is not null)
                                    PlyList.Add(Player.Get(s));
                            }
                            foreach (Player ply in PlyList)
                            {
                                Receiver += $"\n - {Extensions.LogPlayer(ply)}";
                            }
                            DiscordLog.Instance.LOGStaff += $":keyboard: {Extensions.LogPlayer(player)} a changé en {role} : {Receiver}\n";
                            return;
                        }
                        break;
                    case "give":
                        if (Enum.TryParse(args.ElementAtOrDefault(2), out ItemType itemType))
                        {
                            string Receiver = string.Empty;

                            string[] Users = args.ElementAtOrDefault(1).Split('.');
                            List<Player> PlyList = new();
                            foreach (string s in Users)
                            {
                                if (int.TryParse(s, out int id) && Player.Get(id) is not null)
                                    PlyList.Add(Player.Get(id));
                                else if (Player.Get(s) is not null)
                                    PlyList.Add(Player.Get(s));
                            }
                            foreach (Player ply in PlyList)
                            {
                                Receiver += $"\n - {Extensions.LogPlayer(ply)}";
                            }
                            DiscordLog.Instance.LOGStaff += $":keyboard: {Extensions.LogPlayer(player)} a donné : {itemType} {Receiver}\n";
                            return;
                        }
                        break;
                    case "overwatch":
                        {
                            string Receiver = string.Empty;

                            string[] Users = args.ElementAtOrDefault(1).Split('.');
                            List<Player> PlyList = new();
                            foreach (string s in Users)
                            {
                                if (int.TryParse(s, out int id) && Player.Get(id) is not null)
                                    PlyList.Add(Player.Get(id));
                                else if (Player.Get(s) is not null)
                                    PlyList.Add(Player.Get(s));
                            }
                            foreach (Player ply in PlyList)
                            {
                                Receiver += $"\n - {Extensions.LogPlayer(ply)}";
                            }
                            if (args.ElementAtOrDefault(2) is "0")
                            {
                                DiscordLog.Instance.LOGStaff += $":keyboard: {Extensions.LogPlayer(player)} à enlever l'overwatch : {Receiver}\n";
                                return;
                            }
                            if (args.ElementAtOrDefault(2) is "1")
                            {
                                DiscordLog.Instance.LOGStaff += $":keyboard: {Extensions.LogPlayer(player)} à mis l'overwatch : {Receiver}\n";
                                return;
                            }
                        }
                        break;
                    case "bring":
                        {
                            string Receiver = string.Empty;

                            string[] Users = args.ElementAtOrDefault(1).Split('.');
                            List<Player> PlyList = new();
                            foreach (string s in Users)
                            {
                                if (int.TryParse(s, out int id) && Player.Get(id) is not null)
                                    PlyList.Add(Player.Get(id));
                                else if (Player.Get(s) is not null)
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
                        DiscordLog.Instance.LOGStaff += $":keyboard: {Extensions.LogPlayer(player)} se tp à {Extensions.LogPlayer(Player.Get(args.ElementAtOrDefault(1)))}.\n";
                        return;
                    case "$1":
                        {
                            string Receiver = string.Empty;

                            string[] Users = args.ElementAtOrDefault(1).Split('.');
                            List<Player> PlyList = new();
                            foreach (string s in Users)
                            {
                                if (int.TryParse(s, out int id) && Player.Get(id) is not null)
                                    PlyList.Add(Player.Get(id));
                                else if (Player.Get(s) is not null)
                                    PlyList.Add(Player.Get(s));
                            }
                            foreach (Player ply in PlyList)
                            {
                                Receiver += $"\n - {Extensions.LogPlayer(ply)}";
                            }
                            DiscordLog.Instance.LOGStaff += $":keyboard: {Extensions.LogPlayer(player)} a demandé les donnée de {Receiver} : {(args.ElementAtOrDefault(1) == "1" ? "REQUEST" : "REQUEST-IP")}\n";
                        }
                        return;
                    case "$3":
                        {
                            string Receiver = string.Empty;

                            string[] Users = args.ElementAtOrDefault(1).Split('.');
                            List<Player> PlyList = new();
                            foreach (string s in Users)
                            {
                                if (int.TryParse(s, out int id) && Player.Get(id) is not null)
                                    PlyList.Add(Player.Get(id));
                                else if (Player.Get(s) is not null)
                                    PlyList.Add(Player.Get(s));
                            }
                            foreach (Player ply in PlyList)
                            {
                                Receiver += $"\n - {Extensions.LogPlayer(ply)}";
                            }
                            DiscordLog.Instance.LOGStaff += $":keyboard: {Extensions.LogPlayer(player)} a demandé les donnée de {Receiver} : REQUEST-AUTH\n";
                        }
                        return;
                    case "effect":
                        {
                            string Receiver = string.Empty;

                            string[] Users = args.ElementAtOrDefault(1).Split('.');
                            List<Player> PlyList = new();
                            foreach (string s in Users)
                            {
                                if (int.TryParse(s, out int id) && Player.Get(id) is not null)
                                    PlyList.Add(Player.Get(id));
                                else if (Player.Get(s) is not null)
                                    PlyList.Add(Player.Get(s));
                            }
                            foreach (Player ply in PlyList)
                            {
                                Receiver += $"\n - {Extensions.LogPlayer(ply)}";
                            }
                            DiscordLog.Instance.LOGStaff += $":keyboard: {Extensions.LogPlayer(player)} a envoyé {args.ElementAtOrDefault(2)} : {Receiver}\n";
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

                            string[] Users = args.ElementAtOrDefault(1).Split('.');
                            List<Player> PlyList = new();
                            foreach (string s in Users)
                            {
                                if (int.TryParse(s, out int id) && Player.Get(id) is not null)
                                    PlyList.Add(Player.Get(id));
                                else if (Player.Get(s) is not null)
                                    PlyList.Add(Player.Get(s));
                            }
                            foreach (Player ply in PlyList)
                            {
                                Receiver += $"\n - {Extensions.LogPlayer(ply)}";
                            }
                            DiscordLog.Instance.LOGStaff += $":keyboard: {Extensions.LogPlayer(player)} à {args.ElementAtOrDefault(0)} : {Receiver}\n";
                        }
                        return;
                }
                DiscordLog.Instance.LOGStaff += $":keyboard: {Extensions.LogPlayer(player)} a envoyé ``{query}``.\n";
            }
            catch (System.Exception ex)
            {
                Exiled.API.Features.Log.Error($"Error In LogCommand by the command ({query}) and the player [{sender?.Nickname}] : {ex}");
            }
            Harmony.DEBUG = false;
        }
    }
}
