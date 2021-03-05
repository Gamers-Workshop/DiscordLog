using DiscordWebhookData;
using Exiled.API.Features;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

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
        public static async Task UpdateServerInfo(string RoundInfo, string RoundTime)
        {
            WebRequest wr = (HttpWebRequest)WebRequest.Create($"{DiscordLog.Instance.Config.WebhookSi}/messages/{DiscordLog.Instance.Config.IdMessage}");
            wr.ContentType = "application/json";
            wr.Method = "PATCH";

            using (var sw = new StreamWriter(await wr.GetRequestStreamAsync()))
            {
                string json = JsonConvert.SerializeObject(new
                {
                    username = "SCP:SL",
                    embeds = new[]
                    {
                        new
                            {
                                title = DiscordLog.Instance.Config.SIName,
                                description = "",
                                color = 14310235,
                                fields = new[]
                                {
                                new
                                {
                                    name = RoundInfo,
                                    value = RoundTime,
                                    inline = false,
                                },
                                new
                                {
                                    name = $"{(Player.List.Where((p) => p.Role != RoleType.None).ToList().Count <= 1 ? "Joueur connecté" : "Joueurs connectés")}",
                                    value = $"{Player.List.ToList().Count}/{CustomNetworkManager.slots}",
                                    inline = false,
                                },
                                },
                                footer = new
                                {
                                    icon_url = "",
                                    text = "Actualisé à",
                                },
                                timestamp = DateTime.Now,
                            },
                    }
                });
                await sw.WriteAsync(json);
            }
            wr.Abort();
        }
        public static async Task UpdateServerInfoStaffAsync(string RoundInfo, string RoundTime, string PlayerNameList, string PlayerRoleList, string UserIdList)
        {
            if (PlayerNameList != "" || PlayerRoleList != "" || UserIdList != "")
            {
                WebRequest wr = (HttpWebRequest)WebRequest.Create($"{DiscordLog.Instance.Config.WebhookSiStaff}/messages/{DiscordLog.Instance.Config.IdMessageStaff}");
                wr.ContentType = "application/json";
                wr.Method = "PATCH";
                using (var sw = new StreamWriter(await wr.GetRequestStreamAsync()))
                {
                    string json = JsonConvert.SerializeObject(new
                    {
                        username = "SCP:SL",
                        embeds = new[]
                        {
                            new
                            {
                                title = DiscordLog.Instance.Config.SIName,
                                description = "",
                                color = 14310235,
                                fields = new[]
                                {
                                    new
                                    {
                                        name = RoundInfo,
                                        value = RoundTime,
                                        inline = false,
                                    },
                                    new
                                    {
                                        name = "Pseudo",
                                        value = PlayerNameList,
                                        inline = true,
                                    },
                                    new
                                    {
                                        name = "Rôle",
                                        value = PlayerRoleList,
                                        inline = true,
                                    },
                                    new
                                    {
                                        name = "UserID",
                                        value = UserIdList,
                                        inline = true,
                                    },
                                    new
                                    {
                                        name = $"{(Player.List.Where((p) => p.Role != RoleType.None).ToList().Count <= 1 ? "Joueur connecté" : "Joueurs connectés")}",
                                        value = $"{Player.List.ToList().Count}/{CustomNetworkManager.slots}",
                                        inline = false,
                                    },
                                },
                                footer = new
                                {
                                    icon_url = "",
                                    text = "Actualisé à",
                                },
                                timestamp = DateTime.Now,
                            },
                        }
                    });
                    await sw.WriteAsync(json);
                }
                wr.Abort();
            }
            else
            {
                WebRequest wr = (HttpWebRequest)WebRequest.Create($"{DiscordLog.Instance.Config.WebhookSiStaff}/messages/{DiscordLog.Instance.Config.IdMessageStaff}");
                wr.ContentType = "application/json";
                wr.Method = "PATCH";
                using (var sw = new StreamWriter(await wr.GetRequestStreamAsync()))
                {
                    string json = JsonConvert.SerializeObject(new
                    {
                        username = "SCP:SL",
                        embeds = new[]
                        {
                        new
                            {
                                title = DiscordLog.Instance.Config.SIName,
                                description = "",
                                color = 14310235,
                                fields = new[]
                                {
                                new
                                {
                                    name = RoundInfo,
                                    value = RoundTime,
                                    inline = false,
                                },
                                new
                                {
                                    name = $"{(Player.List.Where((p) => p.Role != RoleType.None).ToList().Count <= 1 ? "Joueur connecté" : "Joueurs connectés")}",
                                    value = $"{Player.List.ToList().Count}/{CustomNetworkManager.slots}",
                                    inline = false,
                                },
                                },
                                footer = new
                                {
                                    icon_url = "",
                                    text = "Actualisé à",
                                },
                                timestamp = DateTime.Now,
                            },
                        }
                    });
                    await sw.WriteAsync(json);
                }
                wr.Abort();
            }
        }
        public static async Task BanPlayerAsync(Player player,Player sanctioned,string reason,int Duration)
        {
            WebRequest wr = (HttpWebRequest)WebRequest.Create(DiscordLog.Instance.Config.WebhookUrlLogSanction);
            wr.ContentType = "application/json";
            wr.Method = "POST";
            using (var sw = new StreamWriter(await wr.GetRequestStreamAsync()))
            {
                string json = JsonConvert.SerializeObject(new
                {
                    username = "SCP:SL",
                    embeds = new[]
                    {
                        new
                            {
                                title = DiscordLog.Instance.Config.SIName,
                                description = "",
                                color = 14310235,
                                fields = new[]
                                {
                                new
                                {
                                    name = $"BAN",
                                    value = $"``{sanctioned.Nickname}`` ({sanctioned.UserId})",
                                    inline = false,
                                },
                                new
                                {
                                    name = $"Raison",
                                    value = $"{reason}",
                                    inline = false,
                                },
                                new
                                {
                                    name = $"Temps du ban",
                                    value = $"Durée : {TimeSpan.FromSeconds(Duration).Duration()}\nUnBan : {DateTime.Now.AddSeconds(Duration)}",
                                    inline = false,
                                },
                                },
                                footer = new
                                {
                                    icon_url = "",
                                    text = $"Bannie par {player.Nickname} ({player.UserId})",
                                },
                                timestamp = DateTime.Now,
                            },
                        }
                });
                await sw.WriteAsync(json);
            }
            wr.Abort();
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
