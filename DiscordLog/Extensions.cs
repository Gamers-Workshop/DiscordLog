using Exiled.API.Features;
using NorthwoodLib;
using NorthwoodLib.Pools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
		public static Transform NameOfGeneratorRoom(this Generator079 gen)
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

		}
	}
}