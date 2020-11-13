using System;
using Exiled.API.Enums;
using Exiled.API.Features;
using HarmonyLib;

using ServerEvents = Exiled.Events.Handlers.Server;
using MapEvents = Exiled.Events.Handlers.Map;
using WarheadEvents = Exiled.Events.Handlers.Warhead;
using PlayerEvents = Exiled.Events.Handlers.Player;
using Scp079Events = Exiled.Events.Handlers.Scp079;
using Scp914Events = Exiled.Events.Handlers.Scp914;
using Scp106Events = Exiled.Events.Handlers.Scp106;
using Scp096Events = Exiled.Events.Handlers.Scp096;

namespace DiscordLog
{
	public class DiscordLog : Plugin<Configs>
	{
		public override string Name => "SanyaPlugin";
		public override string Prefix => "sanya";
		public override string Author => "sanyae2439";
		public override PluginPriority Priority => PluginPriority.Default;
		public override Version Version => new Version(2, 9, 1);
		public override Version RequiredExiledVersion => new Version(2, 1, 9);

		public static DiscordLog Instance { get; private set; }
		public EventHandlers Handlers { get; private set; }

		public DiscordLog() => Instance = this;

		public override void OnEnabled()
		{
			if (!Config.IsEnabled) return;

			base.OnEnabled();

			RegistEvents();

			Log.Info($"[OnEnabled] SanyaPlugin({Version}) Enabled Complete.");
		}

		public override void OnDisabled()
		{
			base.OnDisabled();

			UnRegistEvents();

			Log.Info($"[OnDisable] SanyaPlugin({Version}) Disabled Complete.");
		}

		private void RegistEvents()
		{
			Handlers = new EventHandlers(this);
			ServerEvents.WaitingForPlayers += Handlers.OnWaintingForPlayers;
			ServerEvents.RoundStarted += Handlers.OnRoundStart;
			ServerEvents.RoundEnded += Handlers.OnRoundEnd;
			ServerEvents.RestartingRound += Handlers.OnRoundRestart;
			ServerEvents.RespawningTeam += Handlers.OnTeamRespawn;

			WarheadEvents.Starting += Handlers.OnWarheadStart;
			WarheadEvents.Stopping += Handlers.OnWarheadCancel;
			WarheadEvents.Detonated += Handlers.OnDetonated;

			MapEvents.AnnouncingDecontamination += Handlers.OnAnnounceDecont;
			MapEvents.GeneratorActivated += Handlers.OnGeneratorFinish;

			PlayerEvents.Joined += Handlers.OnPlayerJoin;
			PlayerEvents.Left += Handlers.OnPlayerLeave;
			PlayerEvents.ChangingRole += Handlers.OnChangingRole;
			PlayerEvents.Hurting += Handlers.OnPlayerHurt;
			PlayerEvents.Died += Handlers.OnPlayerDeath;
			PlayerEvents.ItemDropped += Handlers.OnItemDropped;
			PlayerEvents.PickingUpItem += Handlers.OnPickingUpItem;
			PlayerEvents.MedicalItemUsed += Handlers.OnPlayerUsedMedicalItem;

			PlayerEvents.UnlockingGenerator += Handlers.OnGeneratorUnlock;
			PlayerEvents.EjectingGeneratorTablet += Handlers.OnEjectingGeneratorTablet;
			PlayerEvents.InsertingGeneratorTablet += Handlers.OnGeneratorInsert;
			PlayerEvents.ActivatingWarheadPanel += Handlers.OnActivatingWarheadPanel;
			PlayerEvents.IntercomSpeaking += Handlers.OnIntercomSpeaking;

			Scp914Events.Activating += Scp914Events_Activating;
			Scp914Events.UpgradingItems += Handlers.On914Upgrade;
		}

		private void Scp914Events_Activating(Exiled.Events.EventArgs.ActivatingEventArgs ev)
		{
			throw new NotImplementedException();
		}

		private void UnRegistEvents()
		{

			Handlers = null;
		}
	}
}