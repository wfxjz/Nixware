using CounterStrikeSharp.API.Core.Logging;
using CounterStrikeSharp.API.Core;
using Microsoft.Extensions.Logging;
using ChaseMod.Utils;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API;
using ChaseMod.Utils.Memory;

namespace ChaseMod;
internal class TeamSwitchManager
{
	private static ILogger Logger = CoreLogging.Factory.CreateLogger("KnifeCooldownManager");

	private bool switchingTeams = false;
	private int terroristWinstreak = 0;

	private ChaseMod Plugin;

	public TeamSwitchManager(ChaseMod chaseMod)
	{
		this.Plugin = chaseMod;
	}

	public void Start()
	{
		this.Plugin.RegisterEventHandler<EventRoundEnd>((@event, info) =>
		{
			var winner = @event.Winner;
			if (winner == (int)CsTeam.CounterTerrorist)
			{
				terroristWinstreak = 0;
				ChaseModUtils.ChatAllPrefixed($"{ChatColors.Blue}CT {ChatColors.Grey}Win - Teams are being switched.");
				Server.NextFrame(() =>
				{
					SwitchTeams();
				});
			}
			else if (winner == (int)CsTeam.Terrorist)
			{
				terroristWinstreak++;
				if (Plugin.Config.maxTerroristWinStreak > 0 && terroristWinstreak >= Plugin.Config.maxTerroristWinStreak)
				{
					ChaseModUtils.ChatAllPrefixed($"{ChatColors.Yellow}T {ChatColors.Grey}Win - Teams are being switched due to winstreak. ({terroristWinstreak} wins in a row)");
					Server.NextFrame(() =>
					{
						SwitchTeams();
					});
					terroristWinstreak = 0;
				}
				else
				{
					ChaseModUtils.ChatAllPrefixed($"{ChatColors.Yellow}T {ChatColors.Grey}Win");
				}
			}
			return HookResult.Continue;
		});

		this.Plugin.RegisterEventHandler<EventPlayerTeam>((@event, info) =>
		{
			if (switchingTeams)
			{
				info.DontBroadcast = true;
			}

			return HookResult.Continue;
		}, HookMode.Pre);

		this.Plugin.RegisterListener<Listeners.OnMapEnd>(() =>
		{
			this.terroristWinstreak = 0;
		});
	}


	private void SwitchTeams()
	{
		switchingTeams = true;
		CCSGameRules gameRules = Utilities.FindAllEntitiesByDesignerName<CCSGameRulesProxy>("cs_gamerules").First().GameRules!;
		CCSMatch.SwapTeamScores(gameRules);

		foreach (var controller in ChaseModUtils.GetAllRealPlayers())
		{
			var team = controller.Team;
			if (team == CsTeam.CounterTerrorist)
			{
				controller.SwitchTeam(CsTeam.Terrorist);
			}
			else if (team == CsTeam.Terrorist)
			{
				controller.SwitchTeam(CsTeam.CounterTerrorist);
			}
		}
		switchingTeams = false;
	}


}

