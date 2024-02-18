using CounterStrikeSharp.API.Core.Logging;
using CounterStrikeSharp.API.Core;
using Microsoft.Extensions.Logging;
using ChaseMod.Utils.Memory;
using CounterStrikeSharp.API.Modules.Memory.DynamicFunctions;
using Timer = CounterStrikeSharp.API.Modules.Timers.Timer;
using CounterStrikeSharp.API;
using ChaseMod.Utils;
using System.Drawing;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API.Modules.Memory;

namespace ChaseMod;
internal class NadeManager
{
	private static ILogger Logger = CoreLogging.Factory.CreateLogger("FreezeNadeManager");

	private ChaseMod Plugin;

	public NadeManager(ChaseMod chaseMod)
	{
		this.Plugin = chaseMod;
	}

	public void EnableHooks()
	{
		GrenadeFunctions.CSmokeGrenadeProjectile_CreateFunc.Hook(CSmokeGrenadeProjectile_CreateHook, HookMode.Post);
		this.Plugin.RegisterEventHandler<EventPlayerBlind>((@event, info) =>
		{
			if (!ChaseModUtils.IsRealPlayer(@event.Attacker) || !ChaseModUtils.IsRealPlayer(@event.Userid))
			{
				return HookResult.Continue;
			}

			if (@event.Attacker.Team == @event.Userid.Team)
			{
				@event.Userid.PlayerPawn.Value!.BlindUntilTime = 0;
			}

			return HookResult.Continue;
		});
	}

	public void DisableHooks()
	{
		GrenadeFunctions.CSmokeGrenadeProjectile_CreateFunc.Unhook(CSmokeGrenadeProjectile_CreateHook, HookMode.Post);
	}

	private HookResult CSmokeGrenadeProjectile_CreateHook(DynamicHook hook)
	{
		var smoke = new CSmokeGrenadeProjectile(hook.GetReturn<IntPtr>(0));
		Logger.LogInformation("freezenade thrown");

		smoke.NextThinkTick = -1;
		
		new Timer(this.Plugin.Config.stunThrowTime, () =>
		{
			Server.NextFrame(() =>
			{
				if (!smoke.IsValid)
				{
					return;
				}

				var decoyCoord = smoke.AbsOrigin;
				if (decoyCoord == null)
				{
					return;
				}

				var player = smoke.OwnerEntity;
				if (!player.IsValid || player.Value == null)
				{
					return;
				}

				var players = ChaseModUtils.GetAllRealPlayers();
				foreach (var other in players)
				{
					if (!other.IsValid || other.PlayerPawn.Value == null || !other.PlayerPawn.IsValid || !other.PawnIsAlive || other.PlayerPawn.Value.LifeState != (byte)LifeState_t.LIFE_ALIVE)
					{
						continue;
					}

					if (!this.Plugin.Config.stunSameTeam && other.TeamNum == player.Value.TeamNum)
					{
						continue;
					}

					var pawn = other.PlayerPawn.Value;
					if (pawn == null)
					{
						continue;
					}

					var pcCoord = pawn.CBodyComponent?.SceneNode?.AbsOrigin;
					if (pcCoord == null)
					{
						Logger.LogWarning("freezenade: other pawn has null AbsOrigin");
						continue;
					}

					var distance = pcCoord.Distance(decoyCoord);
					Server.PrintToConsole($"{other.PlayerName} {distance}");
					if (distance <= this.Plugin.Config.stunFreezeRadius)
					{
						pawn.Render = Color.FromArgb(255, 4, 58, 140);
						Utilities.SetStateChanged(pawn, "CBaseModelEntity", "m_clrRender");

						pawn.MoveType = MoveType_t.MOVETYPE_OBSOLETE;
						Utilities.SetStateChanged(pawn, "CBaseEntity", "m_MoveType");

						Schema.GetRef<MoveType_t>(pawn.Handle, "CBaseEntity", "m_nActualMoveType") = MoveType_t.MOVETYPE_OBSOLETE;

						pawn.HealthShotBoostExpirationTime = Server.CurrentTime + this.Plugin.Config.stunFreezeTime;
						Utilities.SetStateChanged(pawn, "CCSPlayerPawn", "m_flHealthShotBoostExpirationTime");

						var origVelocity = pawn.AbsVelocity.With();
						pawn.AbsVelocity.X = 0;
						pawn.AbsVelocity.Y = 0;
						pawn.AbsVelocity.Z = 0;

						var weapons = pawn.WeaponServices?.MyWeapons;
						if (weapons != null)
						{
							foreach (var item in weapons)
							{
								var weapon = item.Value;
								if (weapon == null) continue;

								weapon.NextPrimaryAttackTick = int.MaxValue;
								weapon.NextSecondaryAttackTick = int.MaxValue;
								Utilities.SetStateChanged(weapon, "CBasePlayerWeapon", "m_nNextPrimaryAttackTick");
								Utilities.SetStateChanged(weapon, "CBasePlayerWeapon", "m_nNextSecondaryAttackTick");
							}
						}

						ChaseModUtils.ChatPrefixed(other, $" {ChatColors.DarkRed}⚠{ChatColors.Grey} You have been {ChatColors.DarkRed}frozen{ChatColors.Grey}.");

						if (!this.Plugin.playerStates.ContainsKey(other))
						{
							this.Plugin.playerStates[other] = new PlayerState();
						}

						this.Plugin.playerStates[other].frozen = true;

						new Timer(this.Plugin.Config.stunFreezeTime, () =>
						{
							if (!other.PlayerPawn.IsValid) return;

							pawn.Render = Color.White;
							Utilities.SetStateChanged(pawn, "CBaseModelEntity", "m_clrRender");

							pawn.MoveType = MoveType_t.MOVETYPE_WALK;
							Utilities.SetStateChanged(pawn, "CBaseEntity", "m_MoveType");

							Schema.GetRef<MoveType_t>(pawn.Handle, "CBaseEntity", "m_nActualMoveType") = MoveType_t.MOVETYPE_WALK;

							pawn.AbsVelocity.X = origVelocity.X;
							pawn.AbsVelocity.Y = origVelocity.Y;
							pawn.AbsVelocity.Z = origVelocity.Z;

							var weapons = pawn.WeaponServices?.MyWeapons;
							if (weapons != null)
							{
								foreach (var item in weapons)
								{
									var weapon = item.Value;
									if (weapon == null) continue;

									weapon.NextPrimaryAttackTick = 0;
									weapon.NextSecondaryAttackTick = 0;
									Utilities.SetStateChanged(weapon, "CBasePlayerWeapon", "m_nNextPrimaryAttackTick");
									Utilities.SetStateChanged(weapon, "CBasePlayerWeapon", "m_nNextSecondaryAttackTick");
								}
							}

							ChaseModUtils.ChatPrefixed(other, $" {ChatColors.Green}⚠{ChatColors.Grey} You are now {ChatColors.Green}unfrozen{ChatColors.Grey}.");
							this.Plugin.playerStates[other].frozen = false;

						});
					}
				}

				smoke.Remove();
			});

		});

		return HookResult.Continue;
	}

}
