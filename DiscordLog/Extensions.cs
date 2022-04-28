using Exiled.API.Features;
using Exiled.API.Features.Items;
using InventorySystem.Items.Firearms;
using InventorySystem.Items.MicroHID;
using InventorySystem.Items.Radio;
using NorthwoodLib.Pools;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Firearm = Exiled.API.Features.Items.Firearm;

namespace DiscordLog
{
    public static class Extensions
    {
        public static string LogItem(Item item) => item switch
        {
            Firearm firearm => $"{item.Type} [{firearm.Ammo}/{firearm.MaxAmmo}]",
            MicroHid microhid => $"MicroHID [{(int)(microhid.Energy * 100)}%]",
            Exiled.API.Features.Items.Radio radio => $"Radio [{radio.BatteryLevel}%]",
            not null => $"{item.Type}",
            _ => "Unknown"
        };
        public static string LogPickup(Pickup itemPickup) => itemPickup?.Base switch
        {
            FirearmPickup firearm => $"{itemPickup.Type} [{firearm.Status.Ammo}]",
            MicroHIDPickup microhid => $"MicroHID [{(int)(microhid.Energy * 100)}%]",
            RadioPickup radio => $"Radio [{(int)(radio.SavedBattery * 100)}%]",
            not null => $"{itemPickup.Type}",
            _ => "Unknown",
        };
        public static string LogPlayer(Player player) => player is null ? $"``Unknown`` (Unknown)" : $"``{player.Nickname}`` ({ConvertID(player.UserId)})";
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
        public static bool IsEnemy(this Player player, Team target)
        {
            if (player.Role == RoleType.Spectator || player.Role == RoleType.None || player.Role.Team == target)
                return false;

            return target == Team.SCP || target == Team.TUT ||
                ((player.Role.Team != Team.MTF && player.Role.Team != Team.RSC) || (target != Team.MTF && target != Team.RSC))
                &&
                ((player.Role.Team != Team.CDP && player.Role.Team != Team.CHI) || (target != Team.CDP && target != Team.CHI))
            ;
        }
        public static string FormatArguments(ArraySegment<string> sentence, int index)
        {
            StringBuilder SB = StringBuilderPool.Shared.Rent();
            foreach (string word in sentence.Segment(index))
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
                Log.Error("An error has occured while contacting steam servers (Are they down? Invalid API key?)");
            }

            return "Unknown (API Key Not valid)";
        }
    }
}