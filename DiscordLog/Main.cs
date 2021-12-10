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


		public Dictionary<Player, string> NormalisedName = new Dictionary<Player, string>();


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

			if (Instance.Config.WebhookUrlLogError != string.Empty)
				EventHandlers.Coroutines.Add(Timing.RunCoroutine(RunSendLogError()));

			if (Instance.Config.WebhookUrlLogJoueur != string.Empty)
				EventHandlers.Coroutines.Add(Timing.RunCoroutine(RunSendWebhook()));
            if (Instance.Config.WebhookSi != "null" || Instance.Config.IdMessage != "null" )
				EventHandlers.Coroutines.Add(Timing.RunCoroutine(RunUpdateWebhook()));
			foreach (Player p in Player.List)
			{
				string PlayerName = p.Nickname.Normalize(System.Text.NormalizationForm.FormKD);
				if (PlayerName.Length < 18)
					NormalisedName.Add(p, $"[{p.Id}] {PlayerName}");
				else
					NormalisedName.Add(p, $"[{p.Id}] {PlayerName.Remove(17)}");
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

			PlayerEvents.Hurting += Handlers.OnPlayerHurt;
			PlayerEvents.Died += Handlers.OnPlayerDeath;
			PlayerEvents.DroppingItem += Handlers.OnDroppingItem;
			PlayerEvents.PickingUpItem += Handlers.OnPickingUpItem;
			PlayerEvents.PickingUpArmor += Handlers.OnPickingUpArmor;
			//PlayerEvents.PickingUpScp330 += Handlers.OnPickingUpScp330;
			Scp330Events.EatenScp330 += Handlers.OnEatenScp330;
			PlayerEvents.ItemUsed += Handlers.OnPlayerUsedItem;

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
			PlayerEvents.Hurting -= Handlers.OnPlayerHurt;
			PlayerEvents.Died -= Handlers.OnPlayerDeath;
			PlayerEvents.DroppingItem -= Handlers.OnDroppingItem;
			PlayerEvents.PickingUpItem -= Handlers.OnPickingUpItem;
			PlayerEvents.PickingUpArmor -= Handlers.OnPickingUpArmor;
			//PlayerEvents.PickingUpScp330 -= Handlers.OnPickingUpScp330;
			Scp330Events.EatenScp330 -= Handlers.OnEatenScp330;
			PlayerEvents.ItemUsed -= Handlers.OnPlayerUsedItem;

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
				if (LOGError != null)
				{
					if (LOGError.Length < 2001)
					{
						Webhook.SendWebhookError(LOGError);
						LOGError = null;
					}
					else
					{
						int Limiteur = 0;
						string LogLimite = string.Empty;
						string logs = LOGError;
						LOGError = null;
						List<string> LogToSend = new List<string>();
						foreach (string ligne in logs.Split('\n'))
						{
							if (ligne.Count() + Limiteur < 1999)
							{
								Limiteur += ligne.Count() + 1;
								LogLimite += ligne + "\n";
							}
							else
							{
								LogToSend.Add(LogLimite);
								LogLimite = string.Empty;
								Limiteur = 0;
								Limiteur = ligne.Count() + 1;
								LogLimite = ligne + "\n";
							}
						}
						LogToSend.Add(LogLimite);
						foreach (string SendLog in LogToSend)
						{
							Webhook.SendWebhookError(SendLog);
							yield return Timing.WaitForSeconds(5f);
						}
					}
				}
			}
		}
		public IEnumerator<float> RunSendWebhook()
		{
			while(true)
			{
				yield return Timing.WaitForSeconds(1f);
				if (LOG != null)
				{
					if (LOG.Length < 2001)
					{
						Webhook.SendWebhook(LOG);
						LOG = null;
					}
					else
                    {
						int Limiteur = 0;
						string LogLimite = string.Empty;
						string logs = LOG;
						LOG = null;
						List<string> LogToSend = new List<string>();
						foreach (string ligne in logs.Split('\n'))
						{
							if (ligne.Count() + Limiteur < 1999)
							{
								Limiteur += ligne.Count() + 1;
								LogLimite += ligne + "\n";
							}
							else
                            {
								LogToSend.Add(LogLimite);
								LogLimite = string.Empty;
								Limiteur = 0;
								Limiteur = ligne.Count() + 1;
								LogLimite = ligne + "\n";
							}
						}
						LogToSend.Add(LogLimite);
						foreach (string SendLog in LogToSend)
                        {
							Webhook.SendWebhook(SendLog);
							yield return Timing.WaitForSeconds(0.25f);
						}
					}
				}
				if (LOGStaff != null)
				{
					yield return Timing.WaitForSeconds(0.25f);
					if (LOGStaff.Length < 2001)
					{
						Webhook.SendWebhookStaff(LOGStaff);
						LOGStaff = null;
					}
					else
					{
						int Limiteur = 0;
						string LogLimite = string.Empty;
						string logs = LOGStaff;
						LOGStaff = null;
						List<string> LogToSend = new List<string>();
						foreach (string ligne in logs.Split('\n'))
						{
							if (ligne.Count() + Limiteur < 1999)
							{
								Limiteur += ligne.Count() + 1;
								LogLimite += ligne + "\n";
							}
							else
							{
								LogToSend.Add(LogLimite);
								Limiteur = ligne.Count() + 1;
								LogLimite = ligne + "\n";
							}
						}
						LogToSend.Add(LogLimite);
						foreach (string SendLog in LogToSend)
						{
							Webhook.SendWebhookStaff(SendLog);
							yield return Timing.WaitForSeconds(0.25f);
						}
					}
				}
			}
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
			else if (!Round.IsStarted)
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
				else if (GameCore.RoundStart.singleton.NetworkTimer >= 0 || GameCore.RoundStart.singleton.NetworkTimer == -2)
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
			string PlayerNameList = "";
			string PlayerRoleList = "";
			string UserIdList = "";
			if (Player.List.Count() != 0)
			{
				foreach (Player player in Player.List) 
				{
					NormalisedName.TryGetValue(player, out string PlayerName);
					PlayerNameList += $"{PlayerName}\n";
					if (player.Team == Team.RIP)
						if (player.IsOverwatchEnabled)
							PlayerRoleList += $"Overwatch\n";
						else
							PlayerRoleList += $"{player.Role}\n";
					else if (player.TryGetSessionVariable("NewRole", out Tuple<string,string> NewRole))
						PlayerRoleList += $"{NewRole.Item1}({(player.IsGodModeEnabled ? $"GodMod": $"{(int)player.Health}Hp")})\n";
					else if (player.Role == RoleType.Scp079)
						PlayerRoleList += $"Scp079({(player.IsGodModeEnabled ? $"GodMod" : $"{Map.ActivatedGenerators}/3 Gen")})\n";
					else
						PlayerRoleList += $"{player.Role}({(player.IsGodModeEnabled ? $"GodMod" : $"{(int)player.Health}Hp")})\n";
                    UserIdList += $"{player.UserId}\n";
                }
            }
			Webhook.UpdateServerInfoStaffAsync(RoundInfo, RoundTime, PlayerNameList, PlayerRoleList, UserIdList);
		}
	}
}