using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Memory.DynamicFunctions;

namespace ChaseMod.Utils.Memory;
public static class MiscFunctions
{
	// string search '#SFUIHUD_InfoPanel_Coop_DeployMissionBust', xref and takefunc before if statement containing + 0xF3C in
    // param_1 and value 1 in param_2, follow the last function called taking param_1 (just hope this offset doesn't change)
    // TODO: I can remove probably this sig later
	public static MemoryFunctionVoid<IntPtr> CCSMatch_UpdateTeamScores = 
        new(
            ChaseModUtils.IsLinux
                ? @"\x55\x48\x89\xE5\x41\x56\x41\x55\x49\x89\xFD\xBF\x02\x00\x00\x00"
                : @"\x48\x89\x5C\x24\x2A\x48\x89\x74\x24\x2A\x48\x89\x7C\x24\x2A\x41\x56\x48\x83\xEC\x20\x48\x8B\xF9\xB9\x02\x00\x00\x00"
        );
}
