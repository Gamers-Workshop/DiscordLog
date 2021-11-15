using Exiled.API.Features;
using HarmonyLib;
using InventorySystem;
//using InventorySystem.Items.Usables.Scp330;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordLog.Patches
{
	//SCP330 Log
    /*[HarmonyPatch(typeof(Scp330Bag), nameof(Scp330Bag.TryAddSpecific))]
	public class CandyPickup
    {
		public static bool Prefix(Scp330Bag __instance,ref bool __result,CandyKindID kind)
        {
			try
			{
				string Log = string.Empty;
				if (__instance.Candies.Count >= 6)
				{
					__result = false;
					return false;
				}
				__instance.Candies.Add(kind);
				Player player = Player.Get(__instance.Owner);
				DiscordLog.Instance.LOG += $":inbox_tray: ``{player.Nickname}`` ({EventHandlers.ConvertID(player.UserId)}) a récupéré un bonbon : {kind}.\n";
				__result = true;
				return false;
			}
			catch
            {
				Log.Error("Error in CandyPickup Prefix");
				return true;
			}
		}
	}
	[HarmonyPatch(typeof(Scp330Bag), nameof(Scp330Bag.TryRemove))]
	public class CandyDrop
    {
		public static bool Prefix(Scp330Bag __instance, ref CandyKindID __result,int index)
		{
			try
			{
				string Log = string.Empty;
				if (index < 0 || index > __instance.Candies.Count)
				{
					__result = CandyKindID.None;
					return false;
				}
				CandyKindID result = __instance.Candies[index];
				__instance.Candies.RemoveAt(index);
				__instance.ServerRefreshBag();
				Player player = Player.Get(__instance.Owner);
				DiscordLog.Instance.LOG += $":outbox_tray: ``{player.Nickname}`` ({EventHandlers.ConvertID(player.UserId)}) a jeté un bonbon : {result}.\n";
				__result = result;
				return false;
			}
			catch
			{
				Log.Error("Error in CandyPickup Prefix");
				return true;
			}
		}
	}*/
}
