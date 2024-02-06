using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API;
using System;
using CounterStrikeSharp.API.Modules.Entities.Constants;
using CounterStrikeSharp.API.Modules.Memory;
using System.Runtime.InteropServices;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API.Core.Logging;
using Microsoft.Extensions.Logging;

namespace ChaseMod.Utils
{
    public static class ChaseModUtils
    {

        private static ILogger Logger = CoreLogging.Factory.CreateLogger("ChaseModUtils");

        public static bool IsLinux => RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

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

        // these two from https://github.com/DEAFPS/SharpTimer
        public static void PrintHtml(CCSPlayerController player, string hudContent)
        {
            var @event = new EventShowSurvivalRespawnStatus(false)
            {
                LocToken = hudContent,
                Duration = 5,
                Userid = player
            };
            @event.FireEvent(false);

            @event = null;
        }
        public static void RemovePlayerCollision(CCSPlayerPawn pawn)
        {
            pawn.Collision.CollisionGroup = (byte)CollisionGroup.COLLISION_GROUP_DEBRIS;
            pawn.Collision.CollisionAttribute.CollisionGroup = (byte)CollisionGroup.COLLISION_GROUP_DEBRIS;
            pawn.SentToClients = 0;
            Utilities.SetStateChanged(pawn, "CBaseModelEntity", "m_Collision");

            VirtualFunctionVoid<nint> collisionRulesChanged = new VirtualFunctionVoid<nint>(pawn.Handle, RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? 173 : 172);
            collisionRulesChanged.Invoke(pawn.Handle);
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
}
