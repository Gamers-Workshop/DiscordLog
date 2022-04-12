using Exiled.API.Features;
using HarmonyLib;
using InventorySystem.Items.ThrowableProjectiles;
using MapGeneration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DiscordLog.Patches
{
    public class Scp2176Patches
    {
		[HarmonyPatch(typeof(Scp2176Projectile), nameof(Scp2176Projectile.ServerShatter))]
		public class Scp2176DamagePatches
		{
			public static void Postfix(Scp2176Projectile __instance)
			{
				try
				{
					DiscordLog.Instance.LOG += $"<SCP2176:963534500120383539> SCP2176 a été cassé par {__instance.PreviousOwner.Nickname} : {Map.FindParentRoom(__instance.gameObject)?.Type}.\n";
				}
				catch
				{
					Log.Error("Error in Scp2176DamagePatches Postfix");
					return;
				}
			}
		}
	}
}
