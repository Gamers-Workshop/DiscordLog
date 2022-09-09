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

namespace DiscordLog.Command.Bug
{
    [CommandHandler(typeof(GameConsoleCommandHandler))]
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    [CommandHandler(typeof(ClientCommandHandler))]

    class Bug : ICommand
    {
        public string Command { get; } = "bug";

        public string[] Aliases { get; } = new string[] {  };

        public string Description { get; } = "bug <Le bug que vous avez rencontrez>";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            Player player = null;
            if (sender is PlayerCommandSender playerCommandSender) player = Player.Get(playerCommandSender.SenderId);

            if (string.IsNullOrWhiteSpace(Extensions.FormatArguments(arguments, 0)))
            {
                response = $"Vous devez donner signaler un bug";
                return false;
            }
            Webhook.BugInfoAsync(player, Extensions.FormatArguments(arguments, 0));
            response = $"Votre bug a été envoyé : \n{Extensions.FormatArguments(arguments, 0)}";
            return true;
        }
    }
}