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

namespace DiscordLog.Command.Suggestion
{
    [CommandHandler(typeof(GameConsoleCommandHandler))]
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    [CommandHandler(typeof(ClientCommandHandler))]

    class Suggestion : ICommand
    {
        public string Command { get; } = "Suggest";

        public string[] Aliases { get; } = new string[] { "s" };

        public string Description { get; } = "Suggest <Your Suggestion>";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            Player player = null;
            if (sender is PlayerCommandSender playerCommandSender) player = Player.Get(playerCommandSender.SenderId);

            if (string.IsNullOrWhiteSpace(Extensions.FormatArguments(arguments, 0)))
            {
                response = $"Vous devez donner une suggestion";
                return false;
            }
            _ = Webhook.SugestionAsync(player, Extensions.FormatArguments(arguments, 0));
            response = $"Votre suggestion a été envoyé : \n{Extensions.FormatArguments(arguments, 0)}";
            return true;
        }
    }
}