using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Exiled.API.Features.Items;
using Exiled.Events.EventArgs.Map;
using Exiled.Events.EventArgs.Player;
using Exiled.Events.EventArgs.Scp049;
using Exiled.Events.EventArgs.Scp244;
using Exiled.Events.EventArgs.Scp330;
using Exiled.Events.EventArgs.Scp914;
using Exiled.Events.EventArgs.Server;
using Exiled.Events.EventArgs.Warhead;
using GameCore;
using InventorySystem.Items.Usables.Scp330;
using MEC;
using PlayerRoles;
using PlayerStatsSystem;
using Respawning;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using static System.Net.Mime.MediaTypeNames;
using Log = Exiled.API.Features.Log;
using Scp330Pickup = Exiled.API.Features.Pickups.Scp330Pickup;

namespace DiscordLog
{
    public class EventHandlers
    {
        internal readonly DiscordLog plugin;
        public static List<CoroutineHandle> Coroutines = new();
        private static readonly Dictionary<string, string> SteamNickName = new();
        private Player IntercomPlayerSpeek;
        public static Player Use914;
        public EventHandlers(DiscordLog plugin) => this.plugin = plugin;
        public void OnWaintingForPlayers()
        {
            foreach (CoroutineHandle handle in Coroutines)
                Timing.KillCoroutines(handle);
            plugin.NormalisedName.Clear();
            if (!string.IsNullOrWhiteSpace(DiscordLog.Instance.Config.WebhookUrlLogError))
                Coroutines.Add(Timing.RunCoroutine(plugin.RunSendLogError()));
            if (!string.IsNullOrWhiteSpace(DiscordLog.Instance.Config.WebhookUrlLogJoueur))
                Coroutines.Add(Timing.RunCoroutine(plugin.RunSendWebhook(), Segment.RealtimeUpdate));
            if (!string.IsNullOrWhiteSpace(DiscordLog.Instance.Config.WebhookUrlLogStaff))
                Coroutines.Add(Timing.RunCoroutine(plugin.RunSendWebhookStaff(), Segment.RealtimeUpdate));
            if (!string.IsNullOrWhiteSpace(DiscordLog.Instance.Config.WebhookSi) && !string.IsNullOrWhiteSpace(DiscordLog.Instance.Config.IdMessage))
                Coroutines.Add(Timing.RunCoroutine(plugin.RunUpdateWebhook(), Segment.RealtimeUpdate));
            plugin.LOG += ":zzz: En attente de joueurs...\n";
            plugin.LOGStaff += $"Server Start \nExiled Version {Exiled.Loader.Loader.Version} | SCP:SL Version {Server.Version} | Seed {Map.Seed}";
        }
        public void OnRoundStart()
        {
            plugin.LOG += $":triangular_flag_on_post: Démarrage de la partie avec ({Server.PlayerCount}) joueurs.\n";
            foreach (Player player in Player.List.OrderBy(x => x.Role.Team))
            {
                plugin.LOG += $"    - {Extensions.LogPlayer(player)} a spawn en tant que : {player.Role.Type}.\n";
            }
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
            if (!ev.IsAllowed || !ev.Players.Any()) return;
            string objcontent = ":snake: La Main du Serpent est arrivée sur le site.\n";

            if (ev.NextKnownTeam is SpawnableTeamType.NineTailedFox)
                objcontent = ":helicopter: L’équipe Epsilon est arrivée sur le site.\n";
            else if (ev.NextKnownTeam is SpawnableTeamType.ChaosInsurgency)
                objcontent = ":articulated_lorry: L’Insurrection du Chaos est arrivée sur le site.\n";

            plugin.LOG += objcontent;
        }
        public void OnWarheadStart(StartingEventArgs ev)
        {
            if (!ev.IsAllowed)
                return;
            plugin.LOG += $":radioactive: {Extensions.LogPlayer(ev.Player)} a déclenché la détonation de l'Alpha Warhead.\n";
        }
        public void OnWarheadCancel(StoppingEventArgs ev)
        {
            if (!ev.IsAllowed)
                return;
            plugin.LOG += $":radioactive: {Extensions.LogPlayer(ev.Player)} a désactivé la détonation de l’Alpha Warhead.\n";
        }
        public void OnDetonated()
        {
            plugin.LOG += ":radioactive: Explosion du site.\n";
        }
        public void OnDecontaminating(DecontaminatingEventArgs ev)
        {
            if (!ev.IsAllowed)
                return;
            plugin.LOG += ":biohazard: Décontamination de la LCZ.\n";
        }
        public void OnGeneratorFinish(GeneratorActivatedEventArgs ev)
        {
            if (!ev.IsAllowed)
                return;
            plugin.LOG += $":computer: Le générateur dans la {ev.Generator.Room.Type} est activé.\n";
        }
        public void OnPlayerAuth(PreAuthenticatingEventArgs ev)
        {
            if (ev.UserId.EndsWith("@steam"))
            {
                if (SteamNickName.TryGetValue(ev.UserId, out string NickName))
                {
                    plugin.LOGStaff += $":flag_{ev.Country.ToLower()}: ``{NickName}`` ({Extensions.ConvertID(ev.UserId)}) ||{ev.Request.RemoteEndPoint}|| tente une connexion sur le serveur.\n";
                    plugin.LOG += $":flag_{ev.Country.ToLower()}: ``{NickName}`` ({Extensions.ConvertID(ev.UserId)}) tente une connexion sur le serveur.\n";
                    return;
                }
                NickName = Extensions.GetUserName(ev.UserId.Replace("@steam", string.Empty));
                SteamNickName.Add(ev.UserId, NickName);
                plugin.LOGStaff += $":flag_{ev.Country.ToLower()}: ``{NickName}`` ({Extensions.ConvertID(ev.UserId)}) ||{ev.Request.RemoteEndPoint}|| tente une connexion sur le serveur.\n";
                plugin.LOG += $":flag_{ev.Country.ToLower()}: ``{NickName}`` ({Extensions.ConvertID(ev.UserId)}) tente une connexion sur le serveur.\n";
                return;
            }
            plugin.LOGStaff += $":flag_{ev.Country.ToLower()}: ({Extensions.ConvertID(ev.UserId)}) ||{ev.Request.RemoteEndPoint}|| tente une connexion sur le serveur.\n";
            plugin.LOG += $":flag_{ev.Country.ToLower()}: ({Extensions.ConvertID(ev.UserId)}) tente une connexion sur le serveur.\n";
        }
        public void OnPlayerVerified(VerifiedEventArgs ev)
        {
            plugin.LOG += $":chart_with_upwards_trend: {Extensions.LogPlayer(ev.Player)} [{ev.Player.Id}] a rejoint le serveur. ({Server.PlayerCount}/{Server.MaxPlayerCount})\n";
            string PlayerName = ev.Player.Nickname.Normalize(System.Text.NormalizationForm.FormKD);
            if (PlayerName.Length < 18)
              plugin.NormalisedName.Add(ev.Player, $"[{ev.Player.Id}] {PlayerName}");
            else
              plugin.NormalisedName.Add(ev.Player, $"[{ev.Player.Id}] {PlayerName.Remove(17)}");
        }
        public void OnPlayerDestroying(DestroyingEventArgs ev)
        {
            plugin.LOG += $":chart_with_downwards_trend: {Extensions.LogPlayer(ev.Player)} a quitté le serveur. ({Server.PlayerCount - 1}/{Server.MaxPlayerCount})\n";
            plugin.NormalisedName.Remove(ev.Player);
        }
        public void OnSpawned(SpawnedEventArgs ev)
        {
            if (ev.Reason is SpawnReason.Died or SpawnReason.Revived or SpawnReason.RoundStart or SpawnReason.Destroyed) return;
            string NewRole = ev.Player.Role.Type.ToString();
            if (ev.Player.TryGetSessionVariable("NewRole", out Tuple<string, string> NewCustomRole))
                NewRole = NewCustomRole.Item1;

            switch (ev.Reason)
            {
                case SpawnReason.Escaped:
                    double TimeAlive = RoundStart.RoundStartTimer.Elapsed.TotalSeconds;

                    if (ev.Player.IsCuffed)
                    {
                        plugin.LOG += $":chains: {Extensions.LogPlayer(ev.Player)} a été escorté en {TimeAlive / 60:00}:{TimeAlive % 60:00}. Il est devenu : {NewRole}.\n";
                        return;
                    }
                    plugin.LOG += $":person_running: {Extensions.LogPlayer(ev.Player)} s'est échapé en {TimeAlive / 60:00}:{TimeAlive % 60:00}. Il est devenu : {NewRole}.\n";
                    return;

                case SpawnReason.Respawn:
                    plugin.LOG += $"    - {Extensions.LogPlayer(ev.Player)} a spawn en tant que : {NewRole}.\n";
                    return;

            }
            plugin.LOG += $":new: {Extensions.LogPlayer(ev.Player)} a spawn car {ev.Reason} en tant que : {NewRole}.\n";
        }

