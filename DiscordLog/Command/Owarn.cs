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
    class Owarn : ICommand
    {
        public string Command { get; } = "owarn";

        public string[] Aliases { get; } = new string[] { "ow" };

        public string Description { get; } = "owarn <id> <reason>";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            Player sanctionneur = null;
            if (sender is PlayerCommandSender playerCommandSender) sanctionneur = Player.Get(playerCommandSender.SenderId);
            if (sanctionneur is not null && !sanctionneur.CheckPermission("log.warn"))
            {
                response = "Permission denied.";
                return false;
            }
            string SanctionedNickname = "Unknown";
            if (arguments.At(0).EndsWith("@steam"))
            {
                SanctionedNickname = "Unknown (API Key Not valid)";
                try
                {
                    SanctionedNickname = Extensions.GetUserName(arguments.At(0));
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

            _ = Webhook.OwarnPlayerAsync(sanctionneur, SanctionedNickname, arguments.At(0), Extensions.FormatArguments(arguments, 1));
            response = $"Player {arguments.At(0)} Pseudo = {SanctionedNickname} has been warned : {Extensions.FormatArguments(arguments, 1)}";
            return true;
        }
    }
}