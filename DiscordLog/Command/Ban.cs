using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using MEC;
using RemoteAdmin;
using System;
using System.Collections.Generic;

/*namespace DiscordLog.Command
{
    [CommandHandler(typeof(GameConsoleCommandHandler))]
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    class Commands : ICommand
    {
        public string Command { get; } = "ban";

        public string[] Aliases { get; } = new string[] { "ban" };

        public string Description { get; } = "ban <player> <reason>";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            Player sanctionneur = null;
            if (sender is PlayerCommandSender playerCommandSender) sanctionneur = Player.Get(playerCommandSender.SenderId);
            if (sanctionneur != null && !sanctionneur.CheckPermission("at.ban"))
            {
                response = "Permission denied.";
                return false;
            }
            string[] SanctionedPlayerList = arguments.At(0).Split('.');
            foreach (string SanctionedPlayer in SanctionedPlayerList)
            {
                Player Sanctioned = Player.Get(SanctionedPlayer);
                if (Sanctioned == null)
                {
                    response = $"Player not found: {SanctionedPlayer}";
                    return false;
                }
                if (string.IsNullOrEmpty(arguments.At(2)))
                {
                    response = $"Vous devez donner une raison a votre warn";
                    return false;
                }
                Timing.RunCoroutine(EventHandlers.DoSanction(Sanctioned, sanctionneur, Extensions.FormatArguments(arguments, 3),arguments.At(1) == "0" ? "Kick": "Ban", 0));
            }
            response = $"Player {SanctionedPlayerList} has been {(arguments.At(1) == "0" ? "Kick": "Ban")} : {Extensions.FormatArguments(arguments, 3)}";
            return true;
        }
    }
}
*/