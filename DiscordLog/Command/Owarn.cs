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
    class Owarn : ICommand
    {
        public string Command { get; } = "owarn";

        public string[] Aliases { get; } = new string[] { "ow" };

        public string Description { get; } = "owarn <id> <reason>";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!sender.CheckPermission("log.warn"))
            {
                response = "Permission denied.";
                return false;
            }

            string SanctionedNickname = "Unknown";
            string UserId = arguments.ElementAtOrDefault(0);
            if (!UserId.Contains("@"))
            {
                response = "You need to put a UserId like '145655965262@steam' .";
                return false;
            }
            if (UserId.EndsWith("@steam"))
            {
                SanctionedNickname = "Unknown (API Key Not valid)";
                try
                {
                    SanctionedNickname = Extensions.GetUserName(UserId.Substring(0,UserId.IndexOf('@')));
                }
                catch (Exception ex)
                {
                    Log.Warn($"API key is not valide {ex}");
                }
            }

            if (string.IsNullOrWhiteSpace(Extensions.FormatArguments(arguments, 1)))
            {
                response = $"Vous devez donner une raison a votre warn";
                return false;
            }
            if (!Player.TryGet(sender, out Player player)) player = Server.Host;
            Webhook.OWarnPlayerAsync(player, SanctionedNickname, arguments.ElementAtOrDefault(0), Extensions.FormatArguments(arguments, 1));
            response = $"Player {arguments.ElementAtOrDefault(0)} Pseudo = {SanctionedNickname} has been warned : {Extensions.FormatArguments(arguments, 1)}";
            return true;
        }
    }
}