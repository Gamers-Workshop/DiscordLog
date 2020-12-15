using DiscordWebhookData;
using Exiled.API.Features;
using Exiled.Events.EventArgs;
using MEC;
using Newtonsoft.Json.Serialization;
using Respawning;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Utf8Json;

namespace DiscordLog
{
    public class EventHandlers
    {
        internal readonly DiscordLog plugin;
        /*private bool loaded = false;

        /** Infosender **//*
        private readonly UdpClient udpClient = new UdpClient();
        private readonly HttpClient http = new HttpClient();

        internal Task sendertask;
        internal async Task SenderAsync()
        {
            Log.Debug($"[SenderAsync] Started.", plugin.Config.Debugplugin);

            while (true)
            {
                try
                {
                    if (plugin.Config.WebhookSi == "none")
                    {
                        Log.Info($"[SenderAsync] Disabled(config:({plugin.Config.WebhookSi}). breaked.");
                        break;
                    }

                    if (!this.loaded)
                    {
                        Log.Debug($"[SenderAsync] Plugin not loaded. Skipped...", plugin.Config.Debugplugin);
                        await Task.Delay(TimeSpan.FromSeconds(30));
                    }

                    Serverinfo cinfo = new Serverinfo();

                    DateTime dt = DateTime.Now;
                    cinfo.time = dt.ToString("yyyy-MM-ddTHH:mm:sszzzz");
                    cinfo.name = ServerConsole.singleton.RefreshServerName();
                    cinfo.ip = ServerConsole.Ip;
                    cinfo.port = ServerConsole.Port;
                    cinfo.playing = PlayerManager.players.Count;
                    cinfo.maxplayer = CustomNetworkManager.slots;
                    cinfo.duration = RoundSummary.roundTime;

                    if (cinfo.playing > 0)
                    {
                        foreach (GameObject player in PlayerManager.players)
                        {
                            Playerinfo ply = new Playerinfo
                            {
                                name = ReferenceHub.GetHub(player).nicknameSync.MyNick,
                                userid = ReferenceHub.GetHub(player).characterClassManager.UserId,
                                ip = ReferenceHub.GetHub(player).queryProcessor._ipAddress,
                                role = ReferenceHub.GetHub(player).characterClassManager.CurClass.ToString(),
                                rank = ReferenceHub.GetHub(player).serverRoles.MyText
                            };

                            cinfo.players.Add(ply);
                        }
                    }

                    string json = cinfo.ToJson();
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    await http.PostAsync(plugin.Config.WebhookSi, content);
                    }
                catch (Exception e)
                {
                    throw e;
                }
                await Task.Delay(TimeSpan.FromSeconds(30));
            }
        }*/
        public static List<CoroutineHandle> Coroutines = new List<CoroutineHandle>();

        public float IntercomDelay;
        public string bloodwebhook;

