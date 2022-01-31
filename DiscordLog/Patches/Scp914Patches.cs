using Exiled.API.Extensions;
using Exiled.API.Features;
using HarmonyLib;
using InventorySystem.Items;
using InventorySystem.Items.Pickups;
using NorthwoodLib.Pools;
using Scp914;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace DiscordLog.Patches
{
    [HarmonyPatch(typeof(Scp914Upgrader), nameof(Scp914Upgrader.Upgrade))]
    internal class Scp914Patches
    {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            List<CodeInstruction> newInstructions = ListPool<CodeInstruction>.Shared.Rent(instructions);
            const int index = 0;

            newInstructions.InsertRange(index, new[]
            {
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldarg_2),
                new CodeInstruction(OpCodes.Ldarg_3),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Scp914Patches), nameof(Scp914Events))),
            });

            for (int z = 0; z < newInstructions.Count; z++)
                yield return newInstructions[z];

            ListPool<CodeInstruction>.Shared.Return(newInstructions);
        }
        private static void Scp914Events(Collider[] intake, Scp914Mode mode, Scp914KnobSetting knob)
        {
            List<Player> Players = new List<Player>();
            List<ItemType> ItemTypes = new List<ItemType>();
            bool upgradeDropped = (mode & Scp914Mode.Dropped) == Scp914Mode.Dropped;
            bool upgradeInventory = (mode & Scp914Mode.Inventory) == Scp914Mode.Inventory;
            bool heldOnly = upgradeInventory && (mode & Scp914Mode.Held) == Scp914Mode.Held;


            if (upgradeDropped)
            {
                List<PickupSyncInfo> Items = new List<PickupSyncInfo>();
                foreach (Collider collider in intake)
                {
                    GameObject gameObject = collider.transform.root.gameObject;
                    Player player = Player.Get(gameObject);

                    if (player != null && !Players.Contains(player))
                        Players.Add(player);
                    else if (gameObject.TryGetComponent(out ItemPickupBase pickup) && !Items.Contains(pickup.Info))
                        ItemTypes.Add(pickup.Info.ItemId);
                }
            }

            if (upgradeInventory)
                foreach (var player in Players)
                    foreach (var item in player.Inventory.UserInventory.Items)
                        if (!heldOnly || item.Key == player.Inventory.CurItem.SerialNumber)
                            ItemTypes.Add(item.Value.ItemTypeId);

            ItemTypes.OrderBy(x => x);

            string str;
            if (EventHandlers.Use914 != null)
                str = $":gear: SCP-914 a été enclenché en {knob} par ``{EventHandlers.Use914.Nickname}`` ({EventHandlers.ConvertID(EventHandlers.Use914.UserId)}) :\n";
            else
                str = $":gear: SCP-914 a été enclenché en {knob} par Unknown :\n";

            if (ItemTypes.Any())
            {
                str += $"**Item{(ItemTypes.Count() <= 1 ? "" : "s")}**\n";
                foreach (ItemType item in ItemTypes)
                    if (!ItemExtensions.IsAmmo(item))
                        str += $"   - {item}\n";
            }
            if (Players.Any())
            {
                str += $"**Joueur{(Players.Count() <= 1 ? "" : "s")}**\n";
                foreach (Player player in Players)
                    str += $"   - ``{player.Nickname}`` ({EventHandlers.ConvertID(player.UserId)})\n";
            }
            EventHandlers.Use914 = null;
            DiscordLog.Instance.LOG += str;
        }
    }
}