        public void OnPlayerDeath(DiedEventArgs ev)
        {
            if (ev.DamageHandler.Type is DamageType.Unknown)
            {
                Log.Error($"Damage Unknown Trigger {ev.DamageHandler.Base} {ev.Attacker?.CurrentItem?.Type}");
                plugin.LOG += $":warning::warning: {ev.DamageHandler.Base} :warning::warning:\n";
            }

            string DamageString = ev.DamageHandler.Type.ToString();

            if (ev.DamageHandler.Type is DamageType.Custom && ev.DamageHandler.Base is CustomReasonDamageHandler customReason)
            {
                DamageString = customReason.ServerLogsText.Remove(0, 30);
                if (DamageString is "Disconect")
                    return;
            }

            if (ev.Attacker is null || ev.Attacker == ev.Player)
            {
                plugin.LOG += $":skull: {Extensions.LogPlayer(ev.Player)} est mort par {DamageString}.\n";
                return;
            }
            plugin.LOG += $":skull: {Extensions.LogPlayer(ev.Player)} est mort par {Extensions.LogPlayer(ev.Attacker)} avec {DamageString}.\n";
        }
        public void OnDroppingItem(DroppingItemEventArgs ev)
        {
            if (!ev.IsAllowed)
                return;

            if (ev.Item is Scp330 scp330)
            {
                string objcontent = $":outbox_tray: {Extensions.LogPlayer(ev.Player)} a jeté SCP330:\n";
                foreach (CandyKindID candy in scp330.Candies)
                {
                    objcontent += $"  - {candy}\n";
                }
                plugin.LOG += objcontent;
                return;
            }
            plugin.LOG += $":outbox_tray: {Extensions.LogPlayer(ev.Player)} a jeté {Extensions.LogItem(ev.Item)}.\n";
        }
        public void OnPickingUpItem(PickingUpItemEventArgs ev)
        {
            if (!ev.IsAllowed || ev.Pickup.Type.IsAmmo())
                return;

            if (ev.Pickup is Scp330Pickup scp330)
            {
                plugin.LOG += $":inbox_tray: {Extensions.LogPlayer(ev.Player)} a récupéré ses bonbon :\n";
                foreach (CandyKindID Candy in scp330.Candies)
                {
                    plugin.LOG += $"  - {Candy}\n";
                }
                return;
            }
        
            plugin.LOG += $":inbox_tray: {Extensions.LogPlayer(ev.Player)} a récupéré {Extensions.LogPickup(ev.Pickup)}.\n";
        }
        public void OnDroppingUpScp330(DroppingScp330EventArgs ev)
        {
            if (!ev.IsAllowed)
                return;

            plugin.LOG += $":inbox_tray: {Extensions.LogPlayer(ev.Player)} a jeté un bonbon : {ev.Candy}\n";
        }
        public void OnEatenScp330(EatenScp330EventArgs ev)
        {
            plugin.LOG += $":candy: {Extensions.LogPlayer(ev.Player)} a manger un bonbon : {ev.Candy.Kind}.\n";
        }
        public void OnInteractingScp330(InteractingScp330EventArgs ev)
        {
            if (!ev.IsAllowed || ev.Player is null)
                return;

            plugin.LOG += $":inbox_tray: {Extensions.LogPlayer(ev.Player)} a récolté un bonbon : {ev.Candy}\n";
        }
        public void OnOpeningScp244(OpeningScp244EventArgs ev)
        {
            if (ev.IsAllowed)
                plugin.LOG += $":teapot: {ev.Pickup.Type} a été ouvert par {Extensions.LogPlayer(ev.Pickup.PreviousOwner)} : {ev.Pickup.Room?.Type ?? RoomType.Unknown}.\n";
        }
        public void OnDamagingScp244(DamagingScp244EventArgs ev)
        {
            if (ev.IsAllowed)
                plugin.LOG += $":teapot: {ev.Pickup.Type} a été cassé par {Extensions.LogPlayer(ev.Pickup.PreviousOwner)} avec {ev.Handler.Type} : {ev.Pickup.Room?.Type ?? RoomType.Unknown}\n";
        }
        public void OnUsingScp244(UsingScp244EventArgs ev)
        {
            if (ev.IsAllowed)
                plugin.LOG += $":teapot: {Extensions.LogPlayer(ev.Player)} a ouvert {ev.Scp244.Type} : {ev.Player.CurrentRoom?.Type}.\n";
        }
        public void OnExplodingGrenade(ExplodingGrenadeEventArgs ev)
        {
            if (ev.IsAllowed && ev.Projectile.ProjectileType is ProjectileType.Scp2176)
                plugin.LOG += $"<:SCP2176:963534500120383539> SCP2176 a été cassé par {Extensions.LogPlayer(ev.Player)} : {ev.Projectile.Room?.Type ?? RoomType.Unknown}.\n";
        }
        public void OnPlayerUsedItem(UsedItemEventArgs ev)
        {
            if (ItemExtensions.IsMedical(ev.Item.Type))
            {
                plugin.LOG += $":adhesive_bandage: {Extensions.LogPlayer(ev.Player)} s'est soigné avec {ev.Item.Type}.\n";
                return;
            }
            plugin.LOG += ev.Item.Type switch
            {
                ItemType.SCP207 => $"<:ContaCola:881985143718445086> {Extensions.LogPlayer(ev.Player)} a utilisé SCP207.\n",
                ItemType.SCP268 => $"<:Chepeaux:697574292140982313> {Extensions.LogPlayer(ev.Player)} a utilisé SCP268.\n",
                ItemType.SCP1853 => $"<:Scp1853:963526275216064572> {Extensions.LogPlayer(ev.Player)} a utilisé SCP1853.\n",
                ItemType.SCP1576 => $"<:SCP1576:1069063548489191568> {Extensions.LogPlayer(ev.Player)} a utilisé SCP1576.\n",
                _ => $":??: {Extensions.LogPlayer(ev.Player)} a utilisé {ev.Item.Type}.\n",
            };
        }
        public void OnGeneratorUnlock(UnlockingGeneratorEventArgs ev)
        {
            if (!ev.IsAllowed)
                return;
            plugin.LOG += $":computer: {Extensions.LogPlayer(ev.Player)} a débloqué un générateur dans la salle : {ev.Generator.Room.Type}.\n";
        }
        public void OnStoppingGenerator(StoppingGeneratorEventArgs ev)
        {
            if (!ev.IsAllowed)
                return;
            plugin.LOG += $":computer: {Extensions.LogPlayer(ev.Player)} a désactivé un générateur de la salle : {ev.Generator.Room.Type}.\n";
        }
        public void OnActivatingGenerator(ActivatingGeneratorEventArgs ev)
        {
            if (!ev.IsAllowed)
                return;
            plugin.LOG += $":computer: {Extensions.LogPlayer(ev.Player)} a activé un générateur de la salle : {ev.Generator.Room.Type}.\n";
        }
        public void OnActivatingWarheadPanel(ActivatingWarheadPanelEventArgs ev)
        {
            if (!ev.IsAllowed)
                return;

            if (Warhead.IsKeycardActivated)
            {
                plugin.LOG += $":radioactive: {Extensions.LogPlayer(ev.Player)} a fermer la protection pour activé l'Alpha Warhead.\n";
                return;
            }
            plugin.LOG += $":radioactive: {Extensions.LogPlayer(ev.Player)} a ouvert la protection pour activé l'Alpha Warhead.\n";
        }
        public void OnLocalReporting(LocalReportingEventArgs ev)
        {
            if (DiscordLog.Instance.Config.WebhookReport != "none")
                Webhook.ReportAsync(ev.Player, ev.Target, ev.Reason);
            ev.IsAllowed = true;
        }

