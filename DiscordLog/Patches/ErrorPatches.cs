using Exiled.API.Features;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DiscordLog.Patches
{
    [HarmonyPatch(typeof(Log), nameof(Log.Error), new Type[] { typeof(object)})]
    public class ErrorPatchesObject
    {
        public static void Postfix(object message)
        {
            message = $"[{Assembly.GetCallingAssembly().GetName().Name}] {message}";
            if (DiscordLog.Instance.LOGError == null || !DiscordLog.Instance.LOGError.Contains(message.ToString()))
                DiscordLog.Instance.LOGError += message.ToString() + "\n";
        }
    }
    [HarmonyPatch(typeof(Log), nameof(Log.Error), new Type[] { typeof(string) })]
    public class ErrorPatchesString
    {
        public static void Postfix(string message)
        {
            message = $"[{Assembly.GetCallingAssembly().GetName().Name}] {message}";
            if (DiscordLog.Instance.LOGError == null || !DiscordLog.Instance.LOGError.Contains(message.ToString()))
                DiscordLog.Instance.LOGError += message.ToString() + "\n";
        }
    }
}
