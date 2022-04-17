using Exiled.API.Features;
using HarmonyLib;
using InventorySystem;
using InventorySystem.Items.Usables.Scp330;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordLog.Patches
{

	[HarmonyPatch(typeof(Scp330Bag), nameof(Scp330Bag.TryRemove))]
	public class CandyDrop
    {
		public static bool Prefix(Scp330Bag __instance, ref CandyKindID __result,int index)
		{
			try
			{
				if (index < 0 || index > __instance.Candies.Count)
				{
					__result = CandyKindID.None;
					return false;
				}
				CandyKindID result = __instance.Candies[index];
				__instance.Candies.RemoveAt(index);
				__instance.ServerRefreshBag();
				DiscordLog.Instance.LOG += $":outbox_tray: {Extensions.LogPlayer(Player.Get(__instance.Owner))} a jeté un bonbon : {result}.\n";
				__result = result;
				return false;
			}
			catch
			{
				Log.Error("Error in CandyPickup Prefix");
				return true;
			}
		}
	}
}
