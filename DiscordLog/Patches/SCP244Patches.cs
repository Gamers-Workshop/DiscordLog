using Exiled.API.Features;
using HarmonyLib;
using InventorySystem;
using InventorySystem.Items.Usables.Scp244;
using InventorySystem.Searching;
using Mirror;
using PlayerStatsSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace DiscordLog.Patches
{
	//SCP244 quand il est récupéré
	[HarmonyPatch(typeof(Scp244SearchCompletor), nameof(Scp244SearchCompletor.Complete))]
	public class Scp244SearchCompletorPatches
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
				Log.Error("Error in Scp244SearchCompletorPatches Postfix");
				return;
			}
        }
    }
    //Scp244 quand il est utilisé
    [HarmonyPatch(typeof(Scp244Item), nameof(Scp244Item.ServerOnUsingCompleted))]
    public class Scp244UsingPatches
    {
        public static void Postfix(Scp244Item __instance)
        {
            try
            {
                Player player = Player.Get(__instance.Owner);
                DiscordLog.Instance.LOG += $":teapot: ``{ player.Nickname}`` ({EventHandlers.ConvertID(player.UserId)}) a ouvert {__instance.ItemTypeId}.\n";
            }
            catch
            {
                Log.Error("Error in Scp244UsingPatches Postfix");
                return;
            }
        }
    }
	//Scp244 Quand il est cassé
    [HarmonyPatch(typeof(Scp244DeployablePickup), nameof(Scp244DeployablePickup.Damage))]
	public class Scp244DamagePatches
	{
		public static void Postfix(Scp244DeployablePickup __instance, float damage, DamageHandlerBase handler, Vector3 exactHitPos)
		{
			try
			{
				if (__instance.State == Scp244State.Destroyed)
                {
					DamageHandler damageHandler = new DamageHandler(null, handler);
					DiscordLog.Instance.LOG += $":teapot: {__instance?.Info.ItemId} a été cassé par : {damageHandler.Type}.\n";
				}
			}
			catch
			{
				Log.Error("Error in Scp244UsingPatches Postfix");
				return;
			}
		}
	}

	//Scp244 quand il tombe
	[HarmonyPatch(typeof(Scp244DeployablePickup), nameof(Scp244DeployablePickup.UpdateRange))]
	public class Scp244UpdatePatches
	{
		public static bool Prefix(Scp244DeployablePickup __instance)
		{
			try
			{
				{
					if (__instance.ModelDestroyed && __instance._visibleModel.activeSelf)
					{
						__instance.Rb.constraints = RigidbodyConstraints.FreezeAll;
						__instance._visibleModel.SetActive(false);
					}
					if (!NetworkServer.active)
					{
						__instance._currentSizePercent = (float)__instance._syncSizePercent;
						__instance._currentSizePercent /= 255f;
						return false;
					}
					if (__instance.State == Scp244State.Idle && Vector3.Dot(__instance.transform.up, Vector3.up) < __instance._activationDot)
					{
						DiscordLog.Instance.LOG += $":teapot: {__instance?.Info.ItemId} c'est ouvert.\n";
						__instance.State = Scp244State.Active;
					}
					float num = (__instance.State == Scp244State.Active) ? __instance._timeToGrow : (-__instance._timeToDecay);
					__instance._currentSizePercent = Mathf.Clamp01(__instance._currentSizePercent + Time.deltaTime / num);
					__instance.Network_syncSizePercent = (byte)Mathf.RoundToInt(__instance._currentSizePercent * 255f);
					if (!__instance.ModelDestroyed || __instance._currentSizePercent > 0f)
					{
						return false;
					}
					__instance._timeToDecay -= Time.deltaTime;
					if (__instance._timeToDecay <= 0f)
					{
						NetworkServer.Destroy(__instance.gameObject);
					}
				}
				return false;
			}
			catch
			{
				Log.Error("Error in Scp244UpdatePatches Prefix");
				return true;
			}
		}
	}

}
