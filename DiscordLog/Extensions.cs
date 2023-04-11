using Exiled.API.Extensions;
using Exiled.API.Features;
using Exiled.API.Features.Items;
using Exiled.API.Features.Pickups;
using Exiled.API.Structs;
using InventorySystem;
using InventorySystem.Items;
using InventorySystem.Items.Firearms;
using InventorySystem.Items.Firearms.Attachments;
using InventorySystem.Items.Firearms.Attachments.Components;
using NorthwoodLib.Pools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Firearm = Exiled.API.Features.Items.Firearm;
using FirearmPickup = Exiled.API.Features.Pickups.FirearmPickup;

namespace DiscordLog
{
    public static class Extensions
    {
        public static string LogItem(Item item) => item switch
        {
            Firearm firearm => $"{item.Type} ({item.Serial.IntToBase32()}) [{firearm.Ammo}/{firearm.MaxAmmo}]",
            MicroHid microhid => $"MicroHID ({item.Serial.IntToBase32()}) [{(int)(microhid.Energy * 100)}%]",
            Radio radio => $"Radio ({item.Serial.IntToBase32()}) [{radio.BatteryLevel}%]",
            not null => $"{item.Type} ({item.Serial.IntToBase32()})",
            _ => "Unknown"
        };
        public static string LogPickup(Pickup itemPickup) => itemPickup switch
        {
            FirearmPickup firearm => $"{itemPickup.Type} ({itemPickup.Serial.IntToBase32()}) {(firearm.IsDistributed ? $"[{itemPickup.GetMaxAmmo()}/{itemPickup.GetMaxAmmo()}]" : $"[{firearm.Status.Ammo}/{itemPickup.GetMaxAmmo()}]")}",
            MicroHIDPickup microhid => $"MicroHID ({itemPickup.Serial.IntToBase32()}) [{(int)(microhid.Energy * 100)}%]",
            RadioPickup radio => $"Radio ({itemPickup.Serial.IntToBase32()}) [{(int)(radio.BatteryLevel * 100)}%]",
            not null => $"{itemPickup.Type} ({itemPickup.Serial.IntToBase32()})",
            _ => "Unknown",
        };
        public static string LogPlayer(Player player) => player is null ? $"``Unknown`` (Unknown)" :
            $"``{player.Nickname.DiscordSanitize()}`` ({(player.DoNotTrack ? $"||{ConvertID(player.UserId)}||" : ConvertID(player.UserId))})";
        public static string ConvertID(string UserID)
        {
            if (string.IsNullOrEmpty(UserID)) return string.Empty;
            if (UserID.EndsWith("@discord"))
            {
                return $"<@{UserID.Replace("@discord", string.Empty)}>";
            }
            else if (UserID.EndsWith("@steam"))
            {
                return $"{UserID}[:link:](<https://steamidfinder.com/lookup/{UserID.Replace("@steam", string.Empty)}\"SteamFinder\">)";
            }
            return UserID;
        }
        public static string FormatArguments(IEnumerable<string> sentence, int index)
        {
            StringBuilder SB = StringBuilderPool.Shared.Rent();
            foreach (string word in sentence.Skip(index))
            {
                SB.Append(word);
                SB.Append(" ");
            }
            string msg = SB.ToString();
            StringBuilderPool.Shared.Return(SB);
            return msg;
        }
        public static string GetUserName(string userid)
        {
            try
            {
                //013D09D43A87F1D90ED3BEAA19BFCF98 -> Steam Api key to get the nickname of obanned users (Get your api key in https://steamcommunity.com/dev/apikey)
                HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create($"https://api.steampowered.com/ISteamUser/GetPlayerSummaries/v0002/?key={DiscordLog.Instance.Config.SteamAPIKey}&steamids={userid}");
                httpWebRequest.Method = "GET";

                HttpWebResponse httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();

                using StreamReader streamReader = new(httpResponse.GetResponseStream());
                string result = streamReader.ReadToEnd();
                return Regex.Match(result, @"\x22personaname\x22:\x22(.+?)\x22").Groups[1].Value;
            }
            catch (Exception)
            {
                Log.Warn("An error has occured while contacting steam servers (Are they down? Invalid API key?)");
            }

            return "Unknown (API Key Not valid)";
        }
        public static string DiscordSanitize(this string text) => Regex.Replace(text, @"(<|>|`|~~|\*|_)", m => "\\" + m.Value);

        static string IntToBase32(this ushort input)
        {
            const string CrockfordBase32Alphabet = "0123456789ABCDEFGHJKMNPQRSTVWXYZ";
            string result = string.Empty;

            do
            {
                int remainder = input % 32;
                result = CrockfordBase32Alphabet[remainder] + result;
                input /= 32;
            } while (input > 0);

            return result;
        }
        public static byte GetMaxAmmo(this ItemType item)
        {
            if (!InventoryItemLoader.AvailableItems.TryGetValue(item, out ItemBase itemBase) || itemBase is not InventorySystem.Items.Firearms.Firearm firearm)
                return 0;
            return firearm switch
            {
                AutomaticFirearm auto => auto._baseMaxAmmo,
                Shotgun shotgun => shotgun._ammoCapacity,
                ParticleDisruptor => 5,
                _ => 6,
            };
        }
        public static byte GetMaxAmmo(this Pickup pickup)
        {
            if (pickup is not FirearmPickup firearm)
                return 0;
            byte ammo = pickup.Type.GetMaxAmmo();

            if (firearm.Status.Flags.HasFlag(FirearmStatusFlags.Chambered))
                ammo++;

            return ammo += (byte)UnityEngine.Mathf.Clamp(GetAttachmentsValue(firearm, AttachmentParam.MagazineCapacityModifier), byte.MinValue, byte.MaxValue);
        }

        public static float GetAttachmentsValue(this FirearmPickup firearmPickup, AttachmentParam attachmentParam)
        {
            if (firearmPickup.Info.ItemId.GetFirearmType().GetBaseCode() > firearmPickup.Status.Attachments)
                return 0;

            IEnumerable<AttachmentIdentifier> attachements = firearmPickup.Info.ItemId.GetFirearmType().GetAttachmentIdentifiers(firearmPickup.Status.Attachments);

            AttachmentParameterDefinition definitionOfParam = AttachmentsUtils.GetDefinitionOfParam((int)attachmentParam);
            float num = definitionOfParam.DefaultValue;

            foreach (AttachmentIdentifier attachement in attachements)
            {
                Attachment attachment = AttachmentsList.FirstOrDefault(x => x.Name == attachement.Name);
                if (attachment is null || !attachment.TryGetValue((int)attachmentParam, out float paraValue))
                    continue;

                num = AttachmentsUtils.MixValue(num, paraValue, definitionOfParam.MixingMode);
            }
            return num;
        }
        private static Attachment[] attachmentsValue;
        public static Attachment[] AttachmentsList => attachmentsValue ??= UnityEngine.Object.FindObjectsOfType<Attachment>();

    }
}