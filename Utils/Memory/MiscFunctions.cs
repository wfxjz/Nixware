using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Memory.DynamicFunctions;

namespace ChaseMod.Utils.Memory;
public static class MiscFunctions
{
    public static MemoryFunctionVoid<CCSPlayerController, CCSPlayerPawn, bool, bool> CBasePlayerController_SetPawnFunc =
        new(
            ChaseModUtils.IsLinux
                ? @"\x55\x48\x89\xE5\x41\x57\x41\x56\x41\x55\x41\x54\x49\x89\xFC\x53\x48\x89\xF3\x48\x81\xEC\xC8\x00\x00\x00"
                : @"\x44\x88\x4C\x24\x2A\x55\x57"
        );

	public static MemoryFunctionVoid<IntPtr> CCSMatch_UpdateTeamScores = 
        new(
            ChaseModUtils.IsLinux
                ? @"\x55\x48\x89\xE5\x41\x57\x41\x56\x41\x55\x49\x89\xFD\xBF\x02\x00\x00\x00"
                : @"\x48\x89\x5C\x24\x2A\x48\x89\x74\x24\x2A\x48\x89\x7C\x24\x2A\x41\x56\x48\x83\xEC\x20\x48\x8B\xF9\xB9\x02\x00\x00\x00"
        );
}
