using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using RemoteAdmin;
using System;
using System.Linq;

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
            if (!sender.CheckPermission("log.warn"))
            {
                response = "Permission denied.";
                return false;
            }
            Player Sanctioned = Player.Get(arguments.ElementAtOrDefault(0));
            if (Sanctioned is null)
            {
                response = $"Player not found: {arguments.ElementAtOrDefault(0)}";
                return false;
            }
            if (string.IsNullOrWhiteSpace(Extensions.FormatArguments(arguments, 1)))
            {
                response = $"Vous devez donner une raison a votre warn";
                return false;
            }
            if (!Player.TryGet(sender, out Player player)) player = Server.Host;

            if (DiscordLog.Instance.Config.WarnBox)
                Sanctioned.OpenReportWindow($"Vous avez été warn par {player.Nickname} car {Extensions.FormatArguments(arguments, 1)}\n<b>Appuyez sur Esc pour fermer</b>");

            Webhook.WarnPlayerAsync(player, Sanctioned, Extensions.FormatArguments(arguments, 1));
            response = $"Player {Sanctioned.Nickname} has been warned : {Extensions.FormatArguments(arguments, 1)}";
            return true;
        }
	}
}