        public EventHandlers(DiscordLog plugin) => this.plugin = plugin;
        public void OnWaintingForPlayers()
        {
            foreach (CoroutineHandle handle in Coroutines)
                Timing.KillCoroutines(handle);
            Coroutines.Add(Timing.RunCoroutine(plugin.RunSendWebhook()));
            plugin.LOG += ":zzz: Attente des joueurs...\n";
        //    loaded = true;
        }
        public void OnRoundStart()
        {
            plugin.LOG += ":triangular_flag_on_post: Démarage de la partie.\n";
        }
        public void OnRoundEnd(RoundEndedEventArgs ev)
        {
            plugin.LOG +=$":checkered_flag: Fin de la partie.\nWin : {ev.LeadingTeam}\n";
        }
        public void OnRoundRestart()
        {
            plugin.LOG += ":cyclone: Restart de la partie.\n";
        }
        public void OnTeamRespawn(RespawningTeamEventArgs ev)
        {
            string objcontent;
            if (ev.NextKnownTeam == SpawnableTeamType.NineTailedFox)
            {
                objcontent = ":helicopter: L’équipe Epsilon est arrivée sur le site.";
                foreach (Player playerrespawn in ev.Players)
                {
                    objcontent += $"\n- {playerrespawn.Nickname} ({playerrespawn.UserId})";
                }
            }
            else if (ev.NextKnownTeam == SpawnableTeamType.ChaosInsurgency)
            {
                objcontent = ":articulated_lorry: L’Insurrection du Chaos est arrivée sur le site.";
                foreach (Player playerrespawn in ev.Players)
                {
                    objcontent += $"\n- {playerrespawn.Nickname} ({playerrespawn.UserId})";
                }
            }
            else
            {
                objcontent = ":snake: La Mains du Serpent est arrivée sur le site";
                foreach (Player playerrespawn in ev.Players)
                {
                    objcontent += $"\n- {playerrespawn.Nickname} ({playerrespawn.UserId})";
                }
            }
            plugin.LOG += objcontent + "\n";
        }
        public void OnWarheadStart(StartingEventArgs ev)
        {
            if (ev.IsAllowed && ev.Player == null)
                plugin.LOG +=$":radioactive: la détonation de l’alpha warhead a été déclenchée par {ev.Player.Nickname} ({ev.Player.UserId})\n";
        }
        public void OnWarheadCancel(StoppingEventArgs ev)
        {
            if (ev.IsAllowed && ev.Player == null)
                plugin.LOG +=$":radioactive: la détonation de l’alpha warhead a été désactivée par {ev.Player.Nickname} ({ev.Player.UserId})\n";

        }
        public void OnDetonated()
        {
            plugin.LOG += ":boom: Explosion du site\n";
        }
        public void OnAnnounceDecont(AnnouncingDecontaminationEventArgs ev)
        {
            if (ev.Id == 6)
                plugin.LOG += ":biohazard: Décontamination de la LCZ\n";
        }
        public void OnGeneratorFinish(GeneratorActivatedEventArgs ev)
        {
            plugin.LOG +=$":computer: Le generateur dans la {ev.Generator.CurRoom} est activé\n";
        }
        public void OnPlayerJoin(JoinedEventArgs ev)
        {
            plugin.LOG +=$":chart_with_upwards_trend: {ev.Player.Nickname} ({ev.Player.UserId}) [{ev.Player.Id}] a rejoint le serveur\n";
        }
        public void OnPlayerLeave(LeftEventArgs ev)
        {
            plugin.LOG +=$":chart_with_downwards_trend: {ev.Player.Nickname} ({ev.Player.UserId}) a quitter le serveur\n";
        }
        public void OnChangingRole(ChangingRoleEventArgs ev)
        {
            if (ev.IsEscaped)
                plugin.LOG +=string.Format($":new: {ev.Player.Nickname} ({ev.Player.UserId}) c'est échaper Il est devenue : {ev.NewRole}\n");
            else
                plugin.LOG +=string.Format($":new: {ev.Player.Nickname} ({ev.Player.UserId}) a spawn : {ev.NewRole}\n");

        }
        public void OnPlayerHurt(HurtingEventArgs ev)
        {
            if (ev.IsAllowed && ev.Target != null && ev.Attacker != ev.Target && ev.Target.Role != RoleType.Spectator && ev.HitInformations.Amount < ev.Target.Health + ev.Target.AdrenalineHealth && !ev.Attacker.IsEnemy(ev.Target.Team))
                if (bloodwebhook != $":drop_of_blood: {ev.Target.Nickname} ({ev.Target.UserId}) est blessé par {ev.Attacker.Nickname} ({ev.Attacker.UserId}) avec {ev.DamageType.name}")
                plugin.LOG +=$":drop_of_blood: {ev.Target.Nickname} ({ev.Target.UserId}) est blessé par {ev.Attacker.Nickname} ({ev.Attacker.UserId}) avec {ev.DamageType.name}\n";
        }
        public void OnPlayerDeath(DiedEventArgs ev)
        {
            if (ev.Target == null) return;
            if (ev.Killer != null && ev.Killer != ev.Target)
            {
                plugin.LOG +=$":skull: {ev.Target.Nickname} ({ev.Target.UserId}) est mort par {ev.Killer.Nickname} ({ev.Killer.UserId}) avec {ev.HitInformations.GetDamageName()}\n";
            }
            else
            {
                plugin.LOG +=$":skull: {ev.Target.Nickname} ({ev.Target.UserId}) est mort par {ev.HitInformations.GetDamageName()}\n";
            }
        }
        public void OnDroppingItem(DroppingItemEventArgs ev)
        {
            if (ev.IsAllowed && ev.Player != null)
            plugin.LOG +=$":outbox_tray: {ev.Player.Nickname} ({ev.Player.UserId}) a jeter {ev.Item.id}\n";
        }
        public void OnPickingUpItem(PickingUpItemEventArgs ev)
        {
            if (ev.IsAllowed && ev.Player != null)
            plugin.LOG +=$":inbox_tray: {ev.Player.Nickname} ({ev.Player.UserId}) a récupérer {ev.Pickup.ItemId}\n";
        }
        public void OnPlayerUsedMedicalItem(UsedMedicalItemEventArgs ev)
        {
            if (ev.Player != null)
                plugin.LOG +=$":adhesive_bandage: {ev.Player.Nickname} ({ev.Player.UserId}) c'est soigné avec {ev.Item}\n";
        }
        public void OnGeneratorUnlock(UnlockingGeneratorEventArgs ev)
        {
            if (ev.IsAllowed && ev.Player != null)
                plugin.LOG +=$":computer: {ev.Player.Nickname} ({ev.Player.UserId}) a débloqué un générateur dans là : {ev.Generator._curRoom}\n";
        }
        public void OnEjectingGeneratorTablet(EjectingGeneratorTabletEventArgs ev)
        {
            if (ev.IsAllowed && ev.Player.UserId != null)
                plugin.LOG +=$":computer: {ev.Player.Nickname} ({ev.Player.UserId}) a ejecté la tablette du générateur de la : {ev.Generator._curRoom}\n";
        }
        public void OnGeneratorInsert(InsertingGeneratorTabletEventArgs ev)
        {
            if (ev.IsAllowed && ev.Player.UserId != null)
                plugin.LOG +=$":computer: {ev.Player.Nickname} ({ev.Player.UserId}) a inséré une tablette dans un générateur de la : {ev.Generator._curRoom}\n";
        }
        public void OnActivatingWarheadPanel(ActivatingWarheadPanelEventArgs ev)
        {
            if (ev.IsAllowed && ev.Player != null && !UnityEngine.Object.FindObjectOfType<AlphaWarheadOutsitePanel>().keycardEntered)
                plugin.LOG +=$":radioactive: {ev.Player.Nickname} ({ev.Player.UserId}) a ouvert le lock pour activé l'alpha warhead\n";
        }
        public void OnIntercomSpeaking(IntercomSpeakingEventArgs ev)
        {
            if (ev.IsAllowed && ev.Player != null && !Intercom.host.speaking && Time.time > IntercomDelay + 5.1)
            { 
                IntercomDelay += Time.time;
                plugin.LOG +=$":loudspeaker: {ev.Player.Nickname} ({ev.Player.UserId}) Utilise l'intercom\n";
            }
        }
        public void OnHandcuffing(HandcuffingEventArgs ev)
        {
            if (ev.IsAllowed && ev.Cuffer != null)
                plugin.LOG += $":chains: {ev.Target.Nickname} ({ev.Target.UserId}) a été menoté par {ev.Cuffer.Nickname} ({ev.Cuffer.UserId})\n";
        }
        public void OnRemovingHandcuffs(RemovingHandcuffsEventArgs ev)
        {
            if (ev.IsAllowed && ev.Cuffer != null)
                plugin.LOG +=$":chains: {ev.Target.Nickname} ({ev.Target.UserId}) a été libéré par {ev.Cuffer.Nickname} ({ev.Cuffer.UserId})\n";
            else if (ev.IsAllowed)
                plugin.LOG +=$":chains: {ev.Target.Nickname} ({ev.Target.UserId}) a été libéré\n";
        }
        public void On914Activating(ActivatingEventArgs ev)
        {
            if (ev.IsAllowed && ev.Player != null)
                plugin.LOG +=$":gear: SCP-914 a été enclenché par {ev.Player.Nickname} ({ev.Player.UserId})\n";
        }
        public void On914Upgrade(UpgradingItemsEventArgs ev)
        {
            string str = "";
            foreach (Pickup item in ev.Items)
            {
                str += string.Format($"\n- {item.itemId}");
            }
            plugin.LOG +=$":gear: Scp-914 a été enclenché en {ev.KnobSetting}:{str}\n";
        }
        public void OnBanning(BanningEventArgs ev)
        {
        if (ev.IsAllowed && ev.Target != null && ev.Issuer != null)
            Webhook.SendWebhookStaff($":hammer: {ev.Target.Nickname} ({ev.Target.UserId}) a été bannie car ``{ev.Reason}`` pendant {ev.Duration} secondes par {ev.Issuer.Nickname} ({ev.Issuer.UserId})");
        }
        public void OnKicking(KickingEventArgs ev)
        {
            if (ev.IsAllowed && ev.Target != null && ev.Issuer != null)
                Webhook.SendWebhookStaff($":mans_shoe: {ev.Target.Nickname} ({ev.Target.UserId}) a été kick car ``{ev.Reason}`` par {ev.Issuer.Nickname} ({ev.Issuer.UserId})");
        }
        public void OnSendingRemoteAdminCommand(SendingRemoteAdminCommandEventArgs ev)
        {
            if (!ev.Success && ev.Name.ToLower() == "ban" && ev.Name.ToLower() == "kick" && ev.Name == null) return;
            if (ev.Name.ToLower() == "jail" && int.TryParse(ev.Arguments[0], out int result))
            {
                foreach (Player player in Player.List)
                {
                    if (result == player.Id)
                    {
                        Webhook.SendWebhookStaff($"{ev.Sender.Nickname} ({ev.Sender.UserId}) a jail {player.Nickname} {player.UserId})");
                        break;
                    }
                }
            }
            else
            {
                if (ev.Name.ToLower() == "jail")
                    return;
                string str1 = null;
                foreach (string str2 in ev.Arguments)
                    str1 += $" {str2}";
                Webhook.SendWebhookStaff($"{ev.Sender.Nickname} ({ev.Sender.UserId}) a envoyé ``{ev.Name}{str1}``");
            }
        }
        /*public static IEnumerator<float> DoSanction(Player Sanctioned,Player Sanctionneur,string Reason,string Type, int SanctionTime)
        {
            string SanctionneurNickName;
            string SanctionneurId;
            if (Sanctionneur == null)
            {
                SanctionneurNickName = "Dedicated Server";
                SanctionneurId = "SERVEUR";
            }
            else
            {
                SanctionneurNickName = Sanctionneur.Nickname;
                SanctionneurId = Sanctionneur.UserId;
            }
            DiscordLog.SanctionedPlayer.Add(new Sanction
            {
                SanctionedUserId = Sanctioned.UserId,
                SanctionedUserNickName = Sanctioned.Nickname,
                Type = Type,
                Reason = Reason,
                Time = DateTime.Now,
                SanctionTime = SanctionTime,
                UnBanTime = DateTime.Now.AddMinutes(SanctionTime),
                SanctionneurUserId = SanctionneurId,
                SanctionneurUserNickName = SanctionneurNickName,
            });
            JsonString = JsonConverter.;
            yield return Timing.WaitForSeconds(1f);
            Sanctioned.ClearBroadcasts();
            Sanctioned.Broadcast(20, $"<color=red>Vous avez été {Type} par {Sanctionneur} car : {Reason}</color>");
        }*/
    }
}

