using Exiled.API.Features;
using NorthwoodLib;
using NorthwoodLib.Pools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;

namespace DiscordLog
{
	internal static class Extensions
	{
		public static bool IsEnemy(this Player player, Team target)
		{
			if (player.Role == RoleType.Spectator || player.Role == RoleType.None || player.Team == target)
				return false;

			return target == Team.SCP || target == Team.TUT ||
				((player.Team != Team.MTF && player.Team != Team.RSC) || (target != Team.MTF && target != Team.RSC))
				&&
				((player.Team != Team.CDP && player.Team != Team.CHI) || (target != Team.CDP && target != Team.CHI))
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
		public static void OpenReportWindow(this Player player, string text)
		{
			player.ReferenceHub.GetComponent<GameConsoleTransmission>().SendToClient(player.Connection, "[REPORTING] " + text, "white");
		}
		public static string GetUserName(string userid)
		{
			try
			{
				//013D09D43A87F1D90ED3BEAA19BFCF98 -> Steam Api key to get the nickname of obanned users (Get your api key in https://steamcommunity.com/dev/apikey)
				var httpWebRequest = (HttpWebRequest)WebRequest.Create($"https://api.steampowered.com/ISteamUser/GetPlayerSummaries/v0002/?key=013D09D43A87F1D90ED3BEAA19BFCF98&steamids={userid}");
				httpWebRequest.Method = "GET";

				var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();

				using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
				{
					var result = streamReader.ReadToEnd();
					return Regex.Match(result, @"\x22personaname\x22:\x22(.+?)\x22").Groups[1].Value;
				}
			}
			catch (Exception)
			{
				Log.Error("An error has occured while contacting steam servers (Are they down? Invalid API key?)");
			}

			return "Unknown";
		}
		/*public static Transform NameOfGeneratorRoom(this Generator079 gen)
        {
			Transform transform = gen.transform;
			Physics.Raycast(new Ray(transform.position - transform.forward, Vector3.up), out RaycastHit raycastHit, 5f, global::Interface079.singleton.roomDetectionMask);
			Transform transform2 = raycastHit.transform;
			if (!transform2)
			{
				RaycastHit raycastHit2;
				Physics.Raycast(new Ray(transform.position - transform.forward, Vector3.down), out raycastHit2, 5f, global::Interface079.singleton.roomDetectionMask);
				transform2 = raycastHit2.transform;
			}
			if (transform2)
			{
				while (transform2 != null && !transform2.transform.name.Contains("ROOT", StringComparison.OrdinalIgnoreCase) && !transform2.gameObject.CompareTag("Room"))
				{
					transform2 = transform2.transform.parent;
				}
			}
			return transform2;

		}*/
	}
}