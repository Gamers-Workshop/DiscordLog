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

namespace DiscordLog.Patches
{
    [HarmonyPatch(typeof(PlayerCommandSender), nameof(PlayerCommandSender.RaReply))]
    public class RaReplyPatches
    {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            List<CodeInstruction> newInstructions = ListPool<CodeInstruction>.Shared.Rent(instructions);
            const int index = 0;

            Label returnLabel = generator.DefineLabel();

            newInstructions.InsertRange(index, new[]
            {
                new (OpCodes.Ldarg_1),
                new (OpCodes.Ldstr, "$"),
                new (OpCodes.Callvirt, Method(typeof(string), nameof(string.StartsWith),new[] { typeof(string) })),
                new (OpCodes.Brtrue_S, returnLabel),
                new (OpCodes.Call, PropertyGetter(typeof(DiscordLog), nameof(DiscordLog.Instance))),
                new (OpCodes.Dup),
                new (OpCodes.Ldfld, Field(typeof(DiscordLog), nameof(DiscordLog.Instance.LOGStaff))),
                new (OpCodes.Ldarg_1),
                new (OpCodes.Ldstr, "\n"),
                new (OpCodes.Call, Method(typeof(string), nameof(string.Concat),new[] { typeof(string),typeof(string),typeof(string) })),
                new (OpCodes.Stfld, Field(typeof(DiscordLog), nameof(DiscordLog.Instance.LOGStaff))),
                new CodeInstruction(OpCodes.Nop).WithLabels(returnLabel),
            });

            for (int z = 0; z < newInstructions.Count; z++)
                yield return newInstructions[z];

            ListPool<CodeInstruction>.Shared.Return(newInstructions);
        }
    }

    [HarmonyPatch(typeof(ServerConsoleSender), nameof(ServerConsoleSender.RaReply))]
    public class GamCoreReplyPAtches
    {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            List<CodeInstruction> newInstructions = ListPool<CodeInstruction>.Shared.Rent(instructions);
            const int index = 0;

            newInstructions.InsertRange(index, new[]
            {
                new (OpCodes.Nop),
                new (OpCodes.Call, PropertyGetter(typeof(DiscordLog), nameof(DiscordLog.Instance))),
                new (OpCodes.Dup),
                new (OpCodes.Ldfld, Field(typeof(DiscordLog), nameof(DiscordLog.Instance.LOGStaff))),
                new (OpCodes.Ldarg_1),
                new (OpCodes.Ldstr, "\n"),
                new (OpCodes.Call, Method(typeof(string), nameof(string.Concat),new[] { typeof(string),typeof(string),typeof(string) })),
                new (OpCodes.Stfld, Field(typeof(DiscordLog), nameof(DiscordLog.Instance.LOGStaff))),
                new CodeInstruction(OpCodes.Nop),
            });

            for (int z = 0; z < newInstructions.Count; z++)
                yield return newInstructions[z];

            ListPool<CodeInstruction>.Shared.Return(newInstructions);
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
                if (query.StartsWith("$") && query.ElementAtOrDefault(1) is not '1' or '3')
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
                            DiscordLog.Instance.LOGStaff += $":keyboard: {Extensions.LogPlayer(player)} a demandé les donnée de {LogPlayerFromCommand(args.ElementAtOrDefault(1))} : {(args.ElementAtOrDefault(1) is "1" ? "REQUEST" : "REQUEST-IP")}\n";
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
                    DiscordLog.Instance.LOGStaff += $":keyboard: {Extensions.LogPlayer(player)} à {args.ElementAtOrDefault(0)} ``{Regex.Replace(Extensions.FormatArguments(args, 2), "<[^>]*?>", string.Empty).DiscordSanitize()}``: {LogPlayerFromCommand(args.ElementAtOrDefault(1))}\n";
                    return;
                }
                DiscordLog.Instance.LOGStaff += $":keyboard: {Extensions.LogPlayer(player)} a envoyé ``{Regex.Replace(query, "<[^>]*?>", string.Empty).DiscordSanitize()}``.\n";
            }
            catch (Exception ex)
            {
                Exiled.API.Features.Log.Error($"Error In LogCommand by the command ({query.DiscordSanitize()}) and the player [{sender?.Nickname.DiscordSanitize()}] : {ex}");
            }
        }

        public static string LogPlayerFromCommand(string Users)
        {
            string Receiver = string.Empty;
            List<Player> PlyList = new();
            foreach (string s in Users?.Split('.'))
            {
                if (int.TryParse(s, out int id) && Player.TryGet(id, out Player player))
                    PlyList.Add(player);
                else if (Player.TryGet(s, out player))
                    PlyList.Add(player);
            }
            foreach (Player ply in PlyList)
            {
                Receiver += $"\n - {Extensions.LogPlayer(ply)}";
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
