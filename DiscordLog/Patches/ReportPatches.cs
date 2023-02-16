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
    [HarmonyPatch(typeof(CheaterReport), nameof(CheaterReport.SubmitReport))]
    public class ReportPatches
    {
        public static bool Prefix(ref bool __result) 
        {
            __result = true;
            return false; 
        }
    }
}
