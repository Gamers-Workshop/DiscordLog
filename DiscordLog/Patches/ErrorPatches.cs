﻿using Exiled.API.Features;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordLog.Patches
{
    [HarmonyPatch(typeof(Log), nameof(Log.Error))]
    public class ErrorPatches
    {
        public static void Postfix(object message)
        {
            DiscordLog.Instance.LOGError += message.ToString();
        }
    }

}