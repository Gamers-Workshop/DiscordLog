using CommandSystem;
using Exiled.API.Extensions;
using Exiled.API.Features;
using RemoteAdmin;
using System;
using System.Linq;

namespace DiscordLog.Command.Suggestion
{
    [CommandHandler(typeof(GameConsoleCommandHandler))]
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    [CommandHandler(typeof(ClientCommandHandler))]

    class Suggestion : ICommand
    {
        public string Command { get; } = "Suggest";

        public string[] Aliases { get; } = new string[] { "s" };

        public string Description { get; } = "Suggest <> , <Your Suggestion>";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (string.IsNullOrWhiteSpace(Extensions.FormatArguments(arguments, 0)))
            {
                response = $"Vous devez donner une suggestion";
                return false;
            }
            response = $"Votre suggestion a été envoyé : \n{Extensions.FormatArguments(arguments, 0)}";

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
            Webhook.SugestionAsync(player, title, text.Remove(0, title.Count()));

            return true;
        }
    }
}