using Exiled.API.Features;
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
	}
}
