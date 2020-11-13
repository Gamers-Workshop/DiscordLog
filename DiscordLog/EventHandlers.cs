using Exiled.API.Features;
using Exiled.Events.EventArgs;
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

        public EventHandlers(DiscordLog plugin) => this.plugin = plugin;
        public void OnWaintingForPlayers()
        {

        }
        public void OnRoundStart()
        {

        }
        public void OnRoundEnd(RoundEndedEventArgs ev)
        {

        }
        public void OnRoundRestart()
        {

        }
        public void OnTeamRespawn(RespawningTeamEventArgs ev)
        {

        }
        public void OnWarheadStart(StartingEventArgs ev)
        {

        }
        public void OnWarheadCancel(StoppingEventArgs ev)
        {

        }
        public void OnDetonated()
        {

        }
        public void OnAnnounceDecont(AnnouncingDecontaminationEventArgs ev)
        {

        }
        public void OnGeneratorFinish(GeneratorActivatedEventArgs ev)
        {

        }
        public void OnPlayerJoin(JoinedEventArgs ev)
        {

        }
        public void OnPlayerLeave(LeftEventArgs ev)
        {

        }
        public void OnChangingRole(ChangingRoleEventArgs ev)
        {

        }
        public void OnPlayerHurt(HurtingEventArgs ev)
        {

        }
        public void OnPlayerDeath(DiedEventArgs ev)
        {

        }
        public void OnItemDropped(ItemDroppedEventArgs ev)
        {

        }
        public void OnPickingUpItem(PickingUpItemEventArgs ev)
        {

        }
        public void OnPlayerUsedMedicalItem(UsedMedicalItemEventArgs ev)
        {

        }
        public void OnGeneratorUnlock(UnlockingGeneratorEventArgs ev)
        {

        }
        public void OnEjectingGeneratorTablet(EjectingGeneratorTabletEventArgs ev)
        {

        }
        public void OnGeneratorInsert(InsertingGeneratorTabletEventArgs ev)
        {

        }
        public void OnActivatingWarheadPanel(ActivatingWarheadPanelEventArgs ev)
        {

        }
        public void OnIntercomSpeaking(IntercomSpeakingEventArgs ev)
        {

        }
        public void On914Activating(ActivatingEventArgs ev)
        {

        }
        public void On914Upgrade(UpgradingItemsEventArgs ev)
        {

        }
        public void OnBanning(BanningEventArgs ev)
        {

        }
        public void OnKicking(KickingEventArgs ev)
        {

        }
        public void OnSendingRemoteAdminCommand(SendingRemoteAdminCommandEventArgs ev)
        {

        }
    }
}

