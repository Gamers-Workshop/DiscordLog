using Exiled.API.Features;
using HarmonyLib;
using NorthwoodLib.Pools;
using GameCore;
using RemoteAdmin;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Linq;

using static HarmonyLib.AccessTools;
using System.Text.RegularExpressions;
using PlayerRoles;
using UnityEngine;

namespace DiscordLog.Patches
{
    [HarmonyPatch(typeof(PlayerCommandSender), nameof(PlayerCommandSender.RaReply))]
    public class RaReplyPatches
    {
        private static void Postfix(PlayerCommandSender __instance, string text, bool success, bool logToConsole, string overrideDisplay)
        {
            if (!text.StartsWith("$") || text.Count() < 500)
                return;
            DiscordLog.Instance.LOGStaff += $"{text}\n";
        }
    }

    [HarmonyPatch(typeof(ServerConsoleSender), nameof(ServerConsoleSender.RaReply))]
    public class GamCoreReplyPAtches
    {
        private static void Postfix(ServerConsoleSender __instance, string text, bool success, bool logToConsole, string overrideDisplay)
        {
            if (!text.StartsWith("$") || text.Count() < 500)
                return;
            DiscordLog.Instance.LOGStaff += $"{text}\n";
        }
    }

    [HarmonyPatch(typeof(CommandProcessor), nameof(CommandProcessor.ProcessQuery))]
    internal class CommandLogging
    {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
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
        }

