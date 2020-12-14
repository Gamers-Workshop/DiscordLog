using Exiled.API.Features;
using NorthwoodLib.Pools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
	}
}