        public void OnIntercomSpeaking(IntercomSpeakingEventArgs ev)
        {
            if (ev.Player is null)
            {
                IntercomPlayerSpeek = null; // I would love to add all the voice :) in an audio files
                return;
            }

            if (ev.IsAllowed && IntercomPlayerSpeek != ev.Player)
            {
                IntercomPlayerSpeek = ev.Player;
                plugin.LOG += $":loudspeaker: {Extensions.LogPlayer(ev.Player)} utilise l'intercom.\n";
                return;
            }
        }
        public void OnHandcuffing(HandcuffingEventArgs ev)
        {
            if (!ev.IsAllowed)
                return;
            plugin.LOG += $":chains: {Extensions.LogPlayer(ev.Target)} a été menoté par {Extensions.LogPlayer(ev.Player)}.\n";
        }
        public void OnRemovingHandcuffs(RemovingHandcuffsEventArgs ev)
        {
            if (!ev.IsAllowed)
                return;
            if (ev.Player is null)
            {
                plugin.LOG += $":chains: {Extensions.LogPlayer(ev.Target)} a été démenoté.\n";
                return;
            }
            plugin.LOG += $":chains: {Extensions.LogPlayer(ev.Target)} a été démenoté par {Extensions.LogPlayer(ev.Player)}.\n";
        }
        public void OnEnteringPocketDimension(EnteringPocketDimensionEventArgs ev)
        {
            if (ev.IsAllowed)
                plugin.LOG += $":hole: {Extensions.LogPlayer(ev.Player)} est entré dans la dimension de poche.\n";
        }
        public void OnEscapingPocketDimension(EscapingPocketDimensionEventArgs ev)
        {
            if (ev.IsAllowed)
                plugin.LOG += $":hole: {Extensions.LogPlayer(ev.Player)} a échappé a la dimension de poche.\n";
        }
        public void On914Activating(ActivatingEventArgs ev)
        {
            if (ev.IsAllowed)
                Use914 = ev.Player;
        }
        