        private static void LogCommand(string query, CommandSender sender)
        {
            try
            {
                List<string> args = query.Trim().Split(QueryProcessor.SpaceArray, 512, StringSplitOptions.RemoveEmptyEntries).ToList();
                if (args.IsEmpty()) return;
                if (query.StartsWith("$") && query.ElementAtOrDefault(1) is not '0' or '7' or '8')
                    return;

                Player player = sender is PlayerCommandSender playerCommandSender ? Player.Get(playerCommandSender) : Server.Host;

                switch (args.ElementAtOrDefault(0)?.ToLower())
                {
                    case "jail":
                        DiscordLog.Instance.LOGStaff += $":keyboard: {Extensions.LogPlayer(player)} a jail {Extensions.LogPlayer(Player.Get(args.ElementAtOrDefault(1)))}.\n";
                        return;
                    case "forceclass":
                        {
                            RoleSpawnFlags spawnFlags = RoleSpawnFlags.All;
                            if (args.Count > 3 && byte.TryParse(args.ElementAtOrDefault(3), out byte b) && b is not 3)
                            {
                                spawnFlags = (RoleSpawnFlags)b;
                            }
                            DiscordLog.Instance.LOGStaff += $":keyboard: {Extensions.LogPlayer(player)} a changé en {args.ElementAtOrDefault(2)} {spawnFlags}: {LogPlayerFromCommand(args.ElementAtOrDefault(1))}\n";
                            return;
                        }
                    case "grantloadout":
                        {
                            if (Enum.TryParse(args.ElementAtOrDefault(2), out RoleTypeId role))
                            {
                                DiscordLog.Instance.LOGStaff += $":keyboard: {Extensions.LogPlayer(player)} a pris l'inventaire d'un {role}: {LogPlayerFromCommand(args.ElementAtOrDefault(1))}\n";
                                return;
                            }
                        }
                        break;
                    case "give":
                        {
                            DiscordLog.Instance.LOGStaff += $":keyboard: {Extensions.LogPlayer(player)} a donné : {ParseEnum<ItemType>(args.ElementAtOrDefault(2))} {LogPlayerFromCommand(args.ElementAtOrDefault(1))}\n";
                            return;
                        }
                    case "removeitem":
                        {
                            DiscordLog.Instance.LOGStaff += $":keyboard: {Extensions.LogPlayer(player)} a enlever : {ParseEnum<ItemType>(args.ElementAtOrDefault(2))} {LogPlayerFromCommand(args.ElementAtOrDefault(1))}\n";
                            return;
                        }
                    case "overwatch":
                        {
                            string Receiver = LogPlayerFromCommand(args.ElementAtOrDefault(1));
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
                    case "god":
                        {
                            string Receiver = LogPlayerFromCommand(args.ElementAtOrDefault(1));
                            if (args.ElementAtOrDefault(2) is "0")
                            {
                                DiscordLog.Instance.LOGStaff += $":keyboard: {Extensions.LogPlayer(player)} à enlever le godmod : {Receiver}\n";
                                return;
                            }
                            if (args.ElementAtOrDefault(2) is "1")
                            {
                                DiscordLog.Instance.LOGStaff += $":keyboard: {Extensions.LogPlayer(player)} à mis le godmod : {Receiver}\n";
                                return;
                            }
                        }
                        break;
                    case "bring":
                        {
                            DiscordLog.Instance.LOGStaff += $":keyboard: {Extensions.LogPlayer(player)} à tp les joueurs sur lui : {LogPlayerFromCommand(args.ElementAtOrDefault(1))}\n";
                        }
                        return;
                    case "goto":
                        DiscordLog.Instance.LOGStaff += $":keyboard: {Extensions.LogPlayer(player)} se tp à {LogPlayerFromCommand(args.ElementAtOrDefault(1))}.\n";
                        return;
                    case "$1":
                        {
                            DiscordLog.Instance.LOGStaff += $":keyboard: {Extensions.LogPlayer(player)} a demandé les donnée de {LogPlayerFromCommand(args.ElementAtOrDefault(2))} : {(args.ElementAtOrDefault(1) is "1" ? "REQUEST" : "REQUEST-IP")}\n";
                        }
                        return;
                    case "$3":
                        {
                            DiscordLog.Instance.LOGStaff += $":keyboard: {Extensions.LogPlayer(player)} a demandé les donnée de {LogPlayerFromCommand(args.ElementAtOrDefault(1))} : REQUEST-AUTH\n";
                        }
                        return;
                    case "pfx":
                        {
                            DiscordLog.Instance.LOGStaff += $":keyboard: {Extensions.LogPlayer(player)} a donné l'effect {args.ElementAtOrDefault(1)} Intensité {args.ElementAtOrDefault(2)} Durée {args.ElementAtOrDefault(3)} : {LogPlayerFromCommand(args.ElementAtOrDefault(4))}\n";
                        }
                        return;
                }
                if (args.ElementAtOrDefault(1)?.Contains('.') ?? false)
                {
                    DiscordLog.Instance.LOGStaff += $":keyboard: {Extensions.LogPlayer(player)} à {args.ElementAtOrDefault(0)} ``{Extensions.ConvertUnityTagToDiscord(Extensions.FormatArguments(args, 2)).DiscordLightSanitize()}``: {LogPlayerFromCommand(args.ElementAtOrDefault(1))}\n";
                    return;
                }
                DiscordLog.Instance.LOGStaff += $":keyboard: {Extensions.LogPlayer(player)} a envoyé ``{Extensions.ConvertUnityTagToDiscord(query).DiscordLightSanitize()}``.\n";
            }
            catch (Exception ex)
            {
                Exiled.API.Features.Log.Error($"Error In LogCommand by the command ({query.DiscordSanitize()}) and the player [{sender?.Nickname.DiscordSanitize()}] : {ex}");
            }
        }

        public static string LogPlayerFromCommand(string Users)
        {
            string Receiver = string.Empty;
            foreach (Player ply in Player.GetProcessedData(new(new[] { Users, }, 0, 1)))
            {
                Receiver += $"\n- {Extensions.LogPlayer(ply)}";
            }
            return Receiver;
        }
        public static string ParseEnum<TEnum>(string argument) where TEnum : struct, Enum
        {
            argument ??= string.Empty;
            string[] array = argument.Split(new char[] { '.' });
            List<string> names = new();
            for (int i = 0; i < array.Length; i++)
            {
                if (Enum.TryParse(array[i], out TEnum value))
                {
                    names.Add(Enum.GetName(typeof(TEnum), value));
                }
            }
            return string.Join(" ", names);
        }
    }
}
