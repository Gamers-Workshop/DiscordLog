using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.Events.EventArgs;
using InventorySystem.Items.Firearms;
using InventorySystem.Items.Firearms.Ammo;
using InventorySystem.Items.MicroHID;
using InventorySystem.Items.Radio;
using InventorySystem.Items.Usables.Scp330;
using MEC;
using Newtonsoft.Json.Serialization;
using NorthwoodLib;
using PlayerStatsSystem;
using Respawning;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;

namespace DiscordLog
{
    public class EventHandlers
    {
        internal readonly DiscordLog plugin;
        public static List<CoroutineHandle> Coroutines = new();
        private static readonly Dictionary<string, string> SteamNickName = new();
        private bool RoundIsStart = false;
        private Player IntercomPlayerSpeek;
        public static Player Use914;
        public EventHandlers(DiscordLog plugin) => this.plugin = plugin;
        public void OnWaintingForPlayers()
        {
            foreach (CoroutineHandle handle in Coroutines)
                Timing.KillCoroutines(handle);
            plugin.NormalisedName.Clear();
            if (DiscordLog.Instance.Config.WebhookUrlLogError != string.Empty)
                Coroutines.Add(Timing.RunCoroutine(plugin.RunSendLogError()));
            if (DiscordLog.Instance.Config.WebhookUrlLogJoueur != string.Empty)
                Coroutines.Add(Timing.RunCoroutine(plugin.RunSendWebhook(), Segment.RealtimeUpdate));
            if (DiscordLog.Instance.Config.WebhookSi != "null" || DiscordLog.Instance.Config.IdMessage != "null" )
                Coroutines.Add(Timing.RunCoroutine(plugin.RunUpdateWebhook(), Segment.RealtimeUpdate));
            plugin.LOG += ":zzz: En attente de joueurs...\n";
            RoundIsStart = false;
        }
        public void OnRoundStart()
        {
            RoundIsStart = true;
            Timing.CallDelayed(0.5f, () =>
            {
                string RoundStart = $":triangular_flag_on_post: Démarrage de la partie avec {Player.List.Where((p) => p.Role != RoleType.None).Count()} joueurs.\n";
                foreach (Player player in Player.List.Where((p) => p.Role != RoleType.None).OrderBy(x => x.Role.Team))
                    if (player.TryGetSessionVariable("NewRole", out Tuple<string, string> newrole))
                        RoundStart += $"    - {Extensions.LogPlayer(player)} a spawn en {newrole.Item1}.\n";
                    else
                        RoundStart += $"    - {Extensions.LogPlayer(player)} a spawn en {player.Role}.\n";
                plugin.LOG += RoundStart;
            });
        }
        public void OnRoundEnd(RoundEndedEventArgs ev)
        {
            plugin.LOG += $":checkered_flag: Fin de la partie.\n" +
                $"```Win: {ev.LeadingTeam}\nClass-D échappé{(ev.ClassList.class_ds <= 1 ? "" : "s")}: {ev.ClassList.class_ds}\n" +
                $"Scientifique{(ev.ClassList.scientists <= 1 ? "" : "s")} sauvé: {ev.ClassList.scientists}\n" +
                $"SCPs restant{(ev.ClassList.scps_except_zombies <= 1 ? "" : "s")}: {ev.ClassList.scps_except_zombies}\n" +
                $"{(Warhead.IsDetonated ? $"Le site a explosé\nMort par la warhead: {ev.ClassList.warhead_kills}" : "Le site n'a pas explosé")}```\n";
        }
        public void OnRoundRestart()
        {
            plugin.LOG += ":cyclone: Redémarrage de la partie.\n";
        }
        public void OnTeamRespawn(RespawningTeamEventArgs ev)
        {
            if (!ev.IsAllowed) return;
            if (ev.Players.Count() == 0) return;
            string objcontent;
            if (ev.NextKnownTeam == SpawnableTeamType.NineTailedFox)
            {
                objcontent = ":helicopter: L’équipe Epsilon est arrivée sur le site.\n";
                foreach (Player playerrespawn in ev.Players)
                {
                    objcontent += $"    - {Extensions.LogPlayer(playerrespawn)}\n";
                }
            }
            else if (ev.NextKnownTeam == SpawnableTeamType.ChaosInsurgency)
            {
                objcontent = ":articulated_lorry: L’Insurrection du Chaos est arrivée sur le site.\n";
                foreach (Player playerrespawn in ev.Players)
                {
                    objcontent += $"    - {Extensions.LogPlayer(playerrespawn)}\n";
                }
            }
            else
            {
                objcontent = ":snake: La Main du Serpent est arrivée sur le site.\n";
                foreach (Player playerrespawn in ev.Players)
                {
                    objcontent += $"    - {Extensions.LogPlayer(playerrespawn)}\n";
                }
            }
            plugin.LOG += objcontent;
        }
        public void OnWarheadStart(StartingEventArgs ev)
        {
            if (ev.IsAllowed && ev.Player is not null)
                plugin.LOG += $":radioactive: {Extensions.LogPlayer(ev.Player)} a déclenché la détonation de l'Alpha Warhead.\n";
        }
        public void OnWarheadCancel(StoppingEventArgs ev)
        {
            if (ev.IsAllowed && ev.Player is not null)
                plugin.LOG += $":radioactive: {Extensions.LogPlayer(ev.Player)} a désactivé la détonation de l’Alpha Warhead.\n";
        }
        public void OnDetonated()
        {
            plugin.LOG += ":radioactive: Explosion du site.\n";
        }
        public void OnDecontaminating(DecontaminatingEventArgs ev)
        {
            if (ev.IsAllowed)
                plugin.LOG += ":biohazard: Décontamination de la LCZ.\n";
        }
        public void OnGeneratorFinish(GeneratorActivatedEventArgs ev)
        {
            if (ev.IsAllowed)
                plugin.LOG += $":computer: Le générateur dans la {ev.Generator.Room.Type} est activé.\n";
        }
        public void OnPlayerAuth(PreAuthenticatingEventArgs ev)
        {
            if (ev.UserId.EndsWith("@steam"))
            {
                if (SteamNickName.TryGetValue(ev.UserId, out string NickName))
                    plugin.LOGStaff += $":flag_{ev.Country.ToLower()}: ``{NickName}`` ({Extensions.ConvertID(ev.UserId)}) ||{ev.Request.RemoteEndPoint}|| tente une connexion sur le serveur.\n";
                else
                {
                    NickName = Extensions.GetUserName(ev.UserId.Replace("@steam", string.Empty));
                    SteamNickName.Add(ev.UserId, NickName);
                    plugin.LOGStaff += $":flag_{ev.Country.ToLower()}: ``{NickName}`` ({Extensions.ConvertID(ev.UserId)}) ||{ev.Request.RemoteEndPoint}|| tente une connexion sur le serveur.\n";
                }
            }
            else
                plugin.LOGStaff += $":flag_{ev.Country.ToLower()}: {Extensions.ConvertID(ev.UserId)} ||{ev.Request.RemoteEndPoint}|| tente une connexion sur le serveur.\n";
        }
        public void OnPlayerVerified(VerifiedEventArgs ev)
        {
            plugin.LOG += $":chart_with_upwards_trend: {Extensions.LogPlayer(ev.Player)} [{ev.Player.Id}] a rejoint le serveur.\n";
            string PlayerName = ev.Player.Nickname.Normalize(System.Text.NormalizationForm.FormKD);
            if (PlayerName.Length < 18)
              plugin.NormalisedName.Add(ev.Player, $"[{ev.Player.Id}] {PlayerName}");
            else
              plugin.NormalisedName.Add(ev.Player, $"[{ev.Player.Id}] {PlayerName.Remove(17)}");
        }
        public void OnPlayerDestroying(DestroyingEventArgs ev)
        {
            if (ev.Player is null) return;
            plugin.LOG += $":chart_with_downwards_trend: {Extensions.LogPlayer(ev.Player)} a quitté le serveur.\n";
            plugin.NormalisedName.Remove(ev.Player);
        }
        public void OnChangingRole(ChangingRoleEventArgs ev)
        {
            if (!RoundIsStart || ev.Player is null || ev.Reason == SpawnReason.Died || ev.Reason == SpawnReason.Revived || ev.Reason == SpawnReason.RoundStart || !ev.IsAllowed) return;
            float TimeAlive = ev.Player.ReferenceHub.characterClassManager.AliveTime;
            Timing.CallDelayed(0.25f, () =>
            {
                if (ev.Player.TryGetSessionVariable("NewRole", out Tuple<string, string> newrole))
                    plugin.LOG += $":new: {Extensions.LogPlayer(ev.Player)} a spawn en tant que : {newrole.Item1}.\n";
                else if (ev.Reason == SpawnReason.Escaped)
                    if (ev.Player.IsCuffed)
                        plugin.LOG += $":chains: {Extensions.LogPlayer(ev.Player)} a été escorté en {TimeAlive / 60:00}:{TimeAlive % 60:00}. Il est devenu : {ev.NewRole}.\n";
                    else
                        plugin.LOG += $":person_running: {Extensions.LogPlayer(ev.Player)} s'est échapé en {TimeAlive / 60:00}:{TimeAlive % 60:00}. Il est devenu : {ev.NewRole}.\n";
                else
                    plugin.LOG += $":new: {Extensions.LogPlayer(ev.Player)} a spawn en tant que : {ev.NewRole}.\n";
            });
        }

