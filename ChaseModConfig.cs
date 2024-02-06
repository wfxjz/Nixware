using CounterStrikeSharp.API.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ChaseMod;
public sealed class ChaseModConfig : BasePluginConfig
{
    [JsonPropertyName("Version")]
    public override int Version { get; set; } = 2;

    [JsonPropertyName("ctStartFreezeTime")] public float ctStartFreezeTime { get; set; } = 15.0f;
    [JsonPropertyName("knifeDamage")] public int knifeDamage { get; set; } = 50;
    [JsonPropertyName("knifeCooldown")] public float knifeCooldown { get; set; } = 2.0f;
    [JsonPropertyName("stunThrowTime")] public float stunThrowTime { get; set; } = 2.0f;
    [JsonPropertyName("stunFreezeTime")] public float stunFreezeTime { get; set; } = 15.0f;
    [JsonPropertyName("stunFreezeRadius")] public float stunFreezeRadius { get; set; } = 500f;
    [JsonPropertyName("stunSameTeam")] public bool stunSameTeam { get; set; } = false;
    [JsonPropertyName("absvelocityWorkaroundMultiplier")] public float absvelocityWorkaroundMultiplier { get; set; } = 1.0f;
    [JsonPropertyName("maxTerroristWinStreak")] public int maxTerroristWinStreak { get; set; } = 5;
}