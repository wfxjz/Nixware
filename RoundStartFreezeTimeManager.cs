using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Logging;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API.Modules.Timers;
using Microsoft.Extensions.Logging;
using ChaseMod.Utils;

using Timer = CounterStrikeSharp.API.Modules.Timers.Timer;

namespace ChaseMod;
internal class RoundStartFreezeTimeManager
{
    private static ILogger Logger = CoreLogging.Factory.CreateLogger("RoundStartFreezeTimeManager");

	private readonly ChaseMod _plugin;
	private readonly PlayerFreezeManager _playerFreezeManager;
	private float RoundStartTime;
	private int RoundStartTick;

	public float FrozenUntilTime => RoundStartTime + _plugin.Config.RoundStartFreezeTime;
	public int FrozenUntilTick => RoundStartTick + (int)(_plugin.Config.RoundStartFreezeTime / Server.TickInterval);

	private Timer? _countdownTimer;

	public RoundStartFreezeTimeManager(ChaseMod chaseMod, PlayerFreezeManager playerFreezeManager)
	{
		_plugin = chaseMod;
		_playerFreezeManager = playerFreezeManager;
	}
    
    public void Start()
	{
		_plugin.RegisterEventHandler<EventRoundFreezeEnd>((@event, info) =>
		{
			this.RoundStartTime = Server.CurrentTime;
			this.RoundStartTick = Server.TickCount;
			Logger.LogInformation("Freeze end");

			if (_plugin.Config.RoundStartFreezeTime <= 0)
			{
				return HookResult.Continue;
			}

            foreach (var player in ChaseModUtils.GetAllRealPlayers())
            {
				if (player.Team != CsTeam.CounterTerrorist)
				{
					continue;
				}

                var pawn = player.PlayerPawn.Value!;
                
				_playerFreezeManager.Freeze(player, _plugin.Config.RoundStartFreezeTime, true, false, true);
            }

			_countdownTimer = _plugin.AddTimer(0.1f, CountdownTimerTick, TimerFlags.REPEAT);

			return HookResult.Continue;
		});
	}

	private void CountdownTimerTick()
	{
		var timeLeft = FrozenUntilTime - Server.CurrentTime;
		if (timeLeft <= 0)
		{
			_countdownTimer?.Kill();
			_countdownTimer = null;
		}

		foreach (var player in ChaseModUtils.GetAllRealPlayers())
		{
			player.PrintToCenter(timeLeft > 0 ? $"Round begins in {timeLeft.ToString("0.0")} seconds!" : "Round start!");
		}
	}

	public bool IsInFreezeTime()
	{
		return Server.TickCount < FrozenUntilTick;
	}

}