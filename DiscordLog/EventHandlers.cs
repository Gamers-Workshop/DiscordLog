using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.Events.EventArgs;
using InventorySystem.Items.Firearms;
using InventorySystem.Items.MicroHID;
using InventorySystem.Items.Radio;
using MEC;
using Newtonsoft.Json.Serialization;
using NorthwoodLib;
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

        private bool RoundIsStart = false;
        private Player IntercomPlayerSpeek;
        public static Player Use914;
        public EventHandlers(DiscordLog plugin) => this.plugin = plugin;
        public void OnWaintingForPlayers()
        {
            foreach (CoroutineHandle handle in Coroutines)
                Timing.KillCoroutines(handle);
            plugin.NormalisedName.Clear();
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
                foreach (Player player in Player.List.Where((p) => p.Role != RoleType.None).OrderBy(x => x.Team))
                    if (player.SessionVariables.ContainsKey("MajorScientist"))
                        RoundStart += $"    - ``{player.Nickname}`` ({ConvertID(player.UserId)}) a spawn en MajorScientist.\n";
                    else if (player.SessionVariables.ContainsKey("MajorGuard"))
                        RoundStart += $"    - ``{player.Nickname}`` ({ConvertID(player.UserId)}) a spawn en MajorGuard.\n";
                    else
                        RoundStart += $"    - ``{player.Nickname}`` ({ConvertID(player.UserId)}) a spawn en {player.Role}.\n";
                plugin.LOG += RoundStart;
            });
        }
        public void OnRoundEnd(RoundEndedEventArgs ev)
        {
            plugin.LOG += $":checkered_flag: Fin de la partie.\n```Win: {ev.LeadingTeam}\nClass-D échappé{(ev.ClassList.class_ds <= 1 ? "" : "s")}: {ev.ClassList.class_ds}\nScientifique{(ev.ClassList.scientists <= 1 ? "" : "s")} sauvé: {ev.ClassList.scientists}\nSCPs restant{(ev.ClassList.scps_except_zombies <= 1 ? "" : "s")}: {ev.ClassList.scps_except_zombies}\n{(Exiled.API.Features.Warhead.IsDetonated ? $"Le site a explosé\nMort par la warhead: {ev.ClassList.warhead_kills}" : "Le site n'a pas explosé")}```\n";
        }
        public void OnRoundRestart()
        {
            plugin.LOG += ":cyclone: Redémarrage de la partie.\n";
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
                    objcontent += $"    - ``{playerrespawn.Nickname}`` ({ConvertID(playerrespawn.UserId)})\n";
                }
            }
            else if (ev.NextKnownTeam == SpawnableTeamType.ChaosInsurgency)
            {
                objcontent = ":articulated_lorry: L’Insurrection du Chaos est arrivée sur le site.\n";
                foreach (Player playerrespawn in ev.Players)
                {
                    objcontent += $"    - ``{playerrespawn.Nickname}`` ({ConvertID(playerrespawn.UserId)})\n";
                }
            }
            else
            {
                objcontent = ":snake: La Main du Serpent est arrivée sur le site.\n";
                foreach (Player playerrespawn in ev.Players)
                {
                    objcontent += $"    - ``{playerrespawn.Nickname}`` ({ConvertID(playerrespawn.UserId)})\n";
                }
            }
            plugin.LOG += objcontent;
        }
        public void OnWarheadStart(StartingEventArgs ev)
        {
            if (ev.IsAllowed && ev.Player != null)
                plugin.LOG += $":radioactive: ``{ev.Player.Nickname}`` ({ConvertID(ev.Player.UserId)}) a déclenché la détonation de l'Alpha Warhead.\n";
        }
        public void OnWarheadCancel(StoppingEventArgs ev)
        {
            if (ev.IsAllowed && ev.Player != null)
                plugin.LOG += $":radioactive: ``{ev.Player.Nickname}`` ({ConvertID(ev.Player.UserId)}) a désactivé la détonation de l’Alpha Warhead.\n";
        }
        public void OnDetonated()
        {
            plugin.LOG += ":radioactive: Explosion du site.\n";
        }
        public void OnAnnounceDecont(AnnouncingDecontaminationEventArgs ev)
        {
            if (ev.Id == 6)
                plugin.LOG += ":biohazard: Décontamination de la LCZ.\n";
        }
        public void OnGeneratorFinish(GeneratorActivatedEventArgs ev)
        {
            plugin.LOG += $":computer: Le générateur dans la {Map.FindParentRoom(ev.Generator.gameObject).Type} est activé.\n";
        }
        public void OnPlayerAuth(PreAuthenticatingEventArgs ev)
        {
            plugin.LOGStaff += $":flag_{ev.Country.ToLower()}: {ConvertID(ev.UserId)} ||{ev.Request.RemoteEndPoint}|| tente une connexion sur le serveur.\n";
        }
        public void OnPlayerVerified(VerifiedEventArgs ev)
        {
            plugin.LOG += $":chart_with_upwards_trend: ``{ev.Player.Nickname}`` ({ConvertID(ev.Player.UserId)}) [{ev.Player.Id}] a rejoint le serveur.\n";
            string PlayerName = ev.Player.Nickname.Normalize(System.Text.NormalizationForm.FormKD);
            if (PlayerName.Length < 18)
              plugin.NormalisedName.Add(ev.Player, $"[{ev.Player.Id}] {PlayerName}");
            else
              plugin.NormalisedName.Add(ev.Player, $"[{ev.Player.Id}] {PlayerName.Remove(17)}");
        }
        public void OnPlayerDestroying(DestroyingEventArgs ev)
        {
            if (ev.Player == null) return;
            plugin.LOG += $":chart_with_downwards_trend: ``{ev.Player.Nickname}`` ({ConvertID(ev.Player.UserId)}) a quitté le serveur.\n";
            plugin.NormalisedName.Remove(ev.Player);
        }
        public void OnChangingRole(ChangingRoleEventArgs ev)
        {
            if (!RoundIsStart || ev.Player == null) return;
            if (ev.Player.SessionVariables.TryGetValue("NewRole", out object NewRole))
                plugin.LOG += $":new: ``{ev.Player.Nickname}`` ({ConvertID(ev.Player.UserId)}) a spawn en tant que : {NewRole}.\n";
            else if (ev.Reason == SpawnReason.Escaped)
                if (ev.Player.IsCuffed)
                    plugin.LOG += $":chains: ``{ev.Player.Nickname}`` ({ConvertID(ev.Player.UserId)}) a été escorté en {ev.Player.ReferenceHub.characterClassManager.AliveTime / 60:00}:{ev.Player.ReferenceHub.characterClassManager.AliveTime % 60:00}. Il est devenu : {ev.NewRole}.\n";
                else 
                    plugin.LOG += $":person_running: ``{ev.Player.Nickname}`` ({ConvertID(ev.Player.UserId)}) s'est échapé en {ev.Player.ReferenceHub.characterClassManager.AliveTime / 60:00}:{ev.Player.ReferenceHub.characterClassManager.AliveTime % 60:00}. Il est devenu : {ev.NewRole}.\n";
            else
                plugin.LOG += $":new: ``{ev.Player.Nickname}`` ({ConvertID(ev.Player.UserId)}) a spawn en tant que : {ev.NewRole}.\n";
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
                plugin.LOG += $":skull: ``{ev.Target.Nickname}`` ({ConvertID(ev.Target.UserId)}) est mort par ``{ev.Killer.Nickname}`` ({ConvertID(ev.Killer.UserId)}) avec {ev.HitInformations.CustomAttackerName}.\n";
            }
            else
            {
                plugin.LOG += $":skull: ``{ev.Target.Nickname}`` ({ConvertID(ev.Target.UserId)}) est mort par {ev.HitInformations.CustomAttackerName}.\n";
            }
        }
        public void OnDroppingItem(DroppingItemEventArgs ev)
        {
            if (ev.IsAllowed && ev.Player != null)
                if (Exiled.API.Extensions.ItemExtensions.IsWeapon(ev.Item.Type))
                    if (ev.Item.Type == ItemType.MicroHID && ev.Item.Base.gameObject.TryGetComponent(out MicroHIDItem microHIDItem))
                        plugin.LOG += $":outbox_tray: ``{ev.Player.Nickname}`` ({ConvertID(ev.Player.UserId)}) a jeté {ev.Item.Type}({(int)(microHIDItem.RemainingEnergy * 100)}%).\n";
                    else if (ev.Item.Base.gameObject.TryGetComponent(out Firearm firearm))
                        plugin.LOG += $":outbox_tray: ``{ev.Player.Nickname}`` ({ConvertID(ev.Player.UserId)}) a jeté {ev.Item.Type}({(int)firearm.Status.Ammo}).\n";
                else if (ev.Item.Type == ItemType.Radio && ev.Item.Base.gameObject.TryGetComponent(out RadioItem radioitem))
                    plugin.LOG += $":outbox_tray: ``{ev.Player.Nickname}`` ({ConvertID(ev.Player.UserId)}) a jeté {ev.Item.Type}({(int)radioitem._battery * 100}%).\n";
                else
                    plugin.LOG += $":outbox_tray: ``{ev.Player.Nickname}`` ({ConvertID(ev.Player.UserId)}) a jeté {ev.Item.Type}.\n";
        }
        public void OnPickingUpItem(PickingUpItemEventArgs ev)
        {
            if (ev.IsAllowed && ev.Player != null)
                if (Exiled.API.Extensions.ItemExtensions.IsWeapon(ev.Pickup.Type))
                    if (ev.Pickup.Type == ItemType.MicroHID && ev.Pickup.Base.gameObject.TryGetComponent(out MicroHIDItem microHIDItem))
                        plugin.LOG += $":inbox_tray: ``{ev.Player.Nickname}`` ({ConvertID(ev.Player.UserId)}) a récupéré {ev.Pickup.Type}({(int)(microHIDItem.RemainingEnergy * 100)}%).\n";
                    else if (ev.Pickup.Base.gameObject.TryGetComponent(out Firearm firearm))
                        plugin.LOG += $":inbox_tray: ``{ev.Player.Nickname}`` ({ConvertID(ev.Player.UserId)}) a récupéré {ev.Pickup.Type}({(int)firearm.Status.Ammo}).\n";
                else if (ev.Pickup.Type == ItemType.Radio && ev.Pickup.Base.gameObject.TryGetComponent(out RadioItem radioitem))
                    plugin.LOG += $":inbox_tray: ``{ev.Player.Nickname}`` ({ConvertID(ev.Player.UserId)}) a récupéré {ev.Pickup.Type}({(int)radioitem._battery * 100}%).\n";
                else
                    plugin.LOG += $":inbox_tray: ``{ev.Player.Nickname}`` ({ConvertID(ev.Player.UserId)}) a récupéré {ev.Pickup.Type}.\n";
        }
        
        public void OnPlayerUsedItem(UsedItemEventArgs ev)
        {
            if (ev.Player != null && Exiled.API.Extensions.ItemExtensions.IsMedical(ev.Item.Type))
                plugin.LOG += $":adhesive_bandage: ``{ev.Player.Nickname}`` ({ConvertID(ev.Player.UserId)}) s'est soigné avec {ev.Item}.\n";
            else
                plugin.LOG += $":??: ``{ev.Player.Nickname}`` ({ConvertID(ev.Player.UserId)}) a utilisé {ev.Item}.\n";
        }
        public void OnGeneratorUnlock(UnlockingGeneratorEventArgs ev)
        {
            if (ev.IsAllowed && ev.Player != null)
                plugin.LOG += $":computer: ``{ev.Player.Nickname}`` ({ConvertID(ev.Player.UserId)}) a débloqué un générateur dans la salle : {Map.FindParentRoom(ev.Generator.gameObject).Type}.\n";
        }
        public void OnStoppingGenerator(StoppingGeneratorEventArgs ev)
        {
            
            if (ev.IsAllowed && ev.Player != null && ev.Generator.Activating)
                plugin.LOG += $":computer: ``{ev.Player.Nickname}`` ({ConvertID(ev.Player.UserId)}) a ejecté la tablette du générateur de la salle : {Map.FindParentRoom(ev.Generator.gameObject).Type}.\n";
        }
        public void OnActivatingGenerator(ActivatingGeneratorEventArgs ev)
        {
            if (ev.IsAllowed && ev.Player != null)
                plugin.LOG += $":computer: ``{ev.Player.Nickname}`` ({ConvertID(ev.Player.UserId)}) a inséré une tablette dans un générateur de la salle : {Map.FindParentRoom(ev.Generator.gameObject)}.\n";
        }
        public void OnActivatingWarheadPanel(ActivatingWarheadPanelEventArgs ev)
        {
            if (ev.IsAllowed && ev.Player != null && !UnityEngine.Object.FindObjectOfType<AlphaWarheadOutsitePanel>().keycardEntered)
                plugin.LOG += $":radioactive: ``{ev.Player.Nickname}`` ({ConvertID(ev.Player.UserId)}) a ouvert la protection pour activé l'Alpha Warhead.\n";
            else if (ev.IsAllowed && ev.Player != null && UnityEngine.Object.FindObjectOfType<AlphaWarheadOutsitePanel>().keycardEntered)
                plugin.LOG += $":radioactive: ``{ev.Player.Nickname}`` ({ConvertID(ev.Player.UserId)}) a fermer la protection pour activé l'Alpha Warhead.\n";
        }
        public void OnLocalReporting(LocalReportingEventArgs ev)
        {
            if (DiscordLog.Instance.Config.WebhookReport != "none")
                Webhook.ReportAsync(ev.Issuer, ev.Target, DiscordLog.Instance.Config.WebhookReport, DiscordLog.Instance.Config.Ping, ev.Reason);
            ev.IsAllowed = true;
        }

        public void OnIntercomSpeaking(IntercomSpeakingEventArgs ev)
        {
            if (ev.IsAllowed && IntercomPlayerSpeek != ev.Player)
            {
                IntercomPlayerSpeek = ev.Player;
                plugin.LOG += $":loudspeaker: ``{ev.Player.Nickname}`` ({ConvertID(ev.Player.UserId)}) utilise l'intercom.\n";
            }
            else if (ev.Player == null)
                IntercomPlayerSpeek = null;
        }
        public void OnHandcuffing(HandcuffingEventArgs ev)
        {
            if (ev.Cuffer != null)
                plugin.LOG += $":chains: ``{ev.Target.Nickname}`` ({ConvertID(ev.Target.UserId)}) a été menoté par ``{ev.Cuffer.Nickname}`` ({ConvertID(ev.Cuffer.UserId)}).\n";
        }
        public void OnRemovingHandcuffs(RemovingHandcuffsEventArgs ev)
        {
            if (ev.Cuffer != null)
                plugin.LOG += $":chains: ``{ev.Target.Nickname}`` ({ConvertID(ev.Target.UserId)}) a été démenoté par ``{ev.Cuffer.Nickname}`` ({ConvertID(ev.Cuffer.UserId)}).\n";
            else
                plugin.LOG += $":chains: ``{ev.Target.Nickname}`` ({ConvertID(ev.Target.UserId)}) a été démenoté.\n";
        }
        public void OnEnteringPocketDimension(EnteringPocketDimensionEventArgs ev)
        {
            if (ev.Player != null)
                plugin.LOG += $":hole: ``{ev.Player.Nickname}`` ({ConvertID(ev.Player.UserId)}) est entré dans la dimension de poche.\n";
        }
        public void OnEscapingPocketDimension(EscapingPocketDimensionEventArgs ev)
        {
            if (ev.Player != null)
                plugin.LOG += $":hole: ``{ev.Player.Nickname}`` ({ConvertID(ev.Player.UserId)}) a échappé a la dimension de poche.\n";
        }
        public void On914Activating(ActivatingEventArgs ev)
        {
            if (ev.IsAllowed && ev.Player != null)
                Use914 = ev.Player;
        }
        
        public void OnFinishingRecall(FinishingRecallEventArgs ev)
        {
            if (ev.IsAllowed && ev.Target != null && ev.Scp049 != null)
                plugin.LOG += $":zombie: ``{ev.Target.Nickname}`` ({ConvertID(ev.Target.UserId)}) a été ressuscité en Scp049-2 par ``{ev.Scp049.Nickname}`` ({ConvertID(ev.Scp049.UserId)}).\n";
        }
        public void OnBanning(BanningEventArgs ev)
        {
            if (ev.IsAllowed && ev.Target != null && ev.Issuer != null)
            {
                Webhook.BanPlayerAsync(ev.Issuer, ev.Target,ev.Reason, ev.Duration);
                plugin.LOGStaff += $":hammer: ``{ev.Target.Nickname}`` ({ConvertID(ev.Target.UserId)}) a été banni pour :``{ev.Reason}`` ; pendant {ev.Duration} secondes par ``{ev.Issuer.Nickname}`` ({ConvertID(ev.Issuer.UserId)}).\n";
            } 
        }
        public void OnKicking(KickingEventArgs ev)
        {
            if (ev.IsAllowed && ev.Target != null && ev.Issuer != null)
            {
                plugin.LOGStaff += $":mans_shoe: ``{ev.Target.Nickname}`` ({ConvertID(ev.Target.UserId)}) a été kick pour : ``{ev.Reason}`` ; par ``{ev.Issuer.Nickname}`` ({ConvertID(ev.Issuer.UserId)}).\n";
                Webhook.KickPlayerAsync(ev.Issuer, ev.Target, ev.Reason);
            }
        }
        public static string ConvertID(string UserID)
        {
            if (string.IsNullOrEmpty(UserID)) return string.Empty;
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