        public void OnPlayerDeath(DiedEventArgs ev)
        {
            if (ev.Target.Role == RoleType.None || !RoundIsStart) return;

            string DamageString = ev.Handler.Type.ToString();

            if (ev.Handler.Type == DamageType.Custom && ev.Handler.Base is CustomReasonDamageHandler customReason)
            {
                DamageString = customReason.ServerLogsText.Remove(0,30);
                if (DamageString == "Disconect") 
                    return;
            }

            if (ev.Killer is not null && ev.Killer != ev.Target)
            {
                plugin.LOG += $":skull: {Extensions.LogPlayer(ev.Target)} est mort par {Extensions.LogPlayer(ev.Killer)} avec {DamageString}.\n";
            }
            else
            {
                plugin.LOG += $":skull: {Extensions.LogPlayer(ev.Target)} est mort par {DamageString}.\n";
            }
        }
        public void OnDroppingItem(DroppingItemEventArgs ev)
        {
            if (ev.IsAllowed && ev.Player is not null)
                if (ev.Item.Type == ItemType.SCP330)
                {
                    plugin.LOG += $":outbox_tray: {Extensions.LogPlayer(ev.Player)} a jeté SCP330: ";
                    ev.Item.Base.TryGetComponent<Scp330Bag>(out var comp);
                    foreach (var candy in comp.Candies)
                    {
                        plugin.LOG += $"\n  - {candy}";
                    }
                    plugin.LOG += "\n";
                }
                else
                    plugin.LOG += $":outbox_tray: {Extensions.LogPlayer(ev.Player)} a jeté {Extensions.LogItem(ev.Item)}.\n";
        }
        public void OnPickingUpItem(PickingUpItemEventArgs ev)
        {
            if (ev.IsAllowed && ev.Player is not null)
                    plugin.LOG += $":inbox_tray: {Extensions.LogPlayer(ev.Player)} a récupéré {Extensions.LogPickup(ev.Pickup)}.\n";
        }
        public void OnPickingUpArmor(PickingUpArmorEventArgs ev)
        {
            if (ev.IsAllowed && ev.Player is not null)
                plugin.LOG += $":inbox_tray: {Extensions.LogPlayer(ev.Player)} a récupéré {ev.Pickup.Type}.\n";
        }

