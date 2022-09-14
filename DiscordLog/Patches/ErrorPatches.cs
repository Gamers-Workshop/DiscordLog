using Exiled.API.Features;
using HarmonyLib;
using NorthwoodLib.Pools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using static HarmonyLib.AccessTools;

namespace DiscordLog.Patches
{
    [HarmonyPatch(typeof(Log), nameof(Log.Error), new Type[] { typeof(object)})]
    public class ErrorPatchesObject
    {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            List<CodeInstruction> newInstructions = ListPool<CodeInstruction>.Shared.Rent(instructions);

            const int offset = 0;

            LocalBuilder flag = generator.DeclareLocal(typeof(bool));

            Label orLabel = generator.DefineLabel();
            Label notLabel = generator.DefineLabel();
            Label nullLabel = generator.DefineLabel();
            Label stringLabel = generator.DefineLabel();

            Label returnLabel = generator.DefineLabel();

            newInstructions.InsertRange(offset, new[]
            {
                new (OpCodes.Ldstr,"[{0}] ``` {1} ```\n"),
                new (OpCodes.Call, Method(typeof(Assembly),nameof(Assembly.GetCallingAssembly))),
                new (OpCodes.Callvirt, Method(typeof(Assembly), nameof(Assembly.GetName))),
                new (OpCodes.Callvirt, PropertyGetter(typeof(AssemblyName),nameof(AssemblyName.Name))),
                new (OpCodes.Ldarg_0),
                new (OpCodes.Call, Method(typeof(string),nameof(string.Format),new[] { typeof(string),typeof(object),typeof(object) })),
                new (OpCodes.Starg_S, 0),
                new (OpCodes.Call, PropertyGetter(typeof(DiscordLog),nameof(DiscordLog.Instance))),
                new (OpCodes.Ldfld, Field(typeof(DiscordLog),nameof(DiscordLog.Instance.LOGError))),
                new CodeInstruction(OpCodes.Brfalse_S, orLabel),
                new (OpCodes.Call, PropertyGetter(typeof(DiscordLog),nameof(DiscordLog.Instance))),
                new (OpCodes.Ldfld, Field(typeof(DiscordLog),nameof(DiscordLog.Instance.LOGError))),
                new (OpCodes.Ldarg_0),
                new (OpCodes.Callvirt, Method(typeof(object),nameof(object.ToString))),
                new (OpCodes.Callvirt, Method(typeof(string),nameof(string.Contains), new[] { typeof(string) })),
                new (OpCodes.Ldc_I4_0),
                new (OpCodes.Ceq),
                new (OpCodes.Br_S, notLabel),
                new CodeInstruction(OpCodes.Ldc_I4_1).WithLabels(orLabel),
                new CodeInstruction(OpCodes.Stloc_0, flag.LocalIndex).WithLabels(notLabel),
                new (OpCodes.Ldloc_0, flag.LocalIndex),
                new (OpCodes.Brfalse_S, returnLabel),
                new (OpCodes.Call, PropertyGetter(typeof(DiscordLog),nameof(DiscordLog.Instance))),
                new (OpCodes.Dup),
                new (OpCodes.Ldfld, Field(typeof(DiscordLog),nameof(DiscordLog.Instance.LOGError))),
                new (OpCodes.Ldarg_0),
                new (OpCodes.Brtrue_S, nullLabel),
                new (OpCodes.Ldnull),
                new (OpCodes.Br_S,stringLabel),
                new CodeInstruction(OpCodes.Ldarg_0).WithLabels(nullLabel),
                new (OpCodes.Callvirt, Method(typeof(object),nameof(object.ToString))),
                new CodeInstruction(OpCodes.Ldstr, "\n").WithLabels(stringLabel),
                new (OpCodes.Call, Method(typeof(string),nameof(string.Concat),new[]{ typeof(string),typeof(string),typeof(string) })),
                new (OpCodes.Stfld, Field(typeof(DiscordLog),nameof(DiscordLog.Instance.LOGError))),
                new CodeInstruction(OpCodes.Nop).WithLabels(returnLabel),
            });

            for (int z = 0; z < newInstructions.Count; z++)
                yield return newInstructions[z];

            ListPool<CodeInstruction>.Shared.Return(newInstructions);
        }
    }
    [HarmonyPatch(typeof(Log), nameof(Log.Error), new Type[] { typeof(string) })]
    public class ErrorPatchesString
    {
        public static void Postfix(string message)
        {
            message = $"[{Assembly.GetCallingAssembly().GetName().Name}] ``` {message} ```\n";
            if (DiscordLog.Instance.LOGError is null || !DiscordLog.Instance.LOGError.Contains(message))
                DiscordLog.Instance.LOGError += message + "\n";
        }
    }
}
