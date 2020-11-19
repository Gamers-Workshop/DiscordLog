using Exiled.API.Features;
using Exiled.Events.EventArgs;
using Newtonsoft.Json.Serialization;
using Respawning;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DiscordLog
{
    public class EventHandlers
    {
        internal readonly DiscordLog plugin;
        public float IntercomDelay;
        public string bloodwebhook;

        public EventHandlers(DiscordLog plugin) => this.plugin = plugin;
        public void OnWaintingForPlayers()
        {
            Webhook.SendWebhook(":zzz: Attente des joueurs...");
        }
        public void OnRoundStart()
        {
            Webhook.SendWebhook(":triangular_flag_on_post: Démarage de la partie.");
        }
        public void OnRoundEnd(RoundEndedEventArgs ev)
        {
            Webhook.SendWebhook($":checkered_flag: Fin de la partie.\nWin : {ev.LeadingTeam}");
        }
        public void OnRoundRestart()
        {
            Webhook.SendWebhook(":cyclone: Restart de la partie.");
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
                objcontent = ":articulated_lorry: L’insurrection du chaos est arrivée sur le site.";
                foreach (Player playerrespawn in ev.Players)
                {
                    objcontent += $"\n- {playerrespawn.Nickname} ({playerrespawn.UserId})";
                }
            }
            else
            {
                objcontent = "ERROR NO TEAM DETECTED";
                foreach (Player playerrespawn in ev.Players)
                {
                    objcontent += $"\n- {playerrespawn.Nickname} ({playerrespawn.UserId})";
                }
            }
            Webhook.SendWebhook(objcontent);
        }
        public void OnWarheadStart(StartingEventArgs ev)
        {
            if (ev.IsAllowed && ev.Player == null)
                Webhook.SendWebhook($":radioactive: la détonation de l’alpha warhead a été déclenchée par {ev.Player.Nickname} ({ev.Player.UserId})");
        }
        public void OnWarheadCancel(StoppingEventArgs ev)
        {
            if (ev.IsAllowed && ev.Player == null)
                Webhook.SendWebhook($":radioactive: la détonation de l’alpha warhead a été désactivée par {ev.Player.Nickname} ({ev.Player.UserId})");

        }
        public void OnDetonated()
        {
            Webhook.SendWebhook(":boom: Explosion du site");
        }
        public void OnAnnounceDecont(AnnouncingDecontaminationEventArgs ev)
        {
            if (ev.Id == 6)
                Webhook.SendWebhook(":biohazard: Décontamination de la LCZ");
        }
        public void OnGeneratorFinish(GeneratorActivatedEventArgs ev)
        {
            Webhook.SendWebhook($":computer: Le generateur dans la {ev.Generator.CurRoom} est activé");
        }
        public void OnPlayerJoin(JoinedEventArgs ev)
        {
            Webhook.SendWebhook($":chart_with_upwards_trend: {ev.Player.Nickname} ({ev.Player.UserId}) [{ev.Player.Id}] a rejoint le serveur");
        }
        public void OnPlayerLeave(LeftEventArgs ev)
        {
            Webhook.SendWebhook($":chart_with_downwards_trend: {ev.Player.Nickname} ({ev.Player.UserId}) a quitter le serveur");
        }
        public void OnChangingRole(ChangingRoleEventArgs ev)
        {
            if (ev.IsEscaped)
                Webhook.SendWebhook(string.Format($":??: {ev.Player.Nickname} ({ev.Player.UserId}) c'est échaper Il est devenue : {ev.NewRole}"));
            else
                Webhook.SendWebhook(string.Format($":??: {ev.Player.Nickname} ({ev.Player.UserId}) a spawn : {ev.NewRole}"));

        }
        public void OnPlayerHurt(HurtingEventArgs ev)
        {
            if (ev.IsAllowed && ev.Target != null && ev.Attacker != ev.Target && ev.Target.Role != RoleType.Spectator && ev.HitInformations.Amount < ev.Target.Health + ev.Target.AdrenalineHealth && !ev.Attacker.IsEnemy(ev.Target.Team))
                if (bloodwebhook != $":drop_of_blood: {ev.Target.Nickname} ({ev.Target.UserId}) est blessé par {ev.Attacker.Nickname} ({ev.Attacker.UserId}) avec {ev.DamageType.name}")
                Webhook.SendWebhook($":drop_of_blood: {ev.Target.Nickname} ({ev.Target.UserId}) est blessé par {ev.Attacker.Nickname} ({ev.Attacker.UserId}) avec {ev.DamageType.name}");
        }
        public void OnPlayerDeath(DiedEventArgs ev)
        {
            if (ev.Target == null) return;
            if (ev.Killer != null && ev.Killer != ev.Target)
            {
                Webhook.SendWebhook($":skull: {ev.Target.Nickname} ({ev.Target.UserId}) est mort par {ev.Killer.Nickname} ({ev.Killer.UserId}) avec {ev.HitInformations.GetDamageName()}");
            }
            else
            {
                Webhook.SendWebhook($":skull: {ev.Target.Nickname} ({ev.Target.UserId}) est mort par {ev.HitInformations.GetDamageName()}");
            }
        }
        public void OnDroppingItem(DroppingItemEventArgs ev)
        {
            if (ev.IsAllowed && ev.Player != null)
            Webhook.SendWebhook($":outbox_tray: {ev.Player.Nickname} ({ev.Player.UserId}) a jeter {ev.Item.id}");
        }
        public void OnPickingUpItem(PickingUpItemEventArgs ev)
        {
            if (ev.IsAllowed && ev.Player != null)
            Webhook.SendWebhook($":inbox_tray: {ev.Player.Nickname} ({ev.Player.UserId}) a récupérer {ev.Pickup.ItemId}");
        }
        public void OnPlayerUsedMedicalItem(UsedMedicalItemEventArgs ev)
        {
            if (ev.Player != null)
                Webhook.SendWebhook($":adhesive_bandage: {ev.Player.Nickname} ({ev.Player.UserId}) c'est soigné avec {ev.Item}");
        }
        public void OnGeneratorUnlock(UnlockingGeneratorEventArgs ev)
        {
            if (ev.IsAllowed && ev.Player != null)
                Webhook.SendWebhook($":computer: {ev.Player.Nickname} ({ev.Player.UserId}) a débloqué un générateur dans là : {ev.Generator._curRoom}");
        }
        public void OnEjectingGeneratorTablet(EjectingGeneratorTabletEventArgs ev)
        {
            if (ev.IsAllowed && ev.Player.UserId != null)
                Webhook.SendWebhook($":computer: {ev.Player.Nickname} ({ev.Player.UserId}) a ejecté la tablette du générateur de la : {ev.Generator._curRoom}");
        }
        public void OnGeneratorInsert(InsertingGeneratorTabletEventArgs ev)
        {
            if (ev.IsAllowed && ev.Player.UserId != null)
                Webhook.SendWebhook($":computer: {ev.Player.Nickname} ({ev.Player.UserId}) a inséré une tablette dans un générateur de la : {ev.Generator._curRoom}");
        }
        public void OnActivatingWarheadPanel(ActivatingWarheadPanelEventArgs ev)
        {
            if (ev.IsAllowed && ev.Player.UserId == null && !Object.FindObjectOfType<AlphaWarheadOutsitePanel>().keycardEntered)
                Webhook.SendWebhook($":radioactive: {ev.Player.Nickname} ({ev.Player.UserId}) a ouvert le lock pour activé l'alpha warhead");
        }
        public void OnIntercomSpeaking(IntercomSpeakingEventArgs ev)
        {
            if (ev.IsAllowed && ev.Player == null && !Intercom.host.speaking && Time.time > IntercomDelay + 5.1)
            { 
                IntercomDelay += Time.time;
                Webhook.SendWebhook($":loudspeaker: {ev.Player.Nickname} ({ev.Player.UserId}) Utilise l'intercom");
            }
        }
        public void OnHandcuffing(HandcuffingEventArgs ev)
        {
            if (ev.IsAllowed && ev.Cuffer != null)
                Webhook.SendWebhook($":??: {ev.Target.Nickname} ({ev.Target.UserId}) a été menoté par {ev.Cuffer.Nickname} ({ev.Cuffer.UserId})");
        }
        public void OnRemovingHandcuffs(RemovingHandcuffsEventArgs ev)
        {
            if (ev.IsAllowed && ev.Cuffer != null)
                Webhook.SendWebhook($":??: {ev.Target.Nickname} ({ev.Target.UserId}) a été libéré par {ev.Cuffer.Nickname} ({ev.Cuffer.UserId})");
            else if (ev.IsAllowed)
                Webhook.SendWebhook($":??: {ev.Target.Nickname} ({ev.Target.UserId}) a été libéré");
        }
        public void On914Activating(ActivatingEventArgs ev)
        {
            if (ev.IsAllowed && ev.Player != null)
                Webhook.SendWebhook($":gear: SCP-914 a été enclenché par {ev.Player.Nickname} ({ev.Player.UserId})");
        }
        public void On914Upgrade(UpgradingItemsEventArgs ev)
        {
            string str = "";
            foreach (Pickup item in ev.Items)
            {
                str += string.Format($"\n- {item.itemId}");
            }
            Webhook.SendWebhook($":gear: Scp-914 a été enclenché en {ev.KnobSetting}:{str}");
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
                Webhook.SendWebhookStaff($"{ev.Sender.Nickname} ({ev.Sender.UserId}) a envoyé ``{ev.Name} {str1}``");
            }
        }
    }
}

