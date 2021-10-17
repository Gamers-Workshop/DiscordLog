using Exiled.API.Features;
using HarmonyLib;
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
    internal class Scp914Patche
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
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Scp914Patche), nameof(Scp914Events))),
            });

            for (int z = 0; z < newInstructions.Count; z++)
                yield return newInstructions[z];

            ListPool<CodeInstruction>.Shared.Return(newInstructions);
        }
        private static void Scp914Events(Collider[] intake, Scp914Mode mode, Scp914KnobSetting knob)
        {
            List<Player> Players = new List<Player>();
            List<PickupSyncInfo> Items = new List<PickupSyncInfo>();

            foreach (Collider collider in intake)
            {
                GameObject gameObject = collider.transform.root.gameObject;
                Player player = Player.Get(gameObject);
                if (player != null && !Players.Contains(player))
                {
                    Players.Add(player);
                }
                else if (gameObject.TryGetComponent(out ItemPickupBase pickup) && !Items.Contains(pickup.Info))
                {
                    Items.Add(pickup.Info);
                }
            }
            Items.OrderBy(x => x.ItemId);
            string str;
            if (EventHandlers.Use914 != null)
                str = $":gear: SCP-914 a été enclenché en {knob} par ``{EventHandlers.Use914?.Nickname}`` ({EventHandlers.ConvertID(EventHandlers.Use914?.UserId)}) :\n";
            else
                str = $":gear: SCP-914 a été enclenché en {knob} par Unknow :\n";
            int ItemCount = (Items.Count() + (int)Players?.Where(x => x?.CurrentItem?.Type != ItemType.None).Count());
            bool PlayerItem = Players?.Where(x => x?.CurrentItem?.Type != ItemType.None).Count() != null;
            if (ItemCount > 0 || PlayerItem)
            {
                str += $"**Item{(ItemCount <= 1 ? "" : "s")}**\n";
                if (ItemCount > 0)
                    foreach (var item in Items)
                    {
                        if (!Exiled.API.Extensions.ItemExtensions.IsAmmo(item.ItemId) && item.ItemId != ItemType.None)
                            str += $"   - {item.ItemId}\n";
                    }
                if (PlayerItem)
                    foreach (Player player in Players)
                    {
                        if (!string.IsNullOrWhiteSpace(player?.CurrentItem?.Type.ToString()))
                            str += $"   - {player?.CurrentItem?.Type}\n";
                    }
            }
            if (Players.Count != 0)
            {
                str += $"**Joueur{(Players.Count() <= 1 ? "" : "s")}**\n";
                foreach (Player player in Players)
                {
                    str += $"   - ``{player.Nickname}`` ({EventHandlers.ConvertID(player.UserId)})\n";
                }
            }
            EventHandlers.Use914 = null;
            DiscordLog.Instance.LOG += str;
        }
    }
}
