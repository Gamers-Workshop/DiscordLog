using DiscordWebhookData;
using Exiled.API.Features;
using MEC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Utf8Json;
using Utf8Json.Formatters;

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
        public static void UpdateServerInfo(string RoundInfo, string RoundTime)
        {
            try
            {
                HttpClient http = new HttpClient();
                string payload = JsonSerializer.ToJsonString(new DiscordWebhook(
                    null, "SCP:SL", null, false, 
                    new DiscordEmbed[1]
                    {
                        new DiscordEmbed(DiscordLog.Instance.Config.SIName, "rich", null,
                            14310235, new DiscordEmbedField[2]
                            {
                                new DiscordEmbedField(RoundInfo, RoundTime, false),
                                new DiscordEmbedField($"{(Player.List.Where((p) => p.Role != RoleType.None).ToList().Count <= 1 ? "Joueur connecté" : "Joueurs connectés")}", $"{Player.List.Where((p) => p.Role != RoleType.None).ToList().Count}/{CustomNetworkManager.slots}", false),
                            })
                    }));
                var content = new StringContent(payload, Encoding.UTF8, "application/json");
                http.PatchAsync($"{DiscordLog.Instance.Config.WebhookSi}/messages/{DiscordLog.Instance.Config.IdMessage}", content).GetAwaiter();
            }
            catch (Exception ex)
            {
                ServerConsole.AddLog("Failed to send The TEST by webhook: \n" + ex.Message, ConsoleColor.Red);
                Log.Error(ex);
            }
        }
        public static void UpdateServerInfoStaff(string RoundInfo, string RoundTime, string PlayerNameList, string PlayerRoleList, string UserIdList)
        {
            if (PlayerNameList != "" || PlayerRoleList != "" || UserIdList != "")
            {
                HttpClient http = new HttpClient();
                string payload = JsonSerializer.ToJsonString(new DiscordWebhook(
                    null, "SCP:SL", null, false,
                    new DiscordEmbed[1]
                    {
                        new DiscordEmbed(DiscordLog.Instance.Config.SIName, "rich", null,
                            14310235, new DiscordEmbedField[5]
                            {
                                new DiscordEmbedField(RoundInfo, RoundTime, false),
                                new DiscordEmbedField("Pseudo", PlayerNameList, true),
                                new DiscordEmbedField("Rôle", PlayerRoleList, true),
                                new DiscordEmbedField("UserId", UserIdList, true),
                                new DiscordEmbedField($"{(Player.List.Where((p) => p.Role != RoleType.None).ToList().Count <= 1 ? "Joueur connecté" : "Joueurs connectés")}", $"{Player.List.Where((p) => p.Role != RoleType.None).ToList().Count}/{CustomNetworkManager.slots}", false),
                            })
                    }));
                var content = new StringContent(payload, Encoding.UTF8, "application/json");
                http.PatchAsync($"{DiscordLog.Instance.Config.WebhookSiStaff}/messages/{DiscordLog.Instance.Config.IdMessageStaff}", content).GetAwaiter();
            }
            else
            {
                HttpClient http = new HttpClient();
                string payload = JsonSerializer.ToJsonString(new DiscordWebhook(
                    null, "SCP:SL", null, false,
                    new DiscordEmbed[1]
                    {
                        new DiscordEmbed(DiscordLog.Instance.Config.SIName, "rich", null,
                            14310235, new DiscordEmbedField[2]
                            {
                                new DiscordEmbedField(RoundInfo, RoundTime, false),
                                new DiscordEmbedField($"{(Player.List.ToList().Count <= 1 ? "Joueur connecté" : "Joueurs connectés")}", $"{Player.List.ToList().Count}/{CustomNetworkManager.slots}", false),
                            })
                    }));
                var content = new StringContent(payload, Encoding.UTF8, "application/json");
                http.PatchAsync($"{DiscordLog.Instance.Config.WebhookSiStaff}/messages/{DiscordLog.Instance.Config.IdMessageStaff}", content).GetAwaiter();
            }
        }
    }
    public static class HttpPatchClientExtensions
    {
        public static async Task<HttpResponseMessage> PatchAsync(this HttpClient client, string requestUri, HttpContent content)
        {
            var method = new HttpMethod("PATCH");
            var request = new HttpRequestMessage(method, requestUri)
            {
                Content = content
            };
            return await client.SendAsync(request, HttpCompletionOption.ResponseContentRead);
        }
    }
}
