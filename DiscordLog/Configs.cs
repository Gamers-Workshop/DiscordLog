using Exiled.API.Interfaces;
using System.ComponentModel;

namespace DiscordLog
{
    public sealed class Configs : IConfig
    {
        public bool IsEnabled { get; set; }

        [Description("Debug Plugin")]
        public bool Debugplugin { get; set; }

        [Description("Webhook url")]
        public string WebhookUrlLogJoueur { get; set; } = string.Empty;
        [Description("Webhook url")]
        public string WebhookUrlLogSanction { get; set; } = string.Empty;

        [Description("Webhook Log STAFF (Ip of player)")]
        public string WebhookUrlLogStaff { get; set; } = string.Empty;

        [Description("Webhook Avatar")]
        public string WebhookAvatar { get; set; } = string.Empty;

        [Description("WebhookName")]
        public string WebhookName { get; set; } = string.Empty;
        [Description("Server Info Name")]
        public string SIName { get; set; } = "none";
        [Description("Webhook URL Serveur info")]
        public string WebhookSi { get; set; } = "none";
        [Description("Webhook Id Message")]
        public string IdMessage { get; set; } = "none";
        [Description("Webhook URL Serveur info Staff")]
        public string WebhookSiStaff { get; set; } = "none";
        [Description("Webhook Id Message")]
        public string IdMessageStaff { get; set; } = "none";
    }
}
