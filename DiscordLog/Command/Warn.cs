using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using MEC;
using RemoteAdmin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordLog.Command.Warn
{
	[CommandHandler(typeof(GameConsoleCommandHandler))]
	[CommandHandler(typeof(RemoteAdminCommandHandler))]
	class Warn : ICommand
	{
		public string Command { get; } = "warn";

		public string[] Aliases { get; } = new string[] { "w" };

		public string Description { get; } = "warn <player> <reason>";

		public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
		{
            Player sanctionneur = null;
            if (sender is PlayerCommandSender playerCommandSender) sanctionneur = Player.Get(playerCommandSender.SenderId);
            if (sanctionneur != null && !sanctionneur.CheckPermission("log.warn"))
            {
                response = "Permission denied.";
                return false;
            }
            Player Sanctioned = Player.Get(arguments.At(0));
            if (Sanctioned == null)
            {
                response = $"Player not found: {arguments.At(0)}";
                return false;
            }
            if (string.IsNullOrWhiteSpace(Extensions.FormatArguments(arguments, 1)))
            {
                response = $"Vous devez donner une raison a votre warn";
                return false;
            }
            Webhook.WarnPlayerAsync(Sanctioned, sanctionneur, Extensions.FormatArguments(arguments, 1));
            response = $"Player {Sanctioned.Nickname} has been warned : {Extensions.FormatArguments(arguments, 1)}";
            return true;
        }
	}
}