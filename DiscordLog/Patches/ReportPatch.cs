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
			ref bool __state,
			string reporterUserId,
			string reportedUserId,
			string reason,
			ref int reportedId,
			string reporterNickname,
			string reportedNickname,
			bool friendlyFire)
        {
			if (DiscordLog.Instance.Config.WebhookReport != "none")
			{
				__state = true;
				return false;
			}
			return true;
        }
		static void Postfix(ref bool __result, bool __state)
		{
			__result = __state;
		}
	}
}