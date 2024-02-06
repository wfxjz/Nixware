using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Logging;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Memory.DynamicFunctions;
using CounterStrikeSharp.API.Modules.Utils;
using Microsoft.Extensions.Logging;

namespace ChaseMod;
internal class KnifeCooldownManager
{
    private static ILogger Logger = CoreLogging.Factory.CreateLogger("KnifeCooldownManager");

    private readonly Dictionary<CBasePlayerController, DateTime> invulnerablePlayers = new();

    private ChaseMod Plugin;

    public KnifeCooldownManager(ChaseMod chaseMod)
    {
        this.Plugin = chaseMod;
    }

    public void EnableHooks()
    {
        VirtualFunctions.CBaseEntity_TakeDamageOldFunc.Hook(CBaseEntity_TakeDamageOldFuncHook, HookMode.Pre);
    }

    public void DisableHooks()
    {
        VirtualFunctions.CBaseEntity_TakeDamageOldFunc.Unhook(CBaseEntity_TakeDamageOldFuncHook, HookMode.Pre);
    }

    private HookResult CBaseEntity_TakeDamageOldFuncHook(DynamicHook hook)
    {
        var entity = hook.GetParam<CEntityInstance>(0);
        var info = hook.GetParam<CTakeDamageInfo>(1);

        if (!entity.IsValid || !info.Attacker.IsValid)
        {
            return HookResult.Continue;
        }

        if (entity.DesignerName != "player" || info.Attacker.Value!.DesignerName != "player")
        {
            return HookResult.Continue;
        }

        var attacker = info.Attacker.Value!.As<CCSPlayerPawn>();
        var attackerController = attacker.Controller.Value!;

        var pawn = entity.As<CCSPlayerPawn>();
        var controller = pawn.OriginalController.Value!;

        Server.PrintToConsole(controller.PlayerName + ", " + attackerController.PlayerName);

        if (attacker.WeaponServices == null || !attacker.WeaponServices.ActiveWeapon.IsValid)
        {
            return HookResult.Continue;
        }

        var weapon = attacker.WeaponServices.ActiveWeapon.Value!;

        if (weapon.DesignerName != "weapon_knife")
        {
            return HookResult.Continue;
        }

        // if attacked player is counter-terrorist, ignore damage from knife
        if (controller.TeamNum == (byte)CsTeam.CounterTerrorist)
        {
            return HookResult.Handled;
        }

        // if attacked player is not terrorist, handle normally?
        if (controller.TeamNum != (byte)CsTeam.Terrorist)
        {
            return HookResult.Continue;
        }

        if (invulnerablePlayers.TryGetValue(controller, out DateTime expiry))
        {
            if (expiry > DateTime.Now)
            {
                return HookResult.Handled;
            }
        }

        info.Damage = this.Plugin.Config.knifeDamage;
        info.DamageFlags |= TakeDamageFlags_t.DFLAG_SUPPRESS_PHYSICS_FORCE;
        invulnerablePlayers[controller] = DateTime.Now.AddSeconds(this.Plugin.Config.knifeCooldown);
        return HookResult.Continue;
    }

}
