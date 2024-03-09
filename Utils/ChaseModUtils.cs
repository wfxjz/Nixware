using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API;
using System;
using CounterStrikeSharp.API.Modules.Entities.Constants;
using CounterStrikeSharp.API.Modules.Memory;
using System.Runtime.InteropServices;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API.Core.Logging;
using Microsoft.Extensions.Logging;

namespace ChaseMod.Utils;
public static class ChaseModUtils
{

    private static ILogger Logger = CoreLogging.Factory.CreateLogger("ChaseModUtils");

    public static bool IsLinux => RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

    public static CCSGameRules GetGameRules()
    {
        return Utilities.FindAllEntitiesByDesignerName<CCSGameRulesProxy>("cs_gamerules")
            .First().GameRules!;
    } 

    public static bool IsRealPlayer(CCSPlayerController p)
    {
        return
            p.IsValid &&
            p.PlayerPawn.IsValid && p.PlayerPawn.Value != null &&
            p.Connected == PlayerConnectedState.PlayerConnected &&
            !p.IsHLTV &&
            (
                p.Team == CsTeam.Terrorist || p.Team == CsTeam.CounterTerrorist
            );
    }

    public static List<CCSPlayerController> GetAllRealPlayers()
    {
        return Utilities.GetPlayers().Where(p => IsRealPlayer(p)).ToList();
    }

    public static void ChatPrefixed(CCSPlayerController player, string message)
    {
        player.PrintToChat($" {ChatColors.Magenta}HNS {ChatColors.Grey}| {message}");
    }
    public static void ChatAllPrefixed(string message)
    {
        Server.PrintToChatAll($" {ChatColors.Magenta}HNS {ChatColors.Grey}| {message}");
    }

}