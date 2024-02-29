using ChaseMod.Utils;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes;
using CounterStrikeSharp.API.Modules.Utils;
using Microsoft.Extensions.Logging;
using ChaseMod.Commands;
using CounterStrikeSharp.API.Core.Logging;

namespace ChaseMod;

[MinimumApiVersion(141)]
public class ChaseMod : BasePlugin, IPluginConfig<ChaseModConfig>
{
    private static new ILogger Logger = CoreLogging.Factory.CreateLogger("ChaseModCS#");

    public override string ModuleName => "HnS ChaseMod";
    public override string ModuleAuthor => "svn";
    public override string ModuleVersion => "v1.0";

    public ChaseModConfig Config { get; set; }
    public void OnConfigParsed(ChaseModConfig config) { Config = config; }


    public readonly Dictionary<CCSPlayerController, PlayerState> playerStates = new();

    private NadeManager? nadeManager;
    private KnifeCooldownManager? knifeCooldownManager;
    private TeamSwitchManager? teamSwitchManager;

    public override void Load(bool hotReload)
    {
        nadeManager = new NadeManager(this);
        knifeCooldownManager = new KnifeCooldownManager(this);
        teamSwitchManager = new TeamSwitchManager(this); 

        if (hotReload)
        {
            // Use regular GetPlayers as we want all players to have PlayerState
            foreach (var controller in Utilities.GetPlayers())
            {
                playerStates[controller] = new PlayerState();
            }
        }

        RegisterEventHandler<EventPlayerJump>((player, info) =>
        {
            return HookResult.Continue;
        });

        MiscCommands.AddCommands(this);

        nadeManager.EnableHooks();
        knifeCooldownManager.EnableHooks();
        TriggerWorkaround.HookTriggerOutput(this);
        teamSwitchManager.Start();

        RegisterEventHandler<EventPlayerSpawned>((@event, info) =>
        {
            var player = @event.Userid;

            if (!player.IsValid)
            {
                return HookResult.Continue;
            }
            
            if (!player.Pawn.IsValid)
            {
                return HookResult.Continue;
            }

            var pawn = player.Pawn.Value;


            return HookResult.Continue;
          
        });

        RegisterEventHandler<EventPlayerConnectFull>((@event, info) =>
        {
            playerStates[@event.Userid] = new PlayerState();
            return HookResult.Continue;
        });

        RegisterEventHandler<EventPlayerDisconnect>((@event, info) =>
        {
            var player = @event.Userid;

            if (!player.IsValid)
            {
                return HookResult.Continue;
            }

            //KnifeCooldownManager.OnPlayerDisconnect(player);

            playerStates.Remove(player);

            return HookResult.Continue;
        });

        RegisterEventHandler<EventRoundFreezeEnd>((@event, info) =>
        {
            Server.NextFrame(() =>
            {
                TriggerWorkaround.DisableWorkaroundTriggers();
            });

            return HookResult.Continue;
        });
        
        RegisterListener<Listeners.OnTick>(() =>
        {
            foreach (var controller in ChaseModUtils.GetAllRealPlayers())
            {
                if (controller is not { IsBot: false, PawnIsAlive: true, LifeState: (byte)LifeState_t.LIFE_ALIVE } || !controller.Pawn.IsValid)
                {
                    continue;
                }

                var pawn = controller.Pawn.Value;
                if (pawn == null)
                {
                    continue;
                }

                if (controller.Team == CsTeam.CounterTerrorist)
                {
					var weapons = pawn.WeaponServices?.MyWeapons;
					if (weapons == null)
					{
						continue;
					}

					foreach (var weapon in weapons)
					{
						if (weapon.IsValid && weapon.Value?.DesignerName == "weapon_knife" && weapon.Value.NextPrimaryAttackTick <= Server.TickCount + (64 * 5))
						{
							weapon.Value.NextPrimaryAttackTick = Server.TickCount + (64 * 10);
							Utilities.SetStateChanged(weapon.Value, "CBasePlayerWeapon", "m_nNextPrimaryAttackTick");
						}
					}
				}

            }
        });

    }

    public override void Unload(bool hotReload)
    { 
        if (hotReload)
        {
            nadeManager?.DisableHooks();
            knifeCooldownManager?.DisableHooks();
        }
    }


  
}