        public void OnEatenScp330(EatenScp330EventArgs ev)
        {
            if (ev.Player is not null)
                plugin.LOG += $":candy: {Extensions.LogPlayer(ev.Player)} a manger un bonbon : {ev.Candy.Kind}.\n";
        }

        public void OnPlayerUsedItem(UsedItemEventArgs ev)
        {
            if (Exiled.API.Extensions.ItemExtensions.IsMedical(ev.Item.Type))
                plugin.LOG += $":adhesive_bandage: {Extensions.LogPlayer(ev.Player)} s'est soigné avec {ev.Item.Type}.\n";
            else
                plugin.LOG += ev.Item.Type switch
                {
                    ItemType.SCP207 => $"<:ContaCola:881985143718445086> {Extensions.LogPlayer(ev.Player)} a utilisé SCP207.\n",
                    ItemType.SCP268 => $"<:Chepeaux:697574292140982313> {Extensions.LogPlayer(ev.Player)} a utilisé SCP268.\n",
                    ItemType.SCP1853 => $"<:Scp1853:963526275216064572> {Extensions.LogPlayer(ev.Player)} a utilisé SCP1853.\n",
                    _ => $":??: {Extensions.LogPlayer(ev.Player)} a utilisé {ev.Item.Type}.\n",
                };
        }
        public void OnGeneratorUnlock(UnlockingGeneratorEventArgs ev)
        {
            if (ev.IsAllowed && ev.Player is not null)
                plugin.LOG += $":computer: {Extensions.LogPlayer(ev.Player)} a débloqué un générateur dans la salle : {ev.Generator.Room.Type}.\n";
        }
        public void OnStoppingGenerator(StoppingGeneratorEventArgs ev)
        {
            
            if (ev.IsAllowed && ev.Player is not null && ev.Generator.IsActivating)
                plugin.LOG += $":computer: {Extensions.LogPlayer(ev.Player)} a désactivé un générateur de la salle : {ev.Generator.Room.Type}.\n";
        }
        public void OnActivatingGenerator(ActivatingGeneratorEventArgs ev)
        {
            if (ev.IsAllowed && ev.Player is not null)
                plugin.LOG += $":computer: {Extensions.LogPlayer(ev.Player)} a activé un générateur de la salle : {ev.Generator.Room.Type}.\n";
        }
        public void OnActivatingWarheadPanel(ActivatingWarheadPanelEventArgs ev)
        {
            if (!ev.IsAllowed && ev.Player is null) return;

            if (!UnityEngine.Object.FindObjectOfType<AlphaWarheadOutsitePanel>().keycardEntered)
                plugin.LOG += $":radioactive: {Extensions.LogPlayer(ev.Player)} a ouvert la protection pour activé l'Alpha Warhead.\n";
            else if (UnityEngine.Object.FindObjectOfType<AlphaWarheadOutsitePanel>().keycardEntered)
                plugin.LOG += $":radioactive: {Extensions.LogPlayer(ev.Player)} a fermer la protection pour activé l'Alpha Warhead.\n";
        }
        public void OnLocalReporting(LocalReportingEventArgs ev)
        {
            if (DiscordLog.Instance.Config.WebhookReport != "none")
                _ = Webhook.ReportAsync(ev.Issuer, ev.Target, DiscordLog.Instance.Config.WebhookReport, DiscordLog.Instance.Config.Ping, ev.Reason);
            ev.IsAllowed = true;
        }

