using Exiled.API.Features;
using HarmonyLib;
using InventorySystem;
using InventorySystem.Configs;
using InventorySystem.Items;
using InventorySystem.Items.Pickups;
using Mirror;
using NorthwoodLib.Pools;
using Scp2536.Scenarios;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace DiscordLog.Patches
{
    [HarmonyPatch(typeof(Scp2536Scenario_AddItem), nameof(Scp2536Scenario_AddItem.ServerActivateScenario))]
    public class Scp2536Scenario_AddItemPatches
    {
		public static bool Prefix(Scp2536Scenario_AddItem __instance, ReferenceHub giftee, GameObject giftObj)
		{
			try
			{
				if (!NetworkServer.active)
				{
					Debug.LogWarning("[Server] function 'System.Void Scp2536.Scenarios.Scp2536Scenario_AddItem::ServerActivateScenario(ReferenceHub,UnityEngine.GameObject)' called when server was not active");
					return false;
				}
				ItemBase itemBase;
				ItemType ItemGift = __instance.ItemToAdd;
				if (InventoryItemLoader.AvailableItems.TryGetValue(ItemGift, out itemBase))
				{
					Player player = Player.Get(giftee);
					DiscordLog.Instance.LOG += $":christmas_tree: ``{player.Nickname}`` ({EventHandlers.ConvertID(player.UserId)}) a eu en cadeaux : {ItemGift}\n";

					bool flag = false;
					sbyte b = 0;
					using (Dictionary<ushort, ItemBase>.ValueCollection.Enumerator enumerator = giftee.inventory.UserInventory.Items.Values.GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							if (enumerator.Current.Category == itemBase.Category)
							{
								b += 1;
							}
						}
					}
					if (Mathf.Abs(InventoryLimits.GetCategoryLimit(itemBase.Category, giftee)) <= b)
					{
						flag = true;
					}
					if (flag)
					{
						PickupSyncInfo psi = new PickupSyncInfo
						{
							ItemId = __instance.ItemToAdd,
							Serial = ItemSerialGenerator.GenerateNext(),
							Weight = itemBase.Weight
						};
						ItemPickupBase itemPickupBase = giftee.inventory.ServerCreatePickup(itemBase, psi, true);
						itemPickupBase.transform.position = giftObj.transform.position + Vector3.up;
						itemPickupBase.RefreshPositionAndRotation();
						return false;
					}
					ItemBase itemBase2 = giftee.inventory.ServerAddItem(__instance.ItemToAdd, 0, null);
					if (itemBase2 != null)
					{
						__instance.ServerAfterActivate(giftee, itemBase2);
					}
				}
				return false;
			}
			catch
			{
				Log.Error("Error in Scp2536Scenario_AddItem Postfix");
				return true;
			}
		}
	}
	[HarmonyPatch(typeof(Scp2536Scenario_UpgradeItem), nameof(Scp2536Scenario_UpgradeItem.ServerActivateScenario))]
	public class Scp2536Scenario_UpgradeItemPatches
	{
		public static bool Prefix(Scp2536Scenario_UpgradeItem __instance, ReferenceHub giftee, GameObject giftObj)
		{
			try
			{
				if (!NetworkServer.active)
				{
					Debug.LogWarning("[Server] function 'System.Void Scp2536.Scenarios.Scp2536Scenario_UpgradeItem::ServerActivateScenario(ReferenceHub,UnityEngine.GameObject)' called when server was not active");
					return false;
				}
				short num = __instance.MaxUpgrades;
				if (num <= 0)
				{
					num = short.MaxValue;
				}
				List<KeyValuePair<ushort, ItemBase>> list = ListPool<KeyValuePair<ushort, ItemBase>>.Shared.Rent();
				List<ItemType> list2 = ListPool<ItemType>.Shared.Rent();
				foreach (KeyValuePair<ushort, ItemBase> keyValuePair in giftee.inventory.UserInventory.Items)
				{
					if (__instance.ServerCanUpgrade(keyValuePair.Value))
					{
						ItemType itemType = __instance.ServerGetUpgrade(giftee, keyValuePair);
						if (itemType != ItemType.None)
						{
							num -= 1;
							list2.Add(itemType);
							list.Add(keyValuePair);
						}
					}
					if (num <= 0)
					{
						break;
					}
				}
				List<ItemType> ItemType1 = new List<ItemType>();
				List<ItemType> ItemType2 = new List<ItemType>();
				foreach (KeyValuePair<ushort, ItemBase> keyValuePair2 in list)
				{
					giftee.inventory.ServerRemoveItem(keyValuePair2.Key, null);
					ItemType1.Add(keyValuePair2.Value.ItemTypeId);
				}
				foreach (ItemType type in list2)
				{
					ItemBase itemBase = giftee.inventory.ServerAddItem(type, 0, null);
					if (itemBase != null)
					{
						__instance.ServerPostUpgrade(giftee, itemBase);
					}
					ItemType2.Add(type);
				}
				ListPool<KeyValuePair<ushort, ItemBase>>.Shared.Return(list);
				ListPool<ItemType>.Shared.Return(list2);

				Player player = Player.Get(giftee);
				DiscordLog.Instance.LOG += $":christmas_tree: ``{player.Nickname}`` ({EventHandlers.ConvertID(player.UserId)}) a échangé :\n";
				while (ItemType1?.Count > 0 || ItemType2?.Count > 0)
                {
					DiscordLog.Instance.LOG += $"{ItemType1?.FirstOrDefault()} -> {ItemType2?.FirstOrDefault()}\n";
					ItemType1?.RemoveAt(0);
					ItemType2?.RemoveAt(0);
				}
				return false;
			}
			catch
			{
				Log.Error("Error in Scp2536Scenario_UpgradeItem Prefix");
				return true;
			}
		}
	}
}
