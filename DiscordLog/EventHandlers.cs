using Exiled.API.Features;
using Exiled.Events.EventArgs;
using MEC;
using Newtonsoft.Json.Serialization;
using Respawning;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DiscordLog
{
    public class EventHandlers
    {
        internal readonly DiscordLog plugin;
        public static List<CoroutineHandle> Coroutines = new List<CoroutineHandle>();

        public bool RoundIsStart = false;
        public float IntercomDelay;
        public string bloodwebhook;

        public EventHandlers(DiscordLog plugin) => this.plugin = plugin;
        public void OnWaintingForPlayers()
        {
            foreach (CoroutineHandle handle in Coroutines)
                Timing.KillCoroutines(handle);
            plugin.NormalisedName.Clear();
            if (DiscordLog.Instance.Config.WebhookUrlLogJoueur != string.Empty)
                Coroutines.Add(Timing.RunCoroutine(plugin.RunSendWebhook()));
            if (DiscordLog.Instance.Config.WebhookSi != "null" || DiscordLog.Instance.Config.IdMessage != "null" )
                Coroutines.Add(Timing.RunCoroutine(plugin.RunUpdateWebhook()));
            plugin.LOG += ":zzz: Attente des joueurs...\n";
            RoundIsStart = false;
        }
        public void OnRoundStart()
        {
            string RoundStart = $":triangular_flag_on_post: Démarage de la partie avec {Player.List.Where((p) => p.Role != RoleType.None).Count()} joueurs\n";
            foreach (Player player in Player.List.Where((p) => p.Role != RoleType.None))
                RoundStart += $"- ``{player.Nickname}`` ({ConvertID(player.UserId)}) a spawn {player.Role}\n";
            plugin.LOG += RoundStart;
            RoundIsStart = true;
        }
        public void OnRoundEnd(RoundEndedEventArgs ev)
        {
            plugin.LOG += $":checkered_flag: Fin de la partie.\n```Win: {ev.LeadingTeam}\nClass-D échapé: {ev.ClassList.class_ds}\nScientifique sauvé: {ev.ClassList.scientists}\nSCPs: {ev.ClassList.scps_except_zombies}\n{(Exiled.API.Features.Warhead.IsDetonated ? "Le site a explosé" : $"Le site n'a pas explosé\nMort par la warhead: {ev.ClassList.warhead_kills}")}```\n";
        }
        public void OnRoundRestart()
        {
            plugin.LOG += ":cyclone: Restart de la partie.\n";
        }
        public void OnTeamRespawn(RespawningTeamEventArgs ev)
        {
            if (ev.Players.Count == 0) return;
            string objcontent;
            if (ev.NextKnownTeam == SpawnableTeamType.NineTailedFox)
            {
                objcontent = ":helicopter: L’équipe Epsilon est arrivée sur le site.\n";
                foreach (Player playerrespawn in ev.Players)
                {
                    objcontent += $"- ``{playerrespawn.Nickname}`` ({ConvertID(playerrespawn.UserId)})\n";
                }
            }
            else if (ev.NextKnownTeam == SpawnableTeamType.ChaosInsurgency)
            {
                objcontent = ":articulated_lorry: L’Insurrection du Chaos est arrivée sur le site.\n";
                foreach (Player playerrespawn in ev.Players)
                {
                    objcontent += $"- ``{playerrespawn.Nickname}`` ({ConvertID(playerrespawn.UserId)})\n";
                }
            }
            else
            {
                objcontent = ":snake: La Mains du Serpent est arrivée sur le site\n";
                foreach (Player playerrespawn in ev.Players)
                {
                    objcontent += $"- ``{playerrespawn.Nickname}`` ({ConvertID(playerrespawn.UserId)})\n";
                }
            }
            plugin.LOG += objcontent;
        }
        public void OnWarheadStart(StartingEventArgs ev)
        {
            if (ev.IsAllowed && ev.Player != null)
                plugin.LOG += $":radioactive: ``{ev.Player.Nickname}`` ({ConvertID(ev.Player.UserId)}) à déclenché la détonation de l'Alpha Warhead\n";
        }
        public void OnWarheadCancel(StoppingEventArgs ev)
        {
            if (ev.IsAllowed && ev.Player != null)
                plugin.LOG += $":radioactive: ``{ev.Player.Nickname}`` ({ConvertID(ev.Player.UserId)}) à désactivée la détonation de l’alpha warhead\n";
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
            plugin.LOG += $":computer: Le generateur dans la {ev.Generator.CurRoom} est activé\n";
        }
        public void OnPlayerAuth(PreAuthenticatingEventArgs ev)
        {
            Webhook.SendWebhookStaff($":flag_{ev.Country.ToLower()}: {ConvertID(ev.UserId)} ||{ev.Request.RemoteEndPoint}|| tente une connexion sur le serveur\n");
        }
        public void OnPlayerVerified(VerifiedEventArgs ev)
        {
            plugin.LOG += $":chart_with_upwards_trend: ``{ev.Player.Nickname}`` ({ConvertID(ev.Player.UserId)}) [{ev.Player.Id}] a rejoint le serveur\n";
            string PlayerName = ev.Player.Nickname.Normalize(System.Text.NormalizationForm.FormKD);
            if (PlayerName.Length < 18)
              plugin.NormalisedName.Add(ev.Player, $"[{ev.Player.Id}] {PlayerName}");
            else
              plugin.NormalisedName.Add(ev.Player, $"[{ev.Player.Id}] {PlayerName.Remove(17)}");
        }
        public void OnPlayerDestroying(DestroyingEventArgs ev)
        {
            if (ev.Player == null || ev.Player.Role == RoleType.None) return;
            plugin.LOG += $":chart_with_downwards_trend: ``{ev.Player.Nickname}`` ({ConvertID(ev.Player.UserId)}) a quitter le serveur\n";
            plugin.NormalisedName.Remove(ev.Player);
            if (Player.List.ToList().Count < 2)
            {
                if (plugin.LOG != null && DiscordLog.Instance.Config.WebhookUrlLogJoueur != string.Empty)
                {
                    Webhook.SendWebhook(plugin.LOG);
                    plugin.LOG = null;
                }
                if (DiscordLog.Instance.Config.WebhookSi != "null" && DiscordLog.Instance.Config.IdMessage != "null")  plugin.UpdateWebhook();
            }
        }
        public void OnChangingRole(ChangingRoleEventArgs ev)
        {
            if (!RoundIsStart || ev.Player == null) return;
            if (SerpentsHand.API.SerpentsHand.GetSHPlayers().Contains(ev.Player))
                plugin.LOG += $":new: ``{ev.Player.Nickname}`` ({ConvertID(ev.Player.UserId)}) a spawn : SerpentHand\n";
            else if (ev.IsEscaped)
                plugin.LOG += $":new: ``{ev.Player.Nickname}`` ({ConvertID(ev.Player.UserId)}) c'est échaper Il est devenue : {ev.NewRole}\n";
            else
                plugin.LOG += $":new: ``{ev.Player.Nickname}`` ({ConvertID(ev.Player.UserId)}) a spawn : {ev.NewRole}\n";
        }
        public void OnPlayerHurt(HurtingEventArgs ev)
        {
            /*if (ev.IsAllowed && ev.Target != null && ev.Attacker != ev.Target && ev.Target.Role != RoleType.Spectator && ev.HitInformations.Amount < ev.Target.Health + ev.Target.AdrenalineHealth && !ev.Attacker.IsEnemy(ev.Target.Team))
                if (bloodwebhook != $":drop_of_blood: ``{ev.Target.Nickname}`` ({ConvertID(ev.Target.UserId)}) est blessé par ``{ev.Attacker.Nickname}`` ({ev.Attacker.UserId}) avec {ev.DamageType.name}")
                    plugin.LOG += $":drop_of_blood: ``{ev.Target.Nickname}`` ({ConvertID(ev.Target.UserId)}) est blessé par ``{ev.Attacker.Nickname}`` ({ev.Attacker.UserId}) avec {ev.DamageType.name}\n";*/
        }
        public void OnPlayerDeath(DiedEventArgs ev)
        {
            if (ev.Target.Role == RoleType.None || ev.HitInformations.Attacker == "DISCONNECT") return;
            if (ev.Killer != null && ev.Killer != ev.Target)
            {
                plugin.LOG += $":skull: ``{ev.Target.Nickname}`` ({ConvertID(ev.Target.UserId)}) est mort par ``{ev.Killer.Nickname}`` ({ConvertID(ev.Killer.UserId)}) avec {ev.HitInformations.GetDamageName()}\n";
            }
            else
            {
                plugin.LOG += $":skull: ``{ev.Target.Nickname}`` ({ConvertID(ev.Target.UserId)}) est mort par {ev.HitInformations.GetDamageName()}\n";
            }
        }
        public void OnDroppingItem(DroppingItemEventArgs ev)
        {
            if (ev.IsAllowed && ev.Player != null)
                plugin.LOG += $":outbox_tray: ``{ev.Player.Nickname}`` ({ConvertID(ev.Player.UserId)}) a jeter {ev.Item.id}\n";
        }
        public void OnPickingUpItem(PickingUpItemEventArgs ev)
        {
            if (ev.IsAllowed && ev.Player != null)
                plugin.LOG += $":inbox_tray: ``{ev.Player.Nickname}`` ({ConvertID(ev.Player.UserId)}) a récupérer {ev.Pickup.ItemId}\n";
        }
        public void OnPlayerUsedMedicalItem(UsedMedicalItemEventArgs ev)
        {
            if (ev.Player != null)
                plugin.LOG += $":adhesive_bandage: ``{ev.Player.Nickname}`` ({ConvertID(ev.Player.UserId)}) c'est soigné avec {ev.Item}\n";
        }
        public void OnGeneratorUnlock(UnlockingGeneratorEventArgs ev)
        {
            if (ev.IsAllowed && ev.Player != null)
                plugin.LOG += $":computer: ``{ev.Player.Nickname}`` ({ConvertID(ev.Player.UserId)}) a débloqué un générateur dans là : {ev.Generator.CurRoom}\n";
        }
        public void OnEjectingGeneratorTablet(EjectingGeneratorTabletEventArgs ev)
        {
            if (ev.IsAllowed && ev.Player != null)
                plugin.LOG += $":computer: ``{ev.Player.Nickname}`` ({ConvertID(ev.Player.UserId)}) a ejecté la tablette du générateur de la : {ev.Generator.CurRoom}\n";
        }
        public void OnGeneratorInsert(InsertingGeneratorTabletEventArgs ev)
        {
            if (ev.IsAllowed && ev.Player != null)
                plugin.LOG += $":computer: ``{ev.Player.Nickname}`` ({ConvertID(ev.Player.UserId)}) a inséré une tablette dans un générateur de la : {ev.Generator.CurRoom}\n";
        }
        public void OnActivatingWarheadPanel(ActivatingWarheadPanelEventArgs ev)
        {
            if (ev.IsAllowed && ev.Player != null && !UnityEngine.Object.FindObjectOfType<AlphaWarheadOutsitePanel>().keycardEntered)
                plugin.LOG += $":radioactive: ``{ev.Player.Nickname}`` ({ConvertID(ev.Player.UserId)}) a ouvert le lock pour activé l'alpha warhead\n";
        }
        public void OnIntercomSpeaking(IntercomSpeakingEventArgs ev)
        {
            var intercom = UnityEngine.Object.FindObjectOfType<Intercom>();
            if (intercom.speaker != null && !intercom.Muted && intercom.remainingCooldown <= 0f && Time.time > IntercomDelay + 5.1)
            {
                IntercomDelay += Time.time;
                plugin.LOG += $":loudspeaker: ``{ev.Player.Nickname}`` ({ConvertID(ev.Player.UserId)}) Utilise l'intercom\n";
            }
        }
        public void OnHandcuffing(HandcuffingEventArgs ev)
        {
            if (ev.Cuffer != null)
                plugin.LOG += $":chains: ``{ev.Target.Nickname}`` ({ConvertID(ev.Target.UserId)}) a été menoté par ``{ev.Cuffer.Nickname}`` ({ConvertID(ev.Cuffer.UserId)})\n";
        }
        public void OnRemovingHandcuffs(RemovingHandcuffsEventArgs ev)
        {
            if (ev.Cuffer != null)
                plugin.LOG += $":chains: ``{ev.Target.Nickname}`` ({ConvertID(ev.Target.UserId)}) a été démenoté par ``{ev.Cuffer.Nickname}`` ({ConvertID(ev.Cuffer.UserId)})\n";
            else
                plugin.LOG += $":chains: ``{ev.Target.Nickname}`` ({ConvertID(ev.Target.UserId)}) a été démenoté\n";
        }
        public void OnEnteringPocketDimension(EnteringPocketDimensionEventArgs ev)
        {
            if (ev.Player != null)
                plugin.LOG += $":hole: ``{ev.Player.Nickname}`` ({ConvertID(ev.Player.UserId)}) est entrer dans la pocket\n";
        }
        public void OnEscapingPocketDimension(EscapingPocketDimensionEventArgs ev)
        {
            if (ev.Player != null)
                plugin.LOG += $":hole: ``{ev.Player.Nickname}`` ({ConvertID(ev.Player.UserId)}) à echaper a la pocket\n";
        }
        public void On914Activating(ActivatingEventArgs ev)
        {
            if (ev.IsAllowed && ev.Player != null)
                plugin.LOG += $":gear: SCP-914 a été enclenché par ``{ev.Player.Nickname}`` ({ConvertID(ev.Player.UserId)})\n";
        }
        public void On914Upgrade(UpgradingItemsEventArgs ev)
        {
            string str = $":gear: SCP-914 a été enclenché en {ev.KnobSetting}:\n";
            foreach (Pickup item in ev.Items)
            {
                str += $"- {item.itemId}\n";
            }
            plugin.LOG += str;
        }
        public void OnBanning(BanningEventArgs ev)
        {
            if (ev.IsAllowed && ev.Target != null && ev.Issuer != null)
            {
                Webhook.BanPlayerAsync(ev.Issuer, ev.Target,ev.Reason, ev.Duration);
                Webhook.SendWebhookStaff($":hammer: ``{ev.Target.Nickname}`` ({ConvertID(ev.Target.UserId)}) a été bannie car ``{ev.Reason}`` pendant {ev.Duration} secondes par ``{ev.Issuer.Nickname}`` ({ConvertID(ev.Issuer.UserId)})");
            } 
        }
        public void OnKicking(KickingEventArgs ev)
        {
            if (ev.IsAllowed && ev.Target != null && ev.Issuer != null)
            { 
                Webhook.SendWebhookStaff($":mans_shoe: ``{ev.Target.Nickname}`` ({ConvertID(ev.Target.UserId)}) a été kick car ``{ev.Reason}`` par ``{ev.Issuer.Nickname}`` ({ConvertID(ev.Issuer.UserId)})");
                Webhook.KickPlayerAsync(ev.Issuer, ev.Target, ev.Reason);
            }
        }
        public void OnSendingRemoteAdminCommand(SendingRemoteAdminCommandEventArgs ev)
        {
            if (!ev.Success && ev.Name.ToLower() == "ban" && ev.Name.ToLower() == "kick" && ev.Name == null) return;
            switch (ev.Name.ToLower())
            {
                case "jail":
                    {
                        if (int.TryParse(ev.Arguments[0], out int result))
                        {
                            foreach (Player player in Player.List)
                            {
                                if (result == player.Id)
                                {
                                    Webhook.SendWebhookStaff($"``{ev.Sender.Nickname}`` ({ConvertID(ev.Sender.UserId)}) a jail ``{player.Nickname}`` ({player.UserId})");
                                    return;
                                }
                            }
                        }
                    }
                    return;
                case "forceclass":
                    {
                        if (int.TryParse(ev.Arguments[1], out int Role))
                        {
                            Webhook.SendWebhookStaff($"``{ev.Sender.Nickname}`` ({ConvertID(ev.Sender.UserId)}) a changer le rôle de {ev.Arguments[0]} en {(RoleType)Role}");
                        }
                    }
                    return;
                case "give":
                    {
                        if (int.TryParse(ev.Arguments[1], out int Item))
                        {
                            Webhook.SendWebhookStaff($"``{ev.Sender.Nickname}`` ({ConvertID(ev.Sender.UserId)}) a give a {ev.Arguments[0]} : {(ItemType)Item}");
                        }
                    }
                    return;
                default:
                    {
                        string str1 = null;
                        foreach (string str2 in ev.Arguments)
                            str1 += $" {str2}";
                        Webhook.SendWebhookStaff($"``{ev.Sender.Nickname}`` ({ConvertID(ev.Sender.UserId)}) a envoyé ``{ev.Name}{str1}``");
                    }
                    return;
            }
        }
        public static string ConvertID(string UserID)
        {
            if (UserID.EndsWith("@discord"))
            {
                return $"<@{UserID.Replace("@discord", "")}>";
            }
            else if (UserID.EndsWith("@steam"))
            {
                return $"{UserID}[:link:](<https://steamidfinder.com/lookup/{UserID.Replace("@steam","")}\"SteamFinder\">)";
            }
            return UserID;
        }
    }
}

