using System.Linq;
using Content.Server.GameTicking;
using Content.Server.GameTicking.Rules;
using Robust.Shared.Player;

namespace Content.Server._BRatbite.PermaBrig;

/// <summary>
/// This handles...
/// </summary>
public sealed class PermaBrigSystem : GameRuleSystem<PermaBrigComponent>
{
    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RulePlayerSpawningEvent>(OnPlayerSpawning);
    }

    private void OnPlayerSpawning(RulePlayerSpawningEvent args)
    {
        var pool = args.PlayerPool;

        var manualSpawn = new List<ICommonSession>();

        foreach (var session in pool)
        {
            manualSpawn.Add(session);
            Logger.Debug($"Player being sent to perma: {session}");
        }

        foreach (var session in manualSpawn)
        {
            pool.Remove(session);
            GameTicker.PlayerJoinGame(session);
            Logger.Debug($"Player sent to perma: {session}");
        }
    }
}
