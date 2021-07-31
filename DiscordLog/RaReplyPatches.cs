using Exiled.API.Features;
using HarmonyLib;
using RemoteAdmin;

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
}
