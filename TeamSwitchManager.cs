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

    private bool _switchingTeams = false;
    private int _terroristWinstreak = 0;

    private ChaseMod _plugin;

    public TeamSwitchManager(ChaseMod chaseMod)
    {
        _plugin = chaseMod;
    }

    public void Start()
    {
        _plugin.RegisterEventHandler<EventRoundEnd>((@event, info) =>
        {
            var winner = @event.Winner;
            if (winner == (int)CsTeam.CounterTerrorist)
            {
                ChaseModUtils.ChatAllPrefixed($"{ChatColors.Blue}CT {ChatColors.Grey}Win - Teams are being switched.");
                _terroristWinstreak = 0;
                Server.NextFrame(() =>
                {
                    SwitchTeams();
                });
            }
            else if (winner == (int)CsTeam.Terrorist)
            {
                _terroristWinstreak++;
                if (_plugin.Config.MaxTerroristWinStreak > 0 && _terroristWinstreak >= _plugin.Config.MaxTerroristWinStreak)
                {
                    ChaseModUtils.ChatAllPrefixed($"{ChatColors.Yellow}T {ChatColors.Grey}Win - Teams are being switched due to winstreak. ({_terroristWinstreak} wins in a row)");
                    _terroristWinstreak = 0;
                    Server.NextFrame(() =>
                    {
                        SwitchTeams();
                    });
                }
                else
                {
                    ChaseModUtils.ChatAllPrefixed($"{ChatColors.Yellow}T {ChatColors.Grey}Win");
                }
            }
            return HookResult.Continue;
        });

        _plugin.RegisterEventHandler<EventPlayerTeam>((@event, info) =>
        {
            if (_switchingTeams)
            {
                info.DontBroadcast = true;
            }

            return HookResult.Continue;
        }, HookMode.Pre);

        _plugin.RegisterListener<Listeners.OnMapEnd>(() =>
        {
            _terroristWinstreak = 0;
        });
    }


    private void SwitchTeams()
    {
        _switchingTeams = true;
        var gameRules = ChaseModUtils.GetGameRules();
        if (gameRules != null)
        {
            CCSMatch.SwapTeamScores(gameRules);
        }

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
            controller.RemoveAllItemsOnNextRoundReset = true;
        }
        _switchingTeams = false;
    }


}

