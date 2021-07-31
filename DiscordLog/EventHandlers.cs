﻿using Exiled.API.Features;
using Exiled.Events.EventArgs;
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
        private Player Use914;
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
            string RoundStart = $":triangular_flag_on_post: Démarrage de la partie avec {Player.List.Where((p) => p.Role != RoleType.None).Count()} joueurs.\n";
            foreach (Player player in Player.List.Where((p) => p.Role != RoleType.None).OrderBy(x=>x.Team))
                RoundStart += $"    - ``{player.Nickname}`` ({ConvertID(player.UserId)}) a spawn en {player.Role}.\n";
            plugin.LOG += RoundStart;
            RoundIsStart = true;
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

            plugin.LOG += $":computer: Le générateur dans la {Map.FindParentRoom(ev.Generator.NameOfGeneratorRoom().gameObject).Type} est activé.\n";
        }
        public void OnPlayerAuth(PreAuthenticatingEventArgs ev)
        {
            plugin.LOGStaff += ($":flag_{ev.Country.ToLower()}: {ConvertID(ev.UserId)} ||{ev.Request.RemoteEndPoint}|| tente une connexion sur le serveur.\n");
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
            if (SerpentsHand.API.IsSerpent(ev.Player))
                plugin.LOG += $":new: ``{ev.Player.Nickname}`` ({ConvertID(ev.Player.UserId)}) a spawn en tant que : SerpentHand.\n";
            else if (ev.IsEscaped)
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
                plugin.LOG += $":skull: ``{ev.Target.Nickname}`` ({ConvertID(ev.Target.UserId)}) est mort par ``{ev.Killer.Nickname}`` ({ConvertID(ev.Killer.UserId)}) avec {ev.HitInformations.GetDamageName()}.\n";
            }
            else
            {
                plugin.LOG += $":skull: ``{ev.Target.Nickname}`` ({ConvertID(ev.Target.UserId)}) est mort par {ev.HitInformations.GetDamageName()}.\n";
            }
        }
        public void OnDroppingItem(DroppingItemEventArgs ev)
        {
            if (ev.IsAllowed && ev.Player != null)
                if (Exiled.API.Extensions.Item.IsWeapon(ev.Item.id))
                    if (ev.Item.id == ItemType.MicroHID)
                        plugin.LOG += $":outbox_tray: ``{ev.Player.Nickname}`` ({ConvertID(ev.Player.UserId)}) a jeté {ev.Item.id}({(int)(ev.Item.durability*100)}%).\n";
                    else 
                        plugin.LOG += $":outbox_tray: ``{ev.Player.Nickname}`` ({ConvertID(ev.Player.UserId)}) a jeté {ev.Item.id}({(int)ev.Item.durability}).\n";
                else if (ev.Item.id == ItemType.Radio)
                    plugin.LOG += $":outbox_tray: ``{ev.Player.Nickname}`` ({ConvertID(ev.Player.UserId)}) a jeté {ev.Item.id}({(int)ev.Item.durability}%).\n";
                else
                    plugin.LOG += $":outbox_tray: ``{ev.Player.Nickname}`` ({ConvertID(ev.Player.UserId)}) a jeté {ev.Item.id}.\n";
        }
        public void OnPickingUpItem(PickingUpItemEventArgs ev)
        {
            if (ev.IsAllowed && ev.Player != null)
                if (Exiled.API.Extensions.Item.IsWeapon(ev.Pickup.ItemId))
                    if (ev.Pickup.ItemId == ItemType.MicroHID)
                        plugin.LOG += $":inbox_tray: ``{ev.Player.Nickname}`` ({ConvertID(ev.Player.UserId)}) a récupéré {ev.Pickup.ItemId}({(int)(ev.Pickup.durability * 100)}%).\n";
                    else
                        plugin.LOG += $":inbox_tray: ``{ev.Player.Nickname}`` ({ConvertID(ev.Player.UserId)}) a récupéré {ev.Pickup.ItemId}({(int)ev.Pickup.durability}).\n";
                else if (ev.Pickup.ItemId == ItemType.Radio)
                    plugin.LOG += $":inbox_tray: ``{ev.Player.Nickname}`` ({ConvertID(ev.Player.UserId)}) a récupéré {ev.Pickup.ItemId}({(int)ev.Pickup.durability}%).\n";
                else
                    plugin.LOG += $":inbox_tray: ``{ev.Player.Nickname}`` ({ConvertID(ev.Player.UserId)}) a récupéré {ev.Pickup.ItemId}.\n";
        }
        
        public void OnPlayerUsedMedicalItem(UsedMedicalItemEventArgs ev)
        {
            if (ev.Player != null)
                plugin.LOG += $":adhesive_bandage: ``{ev.Player.Nickname}`` ({ConvertID(ev.Player.UserId)}) s'est soigné avec {ev.Item}.\n";
        }
        public void OnGeneratorUnlock(UnlockingGeneratorEventArgs ev)
        {
            if (ev.IsAllowed && ev.Player != null)
                plugin.LOG += $":computer: ``{ev.Player.Nickname}`` ({ConvertID(ev.Player.UserId)}) a débloqué un générateur dans la salle : {Map.FindParentRoom(ev.Generator.NameOfGeneratorRoom().gameObject).Type}.\n";
        }
        public void OnEjectingGeneratorTablet(EjectingGeneratorTabletEventArgs ev)
        {
            
            if (ev.IsAllowed && ev.Player != null && ev.Generator.isTabletConnected)
                plugin.LOG += $":computer: ``{ev.Player.Nickname}`` ({ConvertID(ev.Player.UserId)}) a ejecté la tablette du générateur de la salle : {Map.FindParentRoom(ev.Generator.NameOfGeneratorRoom().gameObject).Type}.\n";
        }
        public void OnGeneratorInsert(InsertingGeneratorTabletEventArgs ev)
        {
            if (ev.IsAllowed && ev.Player != null)
                plugin.LOG += $":computer: ``{ev.Player.Nickname}`` ({ConvertID(ev.Player.UserId)}) a inséré une tablette dans un générateur de la salle : {Map.FindParentRoom(ev.Generator.NameOfGeneratorRoom().gameObject).Type}.\n";
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
            if (ev.IsAllowed && IntercomPlayerSpeek != ev.Player && Intercom.host.remainingCooldown <= 0f && !ev.Player.IsIntercomMuted && !ev.Player.IsMuted && Player.Dictionary.TryGetValue(Intercom.host.speaker,out Player speaker) && speaker == ev.Player)
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
        public void On914Upgrade(UpgradingItemsEventArgs ev)
        {
            string str;
            if (Use914 != null)
                str = $":gear: SCP-914 a été enclenché en {ev.KnobSetting} par ``{Use914.Nickname}`` ({ConvertID(Use914.UserId)}) :\n";
            else
                str = $":gear: SCP-914 a été enclenché en {ev.KnobSetting} par Unknow :\n";
            bool Item = ev.Items.Count != 0;
            bool PlayerItem = ev.Players.Where(x => x.CurrentItemIndex != -1).Count() != 0;
            if (Item || PlayerItem)
            {
                str += $"**Item{(ev.Items.Count + ev.Players.Where(x => x.CurrentItemIndex != -1).Count() <= 1 ? "" : "s")}**\n";
                if (Item)
                    foreach (Pickup item in ev.Items.Where(x => !x.ItemId.ToString().Contains("Ammo")))//item.itemId
                    {
                        str += $"   - {item.itemId}\n";
                    }
                if (PlayerItem)
                    foreach (Player player in ev.Players.Where(x => x.CurrentItemIndex != -1))
                    {
                        str += $"   - {player.CurrentItem.id}\n";
                    }
            }
            if (ev.Players.Count != 0)
            {
                str += $"**Joueur{(ev.Players.Count() <= 1 ? "" : "s")}**\n";
                foreach (Player player in ev.Players)
                {
                    str += $"   - ``{player.Nickname}`` ({ConvertID(player.UserId)})\n";
                }
            }
            Use914 = null;
            plugin.LOG += str;
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
                plugin.LOGStaff += ($":hammer: ``{ev.Target.Nickname}`` ({ConvertID(ev.Target.UserId)}) a été banni pour :``{ev.Reason}`` ; pendant {ev.Duration} secondes par ``{ev.Issuer.Nickname}`` ({ConvertID(ev.Issuer.UserId)}).\n");
            } 
        }
        public void OnKicking(KickingEventArgs ev)
        {
            if (ev.IsAllowed && ev.Target != null && ev.Issuer != null)
            {
                plugin.LOGStaff += ($":mans_shoe: ``{ev.Target.Nickname}`` ({ConvertID(ev.Target.UserId)}) a été kick pour : ``{ev.Reason}`` ; par ``{ev.Issuer.Nickname}`` ({ConvertID(ev.Issuer.UserId)}).\n");
                Webhook.KickPlayerAsync(ev.Issuer, ev.Target, ev.Reason);
            }
        }
        public void OnSendingRemoteAdminCommand(SendingRemoteAdminCommandEventArgs ev)
        {
            bool success = false;
            if (!ev.Success || ev.Name.ToLower() == "ban" || ev.Name.ToLower() == "kick" || ev.Name == null) return;
            switch (ev.Name.ToLower())
            {
                case "oban":
                    {
                        if (!string.IsNullOrEmpty(ev.Arguments[0]) && uint.TryParse(ev.Arguments[1], out uint Duration))
                        {
                            int l = 2;
                            string str1 = null;
                            foreach (string str2 in ev.Arguments)
                            {
                                if (l == 0)
                                    str1 += $" {str2}";
                                else
                                    l--;
                            }
                            Webhook.OBanPlayerAsync(ev.Sender, ev.Arguments[0], str1, Duration);
                            success = true;
                        }
                    }
                    return;
                case "jail":
                    {
                        {
                            Player player = Player.Get(ev.Arguments[0]);
                            plugin.LOGStaff +=($":keyboard: ``{ev.Sender.Nickname}`` ({ConvertID(ev.Sender.UserId)}) a jail ``{player.Nickname}`` ({player.UserId}).\n");
                            success = true;
                        }
                    }
                    return;
                case "forceclass":
                    {
                        if (int.TryParse(ev.Arguments[1], out int Role))
                        {
                            string Receiver = string.Empty;

                            string[] Users = ev.Arguments[0].Split('.');
                            List<Player> PlyList = new List<Player>();
                            foreach (string s in Users)
                            {
                                if (int.TryParse(s, out int id) && Player.Get(id) != null)
                                    PlyList.Add(Player.Get(id));
                                else if (Player.Get(s) != null)
                                    PlyList.Add(Player.Get(s));
                            }
                            foreach (Player ply in PlyList)
                            {
                                Receiver += $"\n - ``{ply.Nickname}`` ({ConvertID(ply.UserId)})";
                            }
                            plugin.LOGStaff +=($":keyboard: ``{ev.Sender.Nickname}`` ({ConvertID(ev.Sender.UserId)}) a changé en {(RoleType)Role} : {Receiver}");
                            success = true;
                        }
                    }
                    return;
                case "give":
                    {
                        if (int.TryParse(ev.Arguments[1], out int Item))
                        {
                            string Receiver = string.Empty;

                            string[] Users = ev.Arguments[0].Split('.');
                            List<Player> PlyList = new List<Player>();
                            foreach (string s in Users)
                            {
                                if (int.TryParse(s, out int id) && Player.Get(id) != null)
                                    PlyList.Add(Player.Get(id));
                                else if (Player.Get(s) != null)
                                    PlyList.Add(Player.Get(s));
                            }
                            foreach (Player ply in PlyList)
                            {
                                Receiver += $"\n - ``{ply.Nickname}`` ({ConvertID(ply.UserId)})\n";
                            }
                            plugin.LOGStaff +=($":keyboard: ``{ev.Sender.Nickname}`` ({ConvertID(ev.Sender.UserId)}) a donné : {(ItemType)Item} {Receiver}");
                            success = true;
                        }
                    }
                    return;
                case "overwatch":
                    {
                        string Receiver = string.Empty;

                        string[] Users = ev.Arguments[0].Split('.');
                        List<Player> PlyList = new List<Player>();
                        foreach (string s in Users)
                        {
                            if (int.TryParse(s, out int id) && Player.Get(id) != null)
                                PlyList.Add(Player.Get(id));
                            else if (Player.Get(s) != null)
                                PlyList.Add(Player.Get(s));
                        }
                        foreach (Player ply in PlyList)
                        {
                            Receiver += $"\n - ``{ply.Nickname}`` ({ConvertID(ply.UserId)})";
                        }
                        if (ev.Arguments[1] == "0")
                        {
                            plugin.LOGStaff +=($":keyboard: ``{ev.Sender.Nickname}`` ({ConvertID(ev.Sender.UserId)}) à enlever l'overwatch : {Receiver}");
                            success = true; 
                        }
                        else if (ev.Arguments[1] == "1")
                        {
                            plugin.LOGStaff +=($":keyboard: ``{ev.Sender.Nickname}`` ({ConvertID(ev.Sender.UserId)}) à mis l'overwatch : {Receiver}");
                            success = true; 
                        }
                    }
                    return;
                case "bring":
                    {
                        string Receiver = string.Empty;

                        string[] Users = ev.Arguments[0].Split('.');
                        List<Player> PlyList = new List<Player>();
                        foreach (string s in Users)
                        {
                            if (int.TryParse(s, out int id) && Player.Get(id) != null)
                                PlyList.Add(Player.Get(id));
                            else if (Player.Get(s) != null)
                                PlyList.Add(Player.Get(s));
                        }
                        foreach (Player ply in PlyList)
                        {
                            Receiver += $"\n - ``{ply.Nickname}`` ({ConvertID(ply.UserId)})";
                        }
                        plugin.LOGStaff +=($":keyboard: ``{ev.Sender.Nickname}`` ({ConvertID(ev.Sender.UserId)}) à tp les joueurs sur lui : {Receiver}");
                        success = true;
                    }
                    return;
                case "goto":
                    {
                        Player player = Player.Get(ev.Arguments[0]);
                        plugin.LOGStaff +=($":keyboard: ``{ev.Sender.Nickname}`` ({ConvertID(ev.Sender.UserId)}) se tp à ``{player.Nickname}`` ({player.UserId}).");
                        success = true;
                    }
                    return;
                case "request_data":
                    {
                        Player player = Player.Get(ev.Arguments[1]);
                        plugin.LOGStaff +=($":keyboard: ``{ev.Sender.Nickname}`` ({ConvertID(ev.Sender.UserId)}) a demandé les donnée de {player.Nickname}`` ({ConvertID(player.UserId)}) : {ev.Arguments[0]}");
                        success = true;
                    }
                    return;
                case "effect":
                    {
                        string Receiver = string.Empty;

                        string[] Users = ev.Arguments[0].Split('.');
                        List<Player> PlyList = new List<Player>();
                        foreach (string s in Users)
                        {
                            if (int.TryParse(s, out int id) && Player.Get(id) != null)
                                PlyList.Add(Player.Get(id));
                            else if (Player.Get(s) != null)
                                PlyList.Add(Player.Get(s));
                        }
                        foreach (Player ply in PlyList)
                        {
                            Receiver += $"\n - ``{ply.Nickname}`` ({ConvertID(ply.UserId)})";
                        }
                        plugin.LOGStaff +=($":keyboard: ``{ev.Sender.Nickname}`` ({ConvertID(ev.Sender.UserId)}) a envoyé {ev.Name} {ev.Arguments[1]} : {Receiver}");
                        success = true;
                    }
                    return;
                case "mute":
                case "unmute":
                case "imute":
                case "iunmute":
                case "disarm":
                case "release":
                    {
                        string Receiver = string.Empty;

                        string[] Users = ev.Arguments[0].Split('.');
                        List<Player> PlyList = new List<Player>();
                        foreach (string s in Users)
                        {
                            if (int.TryParse(s, out int id) && Player.Get(id) != null)
                                PlyList.Add(Player.Get(id));
                            else if (Player.Get(s) != null)
                                PlyList.Add(Player.Get(s));
                        }
                        foreach (Player ply in PlyList)
                        {
                            Receiver += $"\n - ``{ply.Nickname}`` ({ConvertID(ply.UserId)})";
                        }
                        plugin.LOGStaff +=($":keyboard: ``{ev.Sender.Nickname}`` ({ConvertID(ev.Sender.UserId)}) à {ev.Name} : {Receiver}");
                        success = true;
                    }
                    return;
            }
            if (!success)
            {
                string str1 = null;
                foreach (string str2 in ev.Arguments)
                    str1 += $" {str2}";
                plugin.LOGStaff +=($":keyboard: ``{ev.Sender.Nickname}`` ({ConvertID(ev.Sender.UserId)}) a envoyé ``{ev.Name}{str1}``.\n");
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