        public void OnFinishingRecall(FinishingRecallEventArgs ev)
        {
            if (ev.IsAllowed)
                plugin.LOG += $":zombie: {Extensions.LogPlayer(ev.Target)} a été ressuscité en Scp049-2 par {Extensions.LogPlayer(ev.Player)}.\n";
        }
        public void OnBanning(BanningEventArgs ev)
        {
            if (!ev.IsAllowed) 
                return;
            if (ev.Target != null)
            {
                BanHandler.IssueBan(new BanDetails
                {
                    OriginalName = ev.Target.Sender.LogName,
                    Id = ev.Target.Connection.address,
                    IssuanceTime = TimeBehaviour.CurrentTimestamp(),
                    Expires = ev.Duration,
                    Reason = ev.Reason,
                    Issuer = ev.Player.Sender.LogName
                }, BanHandler.BanType.IP, false);
            }
            Webhook.BanPlayerAsync(ev.Player, ev.Target, ev.Reason, ev.Duration);
            plugin.LOGStaff += $":hammer: {Extensions.LogPlayer(ev.Target)} a été banni pour :``{ev.Reason}`` ; pendant {ev.Duration} secondes par {Extensions.LogPlayer(ev.Player)}.\n";
        }
        public void OnKicking(KickingEventArgs ev)
        {
            if (!ev.IsAllowed) 
                return;
            plugin.LOGStaff += $":mans_shoe: {Extensions.LogPlayer(ev.Target)} a été kick pour : ``{ev.Reason}`` ; par {Extensions.LogPlayer(ev.Player)}.\n";
            if (ev.Reason.ToLower().RemoveSpaces() is not "afk")
                Webhook.KickPlayerAsync(ev.Player, ev.Target, ev.Reason);
        }
        public void OnBanned(BannedEventArgs ev)
        {
            if (ev.Details.OriginalName != "Unknown - offline ban") 
                return;

            string TargetNick = "Unknown";
            if (ev.Details.Id.EndsWith("@steam"))
            {
                TargetNick = Extensions.GetUserName(ev.Details.Id);
            }
            plugin.LOGStaff += $":hammer: ``{TargetNick}`` ({Extensions.ConvertID(ev.Details.Id)}) a été Oban pour : ``{ev.Details.Reason}`` ; par {Extensions.LogPlayer(ev.Player)}.\n";

            Webhook.OBanPlayerAsync(ev.Player, TargetNick, ev.Details.Id, ev.Details.Reason,
            long.TryParse(TimeSpan.FromTicks(ev.Details.Expires - ev.Details.IssuanceTime).TotalSeconds.ToString(CultureInfo.InvariantCulture), out long timelong) ? timelong : -1);
        }
    }
}

