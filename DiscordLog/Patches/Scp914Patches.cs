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
            List<ItemType> Items = new List<ItemType>();

            foreach (Collider collider in intake)
            {
                GameObject gameObject = collider.transform.root.gameObject;
                Player player = Player.Get(gameObject);
                if (player != null)
                {
                    Players.Add(player);
                }
                else if (gameObject.TryGetComponent(out ItemPickupBase pickup))
                {
                    Items.Add(pickup.Info.ItemId);
                }
            }

            string str;
            if (EventHandlers.Use914 != null)
                str = $":gear: SCP-914 a été enclenché en {knob} par ``{EventHandlers.Use914?.Nickname}`` ({EventHandlers.ConvertID(EventHandlers.Use914.UserId)}) :\n";
            else
                str = $":gear: SCP-914 a été enclenché en {knob} par Unknow :\n";
            bool Item = Items.Count != 0;
            bool PlayerItem = Players.Where(x => x.CurrentItem.Type != ItemType.None).Count() != 0;
            if (Item || PlayerItem)
            {
                str += $"**Item{(Items.Count + Players.Where(x => x.CurrentItem.Type != ItemType.None).Count() <= 1 ? "" : "s")}**\n";
                if (Item)
                    foreach (ItemType item in Items)//item.itemId
                    {
                        if (!Exiled.API.Extensions.ItemExtensions.IsAmmo(item))
                            str += $"   - {item}\n";
                    }
                if (PlayerItem)
                    foreach (Player player in Players.Where(x => x.CurrentItem.Type != ItemType.None))
                    {
                        str += $"   - {player.CurrentItem.Type}\n";
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
