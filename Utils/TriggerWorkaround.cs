using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core.Logging;
using Microsoft.Extensions.Logging;

namespace ChaseMod.Utils;
public static class TriggerWorkaround
{
    private static ILogger Logger = CoreLogging.Factory.CreateLogger("ChaseModTriggerWorkaround");

    public static void HookTriggerOutput(ChaseMod plugin)
    {
        plugin.HookEntityOutput("trigger_multiple", "OnEndTouch", (output, name, activator, caller, value, delay) =>
        {
            if (activator.DesignerName != "player" || activator == null || caller == null)
                return HookResult.Continue;

            var pawn = new CCSPlayerPawn(activator.Handle);
            if (!pawn.IsValid) return HookResult.Continue;
            var player = new CCSPlayerController(pawn.Controller.Value!.Handle);

            if (!player.PawnIsAlive || player == null || caller.Entity!.Name == null)
                return HookResult.Continue;

            var connection = output.Connections;
            while (connection != null)
            {
                if (connection.TargetInput != "workaround")
                {
                    connection = connection.Next;
                    continue;
                }

                var splitValue = connection.ValueOverride.Split(" ");
                switch (splitValue[0])
                {
                    case "absvelocity":
                    {
                        if (splitValue.Length < 4)
                        {
                            break;
                        }
                        pawn.AbsVelocity.X += float.Parse(splitValue[1]) * plugin.Config.absvelocityWorkaroundMultiplier;
                        pawn.AbsVelocity.Y += float.Parse(splitValue[2]) * plugin.Config.absvelocityWorkaroundMultiplier;
                        pawn.AbsVelocity.Z += float.Parse(splitValue[3]) * plugin.Config.absvelocityWorkaroundMultiplier;
                        break;
                    }
                    default:
                    {
                        break;
                    }
                }

                connection = connection.Next;
            }

            return HookResult.Continue;
        });
    }

    public static void DisableWorkaroundTriggers()
    {
        Logger.LogTrace("DisableWorkaroundTriggers");
        foreach (var item in Utilities.FindAllEntitiesByDesignerName<CTriggerGravity>("trigger_gravity"))
        {
            if (!item.IsValid || item.Entity == null || item.Entity.Name != "boostworkaround.gravity")
                continue;

            Logger.LogTrace($"Trigger at {item.AbsOrigin?.X} {item.AbsOrigin?.Y} {item.AbsOrigin?.Z}");
            item.AcceptInput("Disable");
        }

    }

}

