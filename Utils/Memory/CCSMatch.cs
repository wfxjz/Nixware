using CounterStrikeSharp.API.Core;
using System.Runtime.InteropServices;

namespace ChaseMod.Utils.Memory;
public class CCSMatch
{
	public static nint MATCH_OFFSET = 0xF3C;

	[StructLayout(LayoutKind.Sequential)]
	public struct MCCSGameRules
	{
		public short unknown;                       // 0xF3C
		public short m_actualRoundsPlayed;          // 0xF3E
		public short m_nOvertimePlaying;            // 0xF40
		public short m_ctScoreFirstHalf;            // 0xF42
		public short m_ctScoreSecondHalf;           // 0xF44
		public short m_ctScoreOvertime;             // 0xF46
		public short m_ctScoreTotal;                // 0xF48
		public short m_terroristScoreFirstHalf;     // 0xF4A
		public short m_terroristScoreSecondHalf;    // 0xF4C
		public short m_terroristScoreOvertime;      // 0xF4E
		public short m_terroristScoreTotal;         // 0xF50
	}

	public static void SwapTeamScores(CCSGameRules gameRules)
	{
		var structOffset = gameRules.Handle + MATCH_OFFSET;

		var marshallGameRules = Marshal.PtrToStructure<MCCSGameRules>(structOffset);

		short temp = marshallGameRules.m_terroristScoreFirstHalf;
		marshallGameRules.m_terroristScoreFirstHalf = marshallGameRules.m_ctScoreFirstHalf;
		marshallGameRules.m_ctScoreFirstHalf = temp;

		temp = marshallGameRules.m_terroristScoreSecondHalf;
		marshallGameRules.m_terroristScoreSecondHalf = marshallGameRules.m_ctScoreSecondHalf;
		marshallGameRules.m_ctScoreSecondHalf = temp;

		temp = marshallGameRules.m_terroristScoreOvertime;
		marshallGameRules.m_terroristScoreOvertime = marshallGameRules.m_ctScoreOvertime;
		marshallGameRules.m_ctScoreOvertime = temp;

		temp = marshallGameRules.m_terroristScoreTotal;
		marshallGameRules.m_terroristScoreTotal = marshallGameRules.m_ctScoreTotal;
		marshallGameRules.m_ctScoreTotal = temp;

		Marshal.StructureToPtr(marshallGameRules, structOffset, true);
		MiscFunctions.CCSMatch_UpdateTeamScores.Invoke(structOffset);
	}
}

