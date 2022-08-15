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
using Scp049Events = Exiled.Events.Handlers.Scp049;
using Scp330Events = Exiled.Events.Handlers.Scp330;
using Scp244Events = Exiled.Events.Handlers.Scp244;

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
		public static DiscordLog Instance { get; private set; }
		public EventHandlers Handlers { get; private set; }

		public DiscordLog() => Instance = this;
		public Harmony Harmony { get; private set; }
		public string LOG = null;
		public string LOGStaff = null;
		public string LOGError = null;


		public Dictionary<Player, string> NormalisedName = new();


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
		public override void OnReloaded()
		{
            foreach (CoroutineHandle handle in EventHandlers.Coroutines)
                Timing.KillCoroutines(handle);
			base.OnReloaded();
			NormalisedName.Clear();

			if (!string.IsNullOrWhiteSpace(Instance.Config.WebhookUrlLogError))
				EventHandlers.Coroutines.Add(Timing.RunCoroutine(RunSendLogError()));
			if (!string.IsNullOrWhiteSpace(Instance.Config.WebhookUrlLogJoueur))
				EventHandlers.Coroutines.Add(Timing.RunCoroutine(RunSendWebhook()));
			if (!string.IsNullOrWhiteSpace(Instance.Config.WebhookUrlLogStaff))
				EventHandlers.Coroutines.Add(Timing.RunCoroutine(RunSendWebhookStaff()));
			if (Instance.Config.WebhookSi is not "null" || Instance.Config.IdMessage is not "null" )
				EventHandlers.Coroutines.Add(Timing.RunCoroutine(RunUpdateWebhook()));

			foreach (Player p in Player.List)
			{
				string PlayerName = p.Nickname.Normalize(System.Text.NormalizationForm.FormKD);
				if (PlayerName.Length < 17)
					NormalisedName.Add(p, $"[{p.Id}] {PlayerName}");
				else
					NormalisedName.Add(p, $"[{p.Id}] {PlayerName.Remove(16)}");
			}
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

			MapEvents.Decontaminating += Handlers.OnDecontaminating;
			MapEvents.GeneratorActivated += Handlers.OnGeneratorFinish;

			PlayerEvents.PreAuthenticating += Handlers.OnPlayerAuth;
			PlayerEvents.Verified += Handlers.OnPlayerVerified;
			PlayerEvents.Destroying += Handlers.OnPlayerDestroying;
			PlayerEvents.ChangingRole += Handlers.OnChangingRole;

			PlayerEvents.Died += Handlers.OnPlayerDeath;
			PlayerEvents.DroppingItem += Handlers.OnDroppingItem;
			PlayerEvents.PickingUpItem += Handlers.OnPickingUpItem;
			Scp330Events.DroppingScp330 += Handlers.OnDroppingUpScp330;
			Scp330Events.EatenScp330 += Handlers.OnEatenScp330;
			Scp330Events.InteractingScp330 += Handlers.OnInteractingScp330;
			Scp244Events.OpeningScp244 += Handlers.OnOpeningScp244;
			Scp244Events.DamagingScp244 += Handlers.OnDamagingScp244;
			Scp244Events.UsingScp244 += Handlers.OnUsingScp244;
			MapEvents.ExplodingGrenade += Handlers.OnExplodingGrenade;
			PlayerEvents.UsedItem += Handlers.OnPlayerUsedItem;

			PlayerEvents.UnlockingGenerator += Handlers.OnGeneratorUnlock;
			PlayerEvents.StoppingGenerator += Handlers.OnStoppingGenerator;
			PlayerEvents.ActivatingGenerator += Handlers.OnActivatingGenerator;
			PlayerEvents.ActivatingWarheadPanel += Handlers.OnActivatingWarheadPanel;
			PlayerEvents.IntercomSpeaking += Handlers.OnIntercomSpeaking;
			PlayerEvents.Handcuffing += Handlers.OnHandcuffing;
			PlayerEvents.RemovingHandcuffs += Handlers.OnRemovingHandcuffs;

			PlayerEvents.EnteringPocketDimension += Handlers.OnEnteringPocketDimension;
			PlayerEvents.EscapingPocketDimension += Handlers.OnEscapingPocketDimension;
			Scp914Events.Activating += Handlers.On914Activating;
			Scp049Events.FinishingRecall += Handlers.OnFinishingRecall;

			//LogStaff
			PlayerEvents.Banning += Handlers.OnBanning;
			PlayerEvents.Kicking += Handlers.OnKicking;
			PlayerEvents.Banned += Handlers.OnBanned;
			//PingStaff
			ServerEvents.LocalReporting += Handlers.OnLocalReporting;
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

			MapEvents.Decontaminating -= Handlers.OnDecontaminating;
			MapEvents.GeneratorActivated -= Handlers.OnGeneratorFinish;

			PlayerEvents.PreAuthenticating -= Handlers.OnPlayerAuth;
			PlayerEvents.Verified -= Handlers.OnPlayerVerified;
			PlayerEvents.Destroying -= Handlers.OnPlayerDestroying;
			PlayerEvents.ChangingRole -= Handlers.OnChangingRole;
			PlayerEvents.Died -= Handlers.OnPlayerDeath;
			PlayerEvents.DroppingItem -= Handlers.OnDroppingItem;
			PlayerEvents.PickingUpItem -= Handlers.OnPickingUpItem;
			Scp330Events.DroppingScp330 -= Handlers.OnDroppingUpScp330;
			Scp330Events.EatenScp330 -= Handlers.OnEatenScp330;
			Scp330Events.InteractingScp330 -= Handlers.OnInteractingScp330;
			Scp244Events.OpeningScp244 -= Handlers.OnOpeningScp244;
			Scp244Events.DamagingScp244 -= Handlers.OnDamagingScp244;
			Scp244Events.UsingScp244 -= Handlers.OnUsingScp244;
			MapEvents.ExplodingGrenade -= Handlers.OnExplodingGrenade;

			PlayerEvents.UsedItem -= Handlers.OnPlayerUsedItem;

			PlayerEvents.UnlockingGenerator -= Handlers.OnGeneratorUnlock;
			PlayerEvents.StoppingGenerator -= Handlers.OnStoppingGenerator;
			PlayerEvents.ActivatingGenerator -= Handlers.OnActivatingGenerator;
			PlayerEvents.ActivatingWarheadPanel -= Handlers.OnActivatingWarheadPanel;
			PlayerEvents.IntercomSpeaking -= Handlers.OnIntercomSpeaking;
			PlayerEvents.Handcuffing -= Handlers.OnHandcuffing;
			PlayerEvents.RemovingHandcuffs -= Handlers.OnRemovingHandcuffs;

			PlayerEvents.EnteringPocketDimension -= Handlers.OnEnteringPocketDimension;
			PlayerEvents.EscapingPocketDimension -= Handlers.OnEscapingPocketDimension;
			Scp914Events.Activating -= Handlers.On914Activating;

			Scp049Events.FinishingRecall -= Handlers.OnFinishingRecall;

			//LogStaff
			PlayerEvents.Banning -= Handlers.OnBanning;
			PlayerEvents.Kicking -= Handlers.OnKicking;
			PlayerEvents.Banned -= Handlers.OnBanned;

			//PingStaff
			ServerEvents.LocalReporting -= Handlers.OnLocalReporting;
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
		public IEnumerator<float> RunSendLogError()
        {
			while (true)
			{
				yield return Timing.WaitForSeconds(10f);
				if (LOGError is null)
					continue;
				if (LOGError.Length < 2001)
				{
					Webhook.SendWebhookError(LOGError);
					LOGError = null;
				}
				else
				{
					DiscordMessage(LOGError, out List<string> ListString);
					LOGError = null;
					foreach (string SendLog in ListString)
					{
						Webhook.SendWebhookError(SendLog);
						yield return Timing.WaitForSeconds(5f);
					}
				}
			}
		}
		public IEnumerator<float> RunSendWebhook()
		{
			while(true)
			{
				yield return Timing.WaitForSeconds(1f);
				if (LOG is null)
					continue;
				if (LOG.Length < 2001)
				{
					Webhook.SendWebhook(LOG);
					LOG = null;
					continue;
				}

				DiscordMessage(LOG, out List<string> ListString);
				LOG = null;
				foreach (string SendLog in ListString)
				{
					Webhook.SendWebhook(SendLog);
					yield return Timing.WaitForSeconds(0.25f);
				}
			}
		}

		public IEnumerator<float> RunSendWebhookStaff()
		{
			while (true)
			{
				yield return Timing.WaitForSeconds(1f);
				if (LOGStaff is null)
					continue;
				if (LOGStaff.Length < 2001)
				{
					Webhook.SendWebhookStaff(LOGStaff);
					LOGStaff = null;
					continue;
				}

				DiscordMessage(LOGStaff, out List<string> ListString);
				LOGStaff = null;
				foreach (string SendLog in ListString)
				{
					Webhook.SendWebhookStaff(SendLog);
					yield return Timing.WaitForSeconds(0.25f);
				}
			}
		}

		public void DiscordMessage(string message, out List<string> ListString)
        {
			int Limiteur = 0;
			string LogLimite = string.Empty;
			ListString = new();
			foreach (string ligne in message.Split('\n'))
			{
				if (ligne.Count() + Limiteur < 1996)
				{
					Limiteur += ligne.Count() + 1;
					LogLimite += ligne + "\n_ _";
				}
				else
				{
					ListString.Add(LogLimite);
					Limiteur = ligne.Count() + 1;
					LogLimite = ligne + "\n";
				}
			}
			ListString.Add(LogLimite);
		}
		public IEnumerator<float> RunUpdateWebhook()
		{
			yield return Timing.WaitForSeconds(4f);

			while(true)
			{
				yield return Timing.WaitForSeconds(1f);
				UpdateWebhook();
			}
		}
		public void UpdateWebhook()
		{
			string RoundInfo;
			string RoundTime;
			int PlayerCount = Player.List.Where((p) => !p.IsOverwatchEnabled).Count();
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
			else
			{
				if (Round.IsLobbyLocked)
				{
					if (PlayerCount < 2)
					{
						RoundInfo = "En attente des joueurs";
						RoundTime = $"{2 - PlayerCount} {(2 - PlayerCount <= 1 ? "joueur manquant" : "joueurs manquants")}";
					}
					else
					{
						RoundInfo = "En attente des joueurs";
						RoundTime = $"Le lobby est lock";
					}
				}
				else if (GameCore.RoundStart.singleton.NetworkTimer == -1)
				{
					if (RoundSummary.roundTime == 0)
					{
						RoundInfo = "La partie est en cours";
						RoundTime = $"Durée de la partie - {RoundSummary.roundTime / 60:00}:{RoundSummary.roundTime % 60:00}";
					}
					else
					{
						RoundInfo = "La partie se termine";
						RoundTime = $"Durée de la partie - {RoundSummary.roundTime / 60:00}:{RoundSummary.roundTime % 60:00}";
					}
				}
				else if (GameCore.RoundStart.singleton.NetworkTimer == -2)
				{
					if (PlayerCount < 2)
					{
						RoundInfo = "En attente des joueurs";
						RoundTime = $"{2 - PlayerCount} {(2 - PlayerCount <= 1 ? "joueur manquant" : "joueurs manquants")}";
					}
					else
					{
						RoundInfo = "En attente des joueurs";
						RoundTime = $"Départ de la game dans 30 secondes";
					}
				}
				else
				{
					RoundInfo = "En attente des joueurs";
					RoundTime = $"Départ de la game dans {GameCore.RoundStart.singleton.NetworkTimer} seconde{(GameCore.RoundStart.singleton.NetworkTimer <= 1 ? "" : "s")}";
				}
			}
			_ = Webhook.UpdateServerInfo(RoundInfo, RoundTime);
			string PlayerNameList = "";
			string PlayerRoleList = "";
			string UserIdList = "";
			if (Player.List.Count() != 0)
			{
				foreach (Player player in Player.List) 
				{
					NormalisedName.TryGetValue(player, out string PlayerName);
					PlayerNameList += $"{PlayerName}\n";
					if (player.Role.Team == Team.RIP)
						if (player.IsOverwatchEnabled)
							PlayerRoleList += $"Overwatch\n";
						else
							PlayerRoleList += $"{player.Role.Type}\n";
					else if (player.TryGetSessionVariable("NewRole", out Tuple<string,string> NewRole))
						PlayerRoleList += $"{NewRole.Item1}({(player.IsGodModeEnabled ? $"GodMod": $"{(int)player.Health}Hp")})\n";
					else if (player.Role == RoleType.Scp079)
						PlayerRoleList += $"Scp079({(player.IsGodModeEnabled ? $"GodMod" : $"{Generator.Get(GeneratorState.Engaged).Count()}/3 Gen")})\n";
					else
						PlayerRoleList += $"{player.Role.Type}({(player.IsGodModeEnabled ? $"GodMod" : $"{(int)player.Health}Hp")})\n";
                    UserIdList += $"{player.UserId}\n";
                }
            }
			_ = Webhook.UpdateServerInfoStaffAsync(RoundInfo, RoundTime, PlayerNameList, PlayerRoleList, UserIdList);
		}
	}
}