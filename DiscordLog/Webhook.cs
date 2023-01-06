using DiscordWebhookData;
using Exiled.API.Features;
using MEC;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Web.Routing;
using System.Web.UI.WebControls;
using UnityEngine;
using UnityEngine.Networking;

namespace DiscordLog
{
    class Webhook
    {
        public static Dictionary<string, DateTime> DiscordLimitter = new();
        public static HttpClient http = new();
        public static JsonSerializerSettings SerialiseSetting = new()
        {
            NullValueHandling = NullValueHandling.Ignore,
        };
        public static void SendWebhookMessage(string url, string objcontent, bool islogged = true)
        {
            EventHandlers.Coroutines.Add(Timing.RunCoroutine(SendWebhookInformationDiscord(url, "POST", JsonConvert.SerializeObject(new DiscordWebhookData.DiscordWebhook()
            {
                AvatarUrl = DiscordLog.Instance.Config.WebhookAvatar,
                Content = objcontent,
                IsTTS = false,
                Username = DiscordLog.Instance.Config.WebhookName,
            }), islogged)));
        }

        public static IEnumerator<float> SendWebhookInformationDiscord(string link, string method, string json, bool islogged = true)
        {
            if (DiscordLimitter.TryGetValue(link, out DateTime dateTime))
            {
                float time = (float)(DateTime.Now - dateTime).TotalSeconds;
                EventHandlers.Coroutines.Add(Timing.CallDelayed(time / 2 + time, () =>
                EventHandlers.Coroutines.Add(Timing.RunCoroutine(SendWebhookInformationDiscord(link, method, json)))));
            }
            using UnityWebRequest discordWWW = UnityWebRequest.Put(link, json);
            discordWWW.method = method;
            discordWWW.SetRequestHeader("Content-Type", "application/json");
            yield return Timing.WaitUntilDone(discordWWW.SendWebRequest());
            if (discordWWW.isHttpError || discordWWW.isNetworkError)
            {
                Log.Warn(
                $"link {link}\n" +
                $"Error when attempting to send report to discord log: {discordWWW.error}\n" +
                $"StatusCode: {(HttpStatusCode)discordWWW.responseCode}\n" +
                $"content: \n {json}");
            }
        }
        public static void UpdateServerInfo(DiscordFiels PlayerConnected, DiscordFiels RoundInfo)
        {
            EventHandlers.Coroutines.Add(
                Timing.RunCoroutine(
                SendWebhookInformationDiscord(
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
                SerialiseSetting))));
        }
        public static void UpdateServerInfoStaffAsync(DiscordFiels PlayerConnected, DiscordFiels RoundInfo, DiscordFiels PlayerNameList, DiscordFiels PlayerRoleList, DiscordFiels UserIdList)
        {
            EventHandlers.Coroutines.Add(
                Timing.RunCoroutine(
                SendWebhookInformationDiscord(
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
                SerialiseSetting).Replace("null,", string.Empty))));
        }
        public static void BanPlayerAsync(Player player, Player sanctioned, string reason, long Duration)
        {
            EventHandlers.Coroutines.Add(
                Timing.RunCoroutine(
                SendWebhookInformationDiscord(
                DiscordLog.Instance.Config.WebhookUrlLogSanction,
                "POST",
                JsonConvert.SerializeObject(
                    new DiscordWebhookData.DiscordWebhook()
                    {
                        Username = "SCP:SL",
                        Embeds = new DiscordWebhookData.DiscordEmbed[]
                        {
                            new DiscordWebhookData.DiscordEmbed()
                            {
                                Title = DiscordLog.Instance.Config.SIName,
                                Description = "",
                                Color = 16711680,
                                Fields = new DiscordFiels[]
                                {
                                    new DiscordFiels
                                    {
                                        Name = "Ban",
                                        Value = $"{Extensions.LogPlayer(sanctioned)}",
                                    },
                                    new DiscordFiels
                                    {
                                        Name = $"Raison",
                                        Value = $"``{reason}``",
                                    },
                                    new DiscordFiels
                                    {
                                        Name = $"Détail sanction",
                                        Value = $"Le    : <t:{DateTimeOffset.Now.ToUnixTimeSeconds()}>\n" +
                                                $"Durée : {(Duration < 31536000 ? TimeSpan.FromSeconds(Duration).ToString("%d'd. '%h'h. '%m'min.'") : $"{Duration/31536000} ans")}\n" +
                                                $"Unban : <t:{DateTimeOffset.Now.AddSeconds(Duration).ToUnixTimeSeconds()}> -> <t:{DateTimeOffset.Now.AddSeconds(Duration).ToUnixTimeSeconds()}:R>",
                                    },
                                },
                                Footer = new DiscordFooter
                                {
                                    IconUrl = "",
                                    Text = $"Ban par {player.Nickname} ({player.UserId})",
                                },
                                Timestamp = DateTime.Now,
                            },
                        }
                    },SerialiseSetting))));
        }
        public static void OBanPlayerAsync(Player player, string sanctionedNickname, string sanctionedUserId, string reason, long Duration)
        {
            EventHandlers.Coroutines.Add(
                Timing.RunCoroutine(
                SendWebhookInformationDiscord(
                DiscordLog.Instance.Config.WebhookUrlLogSanction,
                "POST",
                JsonConvert.SerializeObject(
                    new DiscordWebhookData.DiscordWebhook()
                    {
                        Username = "SCP:SL",
                        Embeds = new DiscordWebhookData.DiscordEmbed[]
                        {
                            new DiscordWebhookData.DiscordEmbed()
                            {
                                Title = DiscordLog.Instance.Config.SIName,
                                Description = "",
                                Color = 16711680,
                                Fields = new DiscordFiels[]
                                {
                                    new DiscordFiels
                                    {
                                        Name = "OBan",
                                        Value = $"{sanctionedNickname}({sanctionedUserId})",
                                    },
                                    new DiscordFiels
                                    {
                                        Name = $"Raison",
                                        Value = $"``{reason}``",
                                    },
                                    new DiscordFiels
                                    {
                                        Name = $"Détail sanction",
                                        Value = $"Le    : <t:{DateTimeOffset.Now.ToUnixTimeSeconds()}>\n" +
                                                $"Durée : {(Duration < 31536000 ? TimeSpan.FromSeconds(Duration).ToString("%d'd. '%h'h. '%m'min.'") : $"{Duration/31536000} ans")}\n" +
                                                $"Unban : <t:{DateTimeOffset.Now.AddSeconds(Duration).ToUnixTimeSeconds()}> -> <t:{DateTimeOffset.Now.AddSeconds(Duration).ToUnixTimeSeconds()}:R>",
                                    },
                                },
                                Footer = new DiscordFooter
                                {
                                    IconUrl = "",
                                    Text = $"OBan par {player.Nickname} ({player.UserId})",
                                },
                                Timestamp = DateTime.Now,
                            },
                        }
                    },SerialiseSetting))));
        }
        public static void KickPlayerAsync(Player player, Player sanctioned, string reason)
        {
            EventHandlers.Coroutines.Add(
            Timing.RunCoroutine(
            SendWebhookInformationDiscord(
            DiscordLog.Instance.Config.WebhookUrlLogSanction,
            "POST",
            JsonConvert.SerializeObject(
            new DiscordWebhookData.DiscordWebhook()
            {
                Username = "SCP:SL",
                Embeds = new DiscordWebhookData.DiscordEmbed[]
                {
                    new DiscordWebhookData.DiscordEmbed()
                    {
                        Title = DiscordLog.Instance.Config.SIName,
                        Description = "",
                        Color = 14310235,
                        Fields = new DiscordFiels[]
                        {
                            new DiscordFiels
                            {
                                Name = $"Kick",
                                Value = $"{Extensions.LogPlayer(sanctioned)}",
                            },
                            new DiscordFiels
                            {
                                Name = $"Raison",
                                Value = $"``{reason}``",
                            },
                        },
                        Footer = new DiscordFooter
                        {
                            IconUrl = "",
                            Text = $"kick par {player.Nickname} ({player.UserId})",
                        },
                        Timestamp = DateTime.Now,
                    },
                }
            }, SerialiseSetting))));
        }
        public static void WarnPlayerAsync(Player player, Player sanctioned, string reason)
        {
            EventHandlers.Coroutines.Add(
            Timing.RunCoroutine(
            SendWebhookInformationDiscord(
            DiscordLog.Instance.Config.WebhookUrlLogSanction,
            "POST",
            JsonConvert.SerializeObject(
                new DiscordWebhookData.DiscordWebhook()
                {
                    Username = "SCP:SL",
                    Embeds = new DiscordWebhookData.DiscordEmbed[]
                    {
                        new DiscordWebhookData.DiscordEmbed()
                        {
                                Title = DiscordLog.Instance.Config.SIName,
                                Description = "",
                                Color = 16773376,
                                Fields = new DiscordFiels[]
                                {
                                    new DiscordFiels
                                    {
                                        Name = "Warn",
                                        Value = $"{Extensions.LogPlayer(sanctioned)}",
                                    },
                                    new DiscordFiels
                                    {
                                        Name = $"Raison",
                                        Value = $"``{reason}``",
                                    },
                                },
                                Footer = new DiscordFooter
                                {
                                    IconUrl = "",
                                    Text = $"Warn par {player.Nickname} ({player.UserId})",
                                },
                                Timestamp = DateTime.Now,
                            },
                    }
                }, SerialiseSetting))));
        }
        public static void OWarnPlayerAsync(Player player, string sanctionedNickname, string sanctionedUserId, string reason)
        {
            EventHandlers.Coroutines.Add(
            Timing.RunCoroutine(
            SendWebhookInformationDiscord(
            DiscordLog.Instance.Config.WebhookUrlLogSanction,
            "POST",
            JsonConvert.SerializeObject(
                new DiscordWebhookData.DiscordWebhook()
                {
                    Username = "SCP:SL",
                    Embeds = new DiscordWebhookData.DiscordEmbed[]
                    {
                        new DiscordWebhookData.DiscordEmbed()
                        {
                                Title = DiscordLog.Instance.Config.SIName,
                                Description = "",
                                Color = 16773376,
                                Fields = new DiscordFiels[]
                                {
                                    new DiscordFiels
                                    {
                                        Name = "Owarn",
                                        Value = $"``{sanctionedNickname}``({sanctionedUserId})",
                                    },
                                    new DiscordFiels
                                    {
                                        Name = $"Raison",
                                        Value = $"``{reason}``",
                                    },
                                },
                                Footer = new DiscordFooter
                                {
                                    IconUrl = "",
                                    Text = $"Owarn par {player.Nickname} ({player.UserId})",
                                },
                                Timestamp = DateTime.Now,
                            },
                    }
                }, SerialiseSetting))));
        }


        public static void BugInfoAsync(Player player, string Title, string Info)
        {
            EventHandlers.Coroutines.Add(
            Timing.RunCoroutine(
            SendWebhookInformationDiscord(
            DiscordLog.Instance.Config.WebhookUrlLogSanction,
            "POST",
            JsonConvert.SerializeObject(
                new DiscordWebhookData.DiscordWebhook()
                {
                    Username = "SCP:SL",
                    ThreadName = $"[BUG] {Title}",
                    Embeds = new DiscordWebhookData.DiscordEmbed[]
                    {
                        new DiscordWebhookData.DiscordEmbed()
                            {
                                Title = DiscordLog.Instance.Config.SIName,
                                Description = "",
                                Color = 16773376,
                                Fields = new DiscordFiels[]
                                {
                                    new DiscordFiels 
                                    {
                                        Name = $"Bug",
                                        Value = $"{Extensions.LogPlayer(player)}",
                                    },
                                    new DiscordFiels 
                                    {
                                        Name = $"Info",
                                        Value = $"``{Info}``",
                                    },
                                },
                                Footer = new DiscordFooter 
                                {
                                    IconUrl = "",
                                    Text = $"Signalé par {player.Nickname} ({player.UserId})",
                                },
                                Timestamp = DateTime.Now,
                            },
                    }
                }, SerialiseSetting))));
        }
        public static void SugestionAsync(Player player, string Title, string Info)
        {
            EventHandlers.Coroutines.Add(
                Timing.RunCoroutine(
                SendWebhookInformationDiscord(
                DiscordLog.Instance.Config.WebhookUrlLogSanction,
                "POST",
                JsonConvert.SerializeObject(
                new DiscordWebhookData.DiscordWebhook()
                {
                    Username = "SCP:SL",
                    ThreadName = $"[SUGGESTION] {Title}",
                    Embeds = new DiscordWebhookData.DiscordEmbed[]
                    {
                        new DiscordWebhookData.DiscordEmbed
                            {
                                Title = DiscordLog.Instance.Config.SIName,
                                Description = "",
                                Color = 16773376,
                                Fields = new DiscordFiels[]
                                {
                                    new DiscordFiels
                                    {
                                        Name = $"Sugestion",
                                        Value = $"{Extensions.LogPlayer(player)}",
                                    },
                                    new DiscordFiels
                                    {
                                        Name = $"Info",
                                        Value = $"``{Info}``",
                                    },
                                },
                                Footer = new DiscordFooter
                                {
                                    IconUrl = "",
                                    Text = $"Sugesté par {player.Nickname} ({player.UserId})",
                                },
                                Timestamp = DateTime.Now,
                            },
                    }
                }, SerialiseSetting))));
        }
        public static void ReportAsync(Player Reporter, Player Reported, string reason = "Aucune raison donnée")
        {
            EventHandlers.Coroutines.Add(
                        Timing.RunCoroutine(
                        SendWebhookInformationDiscord(
                        DiscordLog.Instance.Config.WebhookReport,
                        "POST",
                        JsonConvert.SerializeObject(
            new DiscordWebhookData.DiscordWebhook()
            {
                Username = "SCP:SL",
                Content = DiscordLog.Instance.Config.Ping,
                Embeds = new DiscordWebhookData.DiscordEmbed[]
                {
                        new DiscordWebhookData.DiscordEmbed
                        {
                            Title = DiscordLog.Instance.Config.SIName,
                            Description = "",
                            Color = 16773376,
                            Fields = new DiscordFiels[]
                            {
                                new DiscordFiels
                                {
                                    Name = $"Report",
                                    Value = $"[{Reported.Id}] {Extensions.LogPlayer(Reported)}",
                                },
                                new DiscordFiels
                                {
                                    Name = $"Reason",
                                    Value = $"``{reason}``",
                                },
                            },
                            Footer = new DiscordFooter
                            {
                                IconUrl = "",
                                Text = $"Report par [{Reporter.Id}] {Reporter.Nickname} ({Reporter.UserId})",
                            },
                            Timestamp = DateTime.Now,
                        },
                }
            }, SerialiseSetting))));
        }
    }
}
