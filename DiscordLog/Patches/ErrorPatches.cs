using Exiled.API.Features;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordLog.Patches
{
    public class ErrorPatches
    {
        [HarmonyPatch(typeof(Log), nameof(Log.Error))]
        public static void Prefix(object message)
        {
            DiscordLog.Instance.LOGError += message.ToString();
        }
    }

}
