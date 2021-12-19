using Exiled.API.Features;
using HarmonyLib;
using InventorySystem;
using InventorySystem.Searching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordLog.Patches
{
	//SCP244
	[HarmonyPatch(typeof(Scp244SearchCompletor), nameof(Scp244SearchCompletor.Complete))]
	public class Scp244Patches
	{
		public static void Postfix(Scp244SearchCompletor __instance)
		{
			try
			{
				Player player = Player.Get(__instance.Hub);
				DiscordLog.Instance.LOG += $":inbox_tray: ``{player.Nickname}`` ({EventHandlers.ConvertID(player.UserId)}) a récupéré {__instance.TargetPickup.Info.ItemId}.\n";
			}
			catch
			{
				Log.Error("Error in Scp244Patches Postfix");
				return;
			}
		}
	}
}
