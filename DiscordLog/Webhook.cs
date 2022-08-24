using DiscordWebhookData;
using Exiled.API.Features;
using FMOD.Studio;
using Newtonsoft.Json;
using System;
using System.Globalization;
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
        public static JsonSerializerSettings SerialiseSetting = new()
        {
            NullValueHandling = NullValueHandling.Ignore,
            
            Formatting = Formatting.Indented
        };
        public static void SendWebhook(string objcontent)
        {
            HttpClient http = new();
            DiscordWebhookData.DiscordWebhook webhook = new()
            {
                AvatarUrl = DiscordLog.Instance.Config.WebhookAvatar,
                Content = objcontent,
                IsTTS = false,
                Username = DiscordLog.Instance.Config.WebhookName
            };
            string webhookstr = webhook.ToJson();
            var content = new StringContent(webhookstr, Encoding.UTF8, "application/json");
            http.PostAsync(DiscordLog.Instance.Config.WebhookUrlLogJoueur, content);
        }
        public static void SendWebhookStaff(string objcontent)
        {
            HttpClient http = new();
            DiscordWebhookData.DiscordWebhook webhook = new()
            {
                AvatarUrl = DiscordLog.Instance.Config.WebhookAvatar,
                Content = objcontent,
                IsTTS = false,
                Username = DiscordLog.Instance.Config.WebhookName
            };
            string webhookstr = webhook.ToJson();
            var content = new StringContent(webhookstr, Encoding.UTF8, "application/json");
            http.PostAsync(DiscordLog.Instance.Config.WebhookUrlLogStaff, content);
        }
        public static void SendWebhookError(string objcontent)
        {
            HttpClient http = new();
            DiscordWebhookData.DiscordWebhook webhook = new()
            {
                AvatarUrl = DiscordLog.Instance.Config.WebhookAvatar,
                Content = objcontent,
                IsTTS = false,
                Username = DiscordLog.Instance.Config.WebhookName
            };
            string webhookstr = webhook.ToJson();
            var content = new StringContent(webhookstr, Encoding.UTF8, "application/json");
            http.PostAsync(DiscordLog.Instance.Config.WebhookUrlLogError, content);
        }

        public static async Task SendWebhookInformationDisocrd(string link,string method, string json)
        {
            json = json.Replace("null,", string.Empty);

            WebRequest wr = WebRequest.Create(link);
            wr.ContentType = "application/json";
            wr.Method = method;
            using (var Request = await wr.GetRequestStreamAsync())
            {
                using var sw = new StreamWriter(Request);
                await sw.WriteAsync(json);
                sw.Dispose();
            }
            var response = await wr.GetResponseAsync();
            response.Dispose();
            wr.Abort();
        }
        public static void UpdateServerInfo(DiscordFiels PlayerConnected, DiscordFiels RoundInfo)
        {
            _ = SendWebhookInformationDisocrd(
                $"{DiscordLog.Instance.Config.WebhookSi}/messages/{DiscordLog.Instance.Config.IdMessage}",
                "PATCH",
                JsonConvert.SerializeObject(new DiscordWebhookData.DiscordWebhook()
                {
                    Username = "SCP:SL",
                    Embeds = new DiscordWebhookData.DiscordEmbed[]
                    {
                        new DiscordWebhookData.DiscordEmbed()
                        {
                            Title = DiscordLog.Instance.Config.SIName,
                            Color = 14310235,
                            Fields = new DiscordFiels[]
                            {
                                RoundInfo,
                                PlayerConnected,
                            },
                            Footer = new DiscordFooter
                            {
                                Text = "Actualisé",
                            },
                            Timestamp = DateTime.Now,
                        },
                    }
                },
                SerialiseSetting));
        }
        public static void UpdateServerInfoStaffAsync(DiscordFiels PlayerConnected, DiscordFiels RoundInfo, DiscordFiels PlayerNameList, DiscordFiels PlayerRoleList, DiscordFiels UserIdList)
        {
            _ = SendWebhookInformationDisocrd(
                $"{DiscordLog.Instance.Config.WebhookSiStaff}/messages/{DiscordLog.Instance.Config.IdMessageStaff}",
                "PATCH",
                JsonConvert.SerializeObject(
                new DiscordWebhookData.DiscordWebhook()
                {
                    Username = "SCP:SL",
                    Embeds = new DiscordWebhookData.DiscordEmbed[]
                    {
                        new DiscordWebhookData.DiscordEmbed()
                        {
                            Title = DiscordLog.Instance.Config.SIName,
                            Color = 14310235,
                            Fields = new DiscordFiels[]
                            {
                                RoundInfo,
                                PlayerNameList,
                                PlayerRoleList,
                                UserIdList,
                                PlayerConnected,
                            },
                        Footer = new DiscordFooter
                        {
                            Text = "Actualisé",
                        },
                        Timestamp = DateTime.Now,
                        },
                    },
                }, 
                SerialiseSetting));
        }
        public static async Task BanPlayerAsync(Player player, Player sanctioned, string reason, long Duration)
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
                                color = 16711680,
                                fields = new[]
                                {
                                    new
                                    {
                                        name = $"Ban",
                                        value = $"{Extensions.LogPlayer(sanctioned)}",
                                        inline = false,
                                    },
                                    new
                                    {
                                        name = $"Raison",
                                        value = $"``{reason}``",
                                        inline = false,
                                    },
                                    new
                                    {
                                        name = $"Détail sanction",
                                        value = $"Le    : <t:{DateTimeOffset.Now.ToUnixTimeSeconds()}>\n" +
                                                $"Durée : {(Duration < 31536000 ? TimeSpan.FromSeconds(Duration).ToString("%d'd. '%h'h. '%m'min.'") : $"{Duration/31536000} ans")}\n" +
                                                $"Unban : <t:{DateTimeOffset.Now.AddSeconds(Duration).ToUnixTimeSeconds()}> -> <t:{DateTimeOffset.Now.AddSeconds(Duration).ToUnixTimeSeconds()}:R>",
                                        inline = false,
                                    },
                                },
                                footer = new
                                {
                                    icon_url = "",
                                    text = $"Ban par {player.Nickname} ({player.UserId})",
                                },
                                timestamp = DateTime.Now,
                            },
                    }
                });
                await sw.WriteAsync(json);
            }
            var response = (HttpWebResponse)await wr.GetResponseAsync();
            wr.Abort();
        }
        public static async Task OBanPlayerAsync(Player player, string sanctionedNickname, string sanctionedUserId, string reason, long Duration)
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
                                color = 16711680,
                                fields = new[]
                                {
                                    new
                                    {
                                        name = $"Oban",
                                        value = $"``{sanctionedNickname}``({Extensions.ConvertID(sanctionedUserId)})",
                                        inline = false,
                                    },
                                    new
                                    {
                                        name = $"Raison",
                                        value = $"``{reason}``",
                                        inline = false,
                                    },
                                    new
                                    {
                                        name = $"Détail sanction",
                                        value = $"Le    : <t:{DateTimeOffset.Now.ToUnixTimeSeconds()}>\n" +
                                                $"Durée : {(Duration < 31536000 ? TimeSpan.FromSeconds(Duration).ToString("%d'd. '%h'h. '%m'min.'") : $"{Duration/31536000} ans")}\n" +
                                                $"Unban : <t:{DateTimeOffset.Now.AddSeconds(Duration).ToUnixTimeSeconds()}> -> <t:{DateTimeOffset.Now.AddSeconds(Duration).ToUnixTimeSeconds()}:R>",
                                        inline = false,
                                    }
                                },
                                footer = new
                                {
                                    icon_url = "",
                                    text = $"Oban par {player.Nickname} ({player.UserId})",
                                },
                                timestamp = DateTime.Now,
                            },
                    }
                });
                await sw.WriteAsync(json);
            }
            var response = (HttpWebResponse)await wr.GetResponseAsync();
            wr.Abort();
        }
        public static async Task KickPlayerAsync(Player player, Player sanctioned, string reason)
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
                                        name = $"Kick",
                                        value = $"{Extensions.LogPlayer(sanctioned)}",
                                        inline = false,
                                    },
                                    new
                                    {
                                        name = $"Raison",
                                        value = $"``{reason}``",
                                        inline = false,
                                    },
                                },
                                footer = new
                                {
                                    icon_url = "",
                                    text = $"kick par {player.Nickname} ({player.UserId})",
                                },
                                timestamp = DateTime.Now,
                            },
                    }
                });
                await sw.WriteAsync(json);
            }
            var response = (HttpWebResponse)await wr.GetResponseAsync();
            wr.Abort();
        }
        public static async Task WarnPlayerAsync(Player player, Player sanctioned, string reason)
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
                                color = 16773376,
                                fields = new[]
                                {
                                    new
                                    {
                                        name = $"Warn",
                                        value = $"{Extensions.LogPlayer(sanctioned)}",
                                        inline = false,
                                    },
                                    new
                                    {
                                        name = $"Raison",
                                        value = $"``{reason}``",
                                        inline = false,
                                    },
                                },
                                footer = new
                                {
                                    icon_url = "",
                                    text = $"Warn par {player.Nickname} ({player.UserId})",
                                },
                                timestamp = DateTime.Now,
                            },
                    }
                });
                await sw.WriteAsync(json);
            }
            var response = (HttpWebResponse)await wr.GetResponseAsync();
            wr.Abort();
        }
        public static async Task OwarnPlayerAsync(Player player, string sanctionedNickname, string sanctionedUserId, string reason)
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
                                color = 16773376,
                                fields = new[]
                                {
                                    new
                                    {
                                        name = $"Warn",
                                        value = $"``{sanctionedNickname}`` ({Extensions.ConvertID(sanctionedUserId)})",
                                        inline = false,
                                    },
                                    new
                                    {
                                        name = $"Raison",
                                        value = $"``{reason}``",
                                        inline = false,
                                    },
                                },
                                footer = new
                                {
                                    icon_url = "",
                                    text = $"Warn par {player.Nickname} ({player.UserId})",
                                },
                                timestamp = DateTime.Now,
                            },
                    }
                });
                await sw.WriteAsync(json);
            }
            var response = (HttpWebResponse)await wr.GetResponseAsync();
            wr.Abort();
        }
        public static async Task BugInfoAsync(Player player, string Info)
        {
            WebRequest wr = (HttpWebRequest)WebRequest.Create(DiscordLog.Instance.Config.WebhookUrlLogBug);
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
                                color = 16773376,
                                fields = new[]
                                {
                                    new
                                    {
                                        name = $"Bug",
                                        value = $"{Extensions.LogPlayer(player)}",
                                        inline = false,
                                    },
                                    new
                                    {
                                        name = $"Info",
                                        value = $"``{Info}``",
                                        inline = false,
                                    },
                                },
                                footer = new
                                {
                                    icon_url = "",
                                    text = $"Signalé par {player.Nickname} ({player.UserId})",
                                },
                                timestamp = DateTime.Now,
                            },
                    }
                });
                await sw.WriteAsync(json);
            }
            var response = (HttpWebResponse)await wr.GetResponseAsync();
            wr.Abort();
        }
        public static async Task SugestionAsync(Player player, string Info)
        {
            WebRequest wr = (HttpWebRequest)WebRequest.Create(DiscordLog.Instance.Config.WebhookUrlLogSuggestion);
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
                                color = 16773376,
                                fields = new[]
                                {
                                    new
                                    {
                                        name = $"Sugestion",
                                        value = $"{Extensions.LogPlayer(player)}",
                                        inline = false,
                                    },
                                    new
                                    {
                                        name = $"Info",
                                        value = $"``{Info}``",
                                        inline = false,
                                    },
                                },
                                footer = new
                                {
                                    icon_url = "",
                                    text = $"Sugesté par {player.Nickname} ({player.UserId})",
                                },
                                timestamp = DateTime.Now,
                            },
                    }
                });
                await sw.WriteAsync(json);
            }
            var response = (HttpWebResponse)await wr.GetResponseAsync();
            wr.Abort();
        }
        public static async Task ReportAsync(Player Reporter, Player Reported, string WebhookUrl,string pings, string reason = "Aucune raison donnée")
        {
            WebRequest wr = (HttpWebRequest)WebRequest.Create(WebhookUrl);
            wr.ContentType = "application/json";
            wr.Method = "POST";
            using (var sw = new StreamWriter(await wr.GetRequestStreamAsync()))
            {
                string json = JsonConvert.SerializeObject(new
                {
                    username = "SCP:SL",
                    content = pings,
                    embeds = new[]
                    {
                        new
                            {
                                title = DiscordLog.Instance.Config.SIName,
                                description = "",
                                color = 16773376,
                                fields = new[]
                                {
                                    new
                                    {
                                        name = $"Report",
                                        value = $"[{Reported.Id}] {Extensions.LogPlayer(Reported)}",
                                        inline = false,
                                    },
                                    new
                                    {
                                        name = $"Reason",
                                        value = $"``{reason}``",
                                        inline = false,
                                    },
                                },
                                footer = new
                                {
                                    icon_url = "",
                                    text = $"Report par [{Reporter.Id}] {Reporter.Nickname} ({Reporter.UserId})",
                                },
                                timestamp = DateTime.Now,
                            },
                    }
                });
                await sw.WriteAsync(json);
            }
            var response = (HttpWebResponse)await wr.GetResponseAsync();
            wr.Abort();
        }
    }
}
