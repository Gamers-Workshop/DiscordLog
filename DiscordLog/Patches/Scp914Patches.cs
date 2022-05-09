using Exiled.API.Extensions;
using Exiled.API.Features;
using Exiled.API.Features.Items;
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
    public static class Scp914Patches2
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
            private static void Scp914Events(Collider[] intake, Scp914Mode mode, Scp914KnobSetting setting)
            {
                try
                {
                    List<Player> Players = new();
                    List<Pickup> PickupTypes = new();
                    List<Item> ItemTypes = new();

                    bool upgradeDropped = (mode & Scp914Mode.Dropped) == Scp914Mode.Dropped;
                    bool upgradeInventory = (mode & Scp914Mode.Inventory) == Scp914Mode.Inventory;
                    bool heldOnly = upgradeInventory && (mode & Scp914Mode.Held) == Scp914Mode.Held;


                    if (upgradeDropped)
                    {
                        foreach (Collider collider in intake)
                        {
                            GameObject gameObject = collider.transform.root.gameObject;
                            Player player = Player.Get(gameObject);

                            if (player is not null && !Players.Contains(player))
                                Players.Add(player);
                            else if (gameObject.TryGetComponent(out ItemPickupBase pickup) && !PickupTypes.Any(x => x.Serial == pickup.Info.Serial))
                                PickupTypes.Add(Pickup.Get(pickup));
                        }
                    }

                    if (upgradeInventory)
                        foreach (var player in Players)
                            foreach (var item in player.Inventory.UserInventory.Items)
                                if (!heldOnly || item.Key == player.Inventory.CurItem.SerialNumber)
                                    ItemTypes.Add(Item.Get(item.Value));

                    string str = $":gear: SCP-914 a été enclenché en {setting} par {Extensions.LogPlayer(EventHandlers.Use914)} :\n";

                    if (PickupTypes.Any() || ItemTypes.Any())
                    {
                        str += $"**Item{((PickupTypes.Count() + ItemTypes.Count()) > 1 ? "s" : "")}**\n";
                        foreach (var item in PickupTypes.OrderBy(x => x.Type))
                            if (!ItemExtensions.IsAmmo(item.Type))
                                str += $"   - {Extensions.LogPickup(item)}\n";
                        foreach (var item in ItemTypes)
                            str += $"   - {Extensions.LogItem(item)}\n";
                    }
                    if (Players.Any())
                    {
                        str += $"**Joueur{(Players.Count() > 1 ? "s" : "")}**\n";
                        foreach (Player player in Players)
                            str += $"   - {Extensions.LogPlayer(player)}\n";
                    }
                    EventHandlers.Use914 = null;
                    DiscordLog.Instance.LOG += str;
                }
                catch (Exception ex)
                {
                    Log.Error(ex);
                }
            }
        }
    }
}