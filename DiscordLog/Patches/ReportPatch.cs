using System;
using Exiled.API.Features;
using HarmonyLib;
using UnityEngine;
using Utf8Json;

namespace DiscordLog.Patches
{
	[HarmonyPatch(typeof(CheaterReport))]
	[HarmonyPatch(nameof(CheaterReport.SubmitReport))]
	internal static class LocalReportPatch
	{
		static bool Prefix(
			ref bool __state)
		{
			if (DiscordLog.Instance.Config.WebhookReport == "none")
				return true;

			__state = true;
			return false;

		}
		static void Postfix(ref bool __result, bool __state)
		{
			__result = __state;
		}
	}
}