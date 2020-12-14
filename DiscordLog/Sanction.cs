using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace DiscordLog
{
    public class Sanction
    {
		public string SanctionedUserId;
		public string SanctionedUserNickName;
		public string Type;
		public string Reason;
		public DateTime Time;
		public int SanctionTime;
		public DateTime UnBanTime;
		public string SanctionneurUserId;
		public string SanctionneurUserNickName;
	}
}
