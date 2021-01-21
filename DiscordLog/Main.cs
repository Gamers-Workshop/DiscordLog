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

using System.Collections.Generic;
using System.IO;
using MEC;
using System.Linq;

namespace DiscordLog
{
	public class DiscordLog : Plugin<Configs>
	{
		public override string Name => "DiscordLog";
		public override string Prefix => "DiscordLog";
		public override string Author => "Husky/Corentin & Louis1706";
		public override PluginPriority Priority => PluginPriority.Lowest;
		public override Version Version => new Version(2, 9, 1);
		public override Version RequiredExiledVersion => new Version(2, 1, 9);

		public static DiscordLog Instance { get; private set; }
		public EventHandlers Handlers { get; private set; }

		public DiscordLog() => Instance = this;
		public Harmony Harmony { get; private set; }
		public string LOG = null;
		public static List<Sanction> SanctionedPlayer = new List<Sanction>();

		private int patchCounter;

		public override void OnEnabled()
		{
			if (!Config.IsEnabled) return;

			base.OnEnabled();

			RegistEvents();

			RegistPatch();

			Log.Info($"[OnEnabled] DiscordLog({Version}) Enabled Complete.");
		}

		public override void OnDisabled()
		{
			foreach (CoroutineHandle handle in EventHandlers.Coroutines)
				Timing.KillCoroutines(handle);
			base.OnDisabled();

			UnRegistEvents();
			UnRegistPatch();

			Log.Info($"[OnDisable] DiscordLog({Version}) Disabled Complete.");
		}

		private void RegistEvents()
		{
			try
			{
				string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
				string pluginPath = Path.Combine(appData, "Plugins");
				string path = Path.Combine(Paths.Plugins, "DiscordLog");
				string sanctionedPlayer = Path.Combine(path, "DiscordLog-SanctionedPlayer.txt");

				if (!Directory.Exists(path))
					Directory.CreateDirectory(path);

				if (!File.Exists(sanctionedPlayer))
					File.Create(sanctionedPlayer).Close();

			//	SanctionedPlayer = sanctionedPlayer;
			}
			catch (Exception e)
			{
				Log.Error($"Loading error: {e}");
			}
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

			PlayerEvents.PreAuthenticating += Handlers.OnPlayerAuth;
			PlayerEvents.Joined += Handlers.OnPlayerJoin;
			PlayerEvents.Left += Handlers.OnPlayerLeave;
			PlayerEvents.ChangingRole += Handlers.OnChangingRole;
			PlayerEvents.Hurting += Handlers.OnPlayerHurt;
			PlayerEvents.Died += Handlers.OnPlayerDeath;
			PlayerEvents.DroppingItem += Handlers.OnDroppingItem;
			PlayerEvents.PickingUpItem += Handlers.OnPickingUpItem;
			PlayerEvents.MedicalItemUsed += Handlers.OnPlayerUsedMedicalItem;

			PlayerEvents.UnlockingGenerator += Handlers.OnGeneratorUnlock;
			PlayerEvents.EjectingGeneratorTablet += Handlers.OnEjectingGeneratorTablet;
			PlayerEvents.InsertingGeneratorTablet += Handlers.OnGeneratorInsert;
			PlayerEvents.ActivatingWarheadPanel += Handlers.OnActivatingWarheadPanel;
			PlayerEvents.IntercomSpeaking += Handlers.OnIntercomSpeaking;
			PlayerEvents.Handcuffing += Handlers.OnHandcuffing;
			PlayerEvents.RemovingHandcuffs += Handlers.OnRemovingHandcuffs;

			PlayerEvents.EnteringPocketDimension += Handlers.OnEnteringPocketDimension;
			PlayerEvents.EscapingPocketDimension += Handlers.OnEscapingPocketDimension;
			Scp914Events.Activating += Handlers.On914Activating;
			Scp914Events.UpgradingItems += Handlers.On914Upgrade;

			//LogStaff
			PlayerEvents.Banning += Handlers.OnBanning;
			PlayerEvents.Kicking += Handlers.OnKicking;

			ServerEvents.SendingRemoteAdminCommand += Handlers.OnSendingRemoteAdminCommand;
		}

