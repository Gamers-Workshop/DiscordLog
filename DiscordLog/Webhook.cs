using DiscordWebhookData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace DiscordLog
{
    class Webhook
    {
        public static void SendWebhook(string objcontent)
        {
            HttpClient http = new HttpClient();
            DiscordWebhookData.DiscordWebhook webhook = new DiscordWebhookData.DiscordWebhook
            {
                AvatarUrl = DiscordLog.Instance.Config.WebhookAvatar,
                Content = objcontent,
                IsTTS = false,
                Username = DiscordLog.Instance.Config.WebhookName
            };
            string webhookstr = webhook.ToJson();
            Console.WriteLine(webhookstr);
            var content = new StringContent(webhookstr, Encoding.UTF8, "application/json");
            http.PostAsync(DiscordLog.Instance.Config.WebhookUrlLogJoueur, content);
        }

        public static void SendWebhook(string objcontent, params object[] objects)
        {
            objcontent = string.Format(objcontent, objects);
            HttpClient http = new HttpClient();
            DiscordWebhookData.DiscordWebhook webhook = new DiscordWebhookData.DiscordWebhook
            {
                AvatarUrl = DiscordLog.Instance.Config.WebhookAvatar,
                Content = objcontent,
                IsTTS = false,
                Username = DiscordLog.Instance.Config.WebhookName
            };
            string webhookstr = webhook.ToJson();
            Console.WriteLine(webhookstr);
            var content = new StringContent(webhookstr, Encoding.UTF8, "application/json");
            http.PostAsync(DiscordLog.Instance.Config.WebhookUrlLogJoueur, content);
        }
        public static void SendWebhookStaff(string objcontent)
        {
            HttpClient http = new HttpClient();
            DiscordWebhookData.DiscordWebhook webhook = new DiscordWebhookData.DiscordWebhook
            {
                AvatarUrl = DiscordLog.Instance.Config.WebhookAvatar,
                Content = objcontent,
                IsTTS = false,
                Username = DiscordLog.Instance.Config.WebhookName
            };
            string webhookstr = webhook.ToJson();
            Console.WriteLine(webhookstr);
            var content = new StringContent(webhookstr, Encoding.UTF8, "application/json");
            http.PostAsync(DiscordLog.Instance.Config.WebhookUrlLogStaff, content);
        }
    }
}