        public void OnIntercomSpeaking(IntercomSpeakingEventArgs ev)
        {
            if (ev.IsAllowed && IntercomPlayerSpeek != ev.Player && Exiled.API.Features.Intercom.Speaker == ev.Player)
            {
                IntercomPlayerSpeek = ev.Player;
                plugin.LOG += $":loudspeaker: {Extensions.LogPlayer(ev.Player)} utilise l'intercom.\n";
            }
            else if (ev.Player is null)
                IntercomPlayerSpeek = null;
        }
        public void OnHandcuffing(HandcuffingEventArgs ev)
        {
            if (ev.IsAllowed && ev.Cuffer is not null)
                plugin.LOG += $":chains: {Extensions.LogPlayer(ev.Target)} a été menoté par {Extensions.LogPlayer(ev.Cuffer)}.\n";
        }
        public void OnRemovingHandcuffs(RemovingHandcuffsEventArgs ev)
        {
            if (ev.IsAllowed)
                if (ev.Cuffer is not null)
                    plugin.LOG += $":chains: {Extensions.LogPlayer(ev.Target)} a été démenoté par {Extensions.LogPlayer(ev.Cuffer)}.\n";
                else
                    plugin.LOG += $":chains: {Extensions.LogPlayer(ev.Target)} a été démenoté.\n";
        }
        public void OnEnteringPocketDimension(EnteringPocketDimensionEventArgs ev)
        {
            if (ev.IsAllowed && ev.Player is not null)
                plugin.LOG += $":hole: {Extensions.LogPlayer(ev.Player)} est entré dans la dimension de poche.\n";
        }
        public void OnEscapingPocketDimension(EscapingPocketDimensionEventArgs ev)
        {
            if (ev.IsAllowed && ev.Player is not null)
                plugin.LOG += $":hole: {Extensions.LogPlayer(ev.Player)} a échappé a la dimension de poche.\n";
        }
        public void On914Activating(ActivatingEventArgs ev)
        {
            if (ev.IsAllowed && ev.Player is not null)
                Use914 = ev.Player;
        }
        
