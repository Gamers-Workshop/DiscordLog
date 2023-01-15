using CommandSystem;
using Exiled.API.Extensions;
using Exiled.API.Features;
using RemoteAdmin;
using System;
using System.Linq;

namespace DiscordLog.Command.Bug
{
    [CommandHandler(typeof(GameConsoleCommandHandler))]
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    [CommandHandler(typeof(ClientCommandHandler))]

    class Bug : ICommand
    {
        public string Command { get; } = "bug";

        public string[] Aliases { get; } = new string[] {  };

        public string Description { get; } = "bug <Titre de votre bug>,<Le bug que vous avez rencontrez>";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (string.IsNullOrWhiteSpace(Extensions.FormatArguments(arguments, 0)))
            {
                response = $"Vous devez donner signaler un bug";
                return false;
            }
            string text = Extensions.FormatArguments(arguments, 0);
            if (!text.Contains(','))
            {
                response = $"Vous devez utilisé une virgule (,) pour donné un titre";
                return false;
            }
            string title = text.GetBefore(',');
            if (title.Length > 50)
            {
                response = $"réduisé la contité de caractére pour le titre";
                return false;
            }

            if (!Player.TryGet(sender, out Player player)) player = Server.Host;
            Webhook.BugInfoAsync(player, title, text.Remove(0, title.Count()));
            response = $"Votre bug a été envoyé : \n{Extensions.FormatArguments(arguments, 0)}";
            return true;
        }
    }
}