using Exiled.API.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace DiscordLog
{
    public class Configs : IConfig
    {
        [Description("Webhook url")]
        public string WebhookURL { get; set; } = string.Empty;

        [Description("Webhook Avatar")]
        public string WebhookAvatar { get; set; } = string.Empty;

        [Description("WebhookName")]
        public string WebhookName { get; set; } = string.Empty;

        public bool IsEnabled { get; set; } = true;
    }
}