        public void OnFinishingRecall(FinishingRecallEventArgs ev)
        {
            if (ev.IsAllowed && ev.Target is not null && ev.Scp049 is not null)
                plugin.LOG += $":zombie: {Extensions.LogPlayer(ev.Target)} a été ressuscité en Scp049-2 par {Extensions.LogPlayer(ev.Scp049)}.\n";
        }
        public void OnBanning(BanningEventArgs ev)
        {
            if (ev.IsAllowed && ev.Target is not null && ev.Issuer is not null)
            {
                _ = Webhook.BanPlayerAsync(ev.Issuer, ev.Target, ev.Reason, ev.Duration);
                plugin.LOGStaff += $":hammer: {Extensions.LogPlayer(ev.Target)} a été banni pour :``{ev.Reason}`` ; pendant {ev.Duration} secondes par {Extensions.LogPlayer(ev.Issuer)}.\n";
            } 
        }
        public void OnKicking(KickingEventArgs ev)
        {
            if (ev.IsAllowed && ev.Target is not null && ev.Issuer is not null)
            {
                plugin.LOGStaff += $":mans_shoe: {Extensions.LogPlayer(ev.Target)} a été kick pour : ``{ev.Reason}`` ; par {Extensions.LogPlayer(ev.Issuer)}.\n";
                _ = Webhook.KickPlayerAsync(ev.Issuer, ev.Target, ev.Reason);
            }
        }
        public void OnBanned(BannedEventArgs ev)
        {
            if (ev.Details.OriginalName != "Unknown - offline ban") return;

            string TargetNick = "Unknown";
            if (ev.Details.Id.EndsWith("@steam"))
            {
                TargetNick = Extensions.GetUserName(ev.Details.Id);
            }
            plugin.LOGStaff += $":hammer: ``{TargetNick}`` ({Extensions.ConvertID(ev.Details.Id)}) a été Oban pour : ``{ev.Details.Reason}`` ; par {Extensions.LogPlayer(ev.Issuer)}.\n";

            _ = Webhook.OBanPlayerAsync(ev.Issuer, TargetNick, ev.Details.Id, ev.Details.Reason,
                long.TryParse(TimeSpan.FromTicks(ev.Details.Expires - ev.Details.IssuanceTime).TotalSeconds.ToString(CultureInfo.InvariantCulture), out var timelong) ? timelong : -1);
        }
    }
}

