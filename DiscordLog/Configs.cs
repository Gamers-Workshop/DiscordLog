using Exiled.API.Interfaces;
using System.ComponentModel;

namespace DiscordLog
{
    public sealed class Configs : IConfig
    {
        public bool IsEnabled { get; set; }

        [Description("Webhook url")]
        public bool Debugplugin { get; set; }

        [Description("Webhook url")]
        public string WebhookUrlLogJoueur { get; set; } = string.Empty;

        [Description("Webhook url")]
        public string WebhookUrlLogStaff { get; set; } = string.Empty;

        [Description("Webhook Avatar")]
        public string WebhookAvatar { get; set; } = string.Empty;

        [Description("WebhookName")]
        public string WebhookName { get; set; } = string.Empty;
    }
}