		private void UnRegistEvents()
		{
			ServerEvents.WaitingForPlayers -= Handlers.OnWaintingForPlayers;
			ServerEvents.RoundStarted -= Handlers.OnRoundStart;
			ServerEvents.RoundEnded -= Handlers.OnRoundEnd;
			ServerEvents.RestartingRound -= Handlers.OnRoundRestart;
			ServerEvents.RespawningTeam -= Handlers.OnTeamRespawn;

			WarheadEvents.Starting -= Handlers.OnWarheadStart;
			WarheadEvents.Stopping -= Handlers.OnWarheadCancel;
			WarheadEvents.Detonated -= Handlers.OnDetonated;

			MapEvents.AnnouncingDecontamination -= Handlers.OnAnnounceDecont;
			MapEvents.GeneratorActivated -= Handlers.OnGeneratorFinish;

			//PlayerEvents.PreAuthenticating -= Handlers.OnPlayerAuth;
			PlayerEvents.Joined -= Handlers.OnPlayerJoin;
			PlayerEvents.Left -= Handlers.OnPlayerLeave;
			PlayerEvents.ChangingRole -= Handlers.OnChangingRole;
			PlayerEvents.Hurting -= Handlers.OnPlayerHurt;
			PlayerEvents.Died -= Handlers.OnPlayerDeath;
			PlayerEvents.DroppingItem -= Handlers.OnDroppingItem;
			PlayerEvents.PickingUpItem -= Handlers.OnPickingUpItem;
			PlayerEvents.MedicalItemUsed -= Handlers.OnPlayerUsedMedicalItem;

			PlayerEvents.UnlockingGenerator -= Handlers.OnGeneratorUnlock;
			PlayerEvents.EjectingGeneratorTablet -= Handlers.OnEjectingGeneratorTablet;
			PlayerEvents.InsertingGeneratorTablet -= Handlers.OnGeneratorInsert;
			PlayerEvents.ActivatingWarheadPanel -= Handlers.OnActivatingWarheadPanel;
			PlayerEvents.IntercomSpeaking -= Handlers.OnIntercomSpeaking;
			PlayerEvents.Handcuffing -= Handlers.OnHandcuffing;
			PlayerEvents.RemovingHandcuffs -= Handlers.OnRemovingHandcuffs;

			PlayerEvents.EnteringPocketDimension -= Handlers.OnEnteringPocketDimension;
			PlayerEvents.EscapingPocketDimension -= Handlers.OnEscapingPocketDimension;

			Scp914Events.Activating -= Handlers.On914Activating;
			Scp914Events.UpgradingItems -= Handlers.On914Upgrade;
			//LogStaff
			PlayerEvents.Banning -= Handlers.OnBanning;
			PlayerEvents.Kicking -= Handlers.OnKicking;
			ServerEvents.SendingRemoteAdminCommand -= Handlers.OnSendingRemoteAdminCommand;
			Handlers = null;
		}

		private void RegistPatch()
		{
			try
			{
				Harmony = new Harmony(Author + "." + Name + ++patchCounter);
				Harmony.PatchAll();
			}
			catch (Exception ex)
			{
				Log.Error($"[RegistPatch] Patching Failed : {ex}");
			}
		}

		private void UnRegistPatch()
		{
			Harmony.UnpatchAll();
		}

		public IEnumerator<float> RunSendWebhook()
		{
			for (; ; )
			{
				yield return Timing.WaitForSeconds(0.5f);
				if (LOG != null)
				{
					Webhook.SendWebhook(LOG);
					LOG = null;
				}
			}
		}
		public IEnumerator<float> RunUpdateWebhook()
		{
			for (; ; )
			{
				yield return Timing.WaitForSeconds(1f);
				UpdateWebhook();
			}
		}
		public void UpdateWebhook()
		{
			string RoundInfo;
			string RoundTime;
			if (Round.IsStarted)
			{
				if (Round.IsLocked)
				{
					RoundInfo = "La partie est bloquée";
					RoundTime = $"Durée de la partie - {RoundSummary.roundTime / 60:00}:{RoundSummary.roundTime % 60:00}";
				}
				else
				{
					RoundInfo = "La partie est en cours";
					RoundTime = $"Durée de la partie - {RoundSummary.roundTime / 60:00}:{RoundSummary.roundTime % 60:00}";
				}
			}
			else if (!Round.IsStarted)
			{
				if (Round.IsLobbyLocked)
				{
					RoundInfo = "La partie est en pause";
					RoundTime = "** **";
				}
				else if (GameCore.RoundStart.singleton.NetworkTimer == -1)
				{
					RoundInfo = "La partie se termine";
					RoundTime = $"{RoundSummary.roundTime / 60:00}:{RoundSummary.roundTime % 60:00}";
				}
				else if (GameCore.RoundStart.singleton.NetworkTimer == -2)
				{
					RoundInfo = "En attente des joueurs";
					RoundTime = $"{2 - Player.List.ToList().Count} {(2 - Player.List.ToList().Count <= 1 ? "joueur manquant" : "joueurs manquants")}";
				}
				else if (GameCore.RoundStart.singleton.NetworkTimer <= 0)
				{
					RoundInfo = "En attente des joueurs";
					RoundTime = $"Départ de la game dans {GameCore.RoundStart.singleton.NetworkTimer} seconde{(GameCore.RoundStart.singleton.NetworkTimer <= 1 ? "" : "s")}";
				}
				else
				{
					RoundInfo = "Error";
					RoundTime = "Error";
				}
			}
			else
			{
				RoundInfo = "Error";
				RoundTime = "Error";
			}
			Webhook.UpdateServerInfo(RoundInfo, RoundTime);
		}
	}
}