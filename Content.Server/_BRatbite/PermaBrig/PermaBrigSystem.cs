using System.Linq;
using Content.Server.Administration.Systems;
using Content.Server.GameTicking;
using Content.Server.GameTicking.Rules;
using Content.Server.Players.PlayTimeTracking;
using Content.Server.Station.Systems;
using Content.Shared.GameTicking;
using Content.Shared.Mind;
using Content.Shared.Players;
using Content.Shared.Roles;
using Content.Shared.Roles.Jobs;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Utility;

namespace Content.Server._BRatbite.PermaBrig;

/// <summary>
/// This handles...
/// </summary>
public sealed class PermaBrigSystem : GameRuleSystem<PermaBrigComponent>
{
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly IRobustRandom _robustRandom = default!;
    [Dependency] private readonly GameTicker _ticker = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly PlayTimeTrackingSystem _playTimeTrackings = default!;
    [Dependency] private readonly StationSpawningSystem _stationSpawning = default!;
    [Dependency] private readonly AdminSystem _admin = default!;
    [Dependency] private readonly SharedJobSystem _jobs = default!;
    [Dependency] private readonly SharedRoleSystem _roles = default!;
    [Dependency] private readonly PermaBrigManager _permaBrigManager = default!;

    public List<ICommonSession> PermaIndividuals = new();

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RulePlayerSpawningEvent>(OnPlayerSpawning);
        SubscribeLocalEvent<PlayerBeforeSpawnEvent>(OnPlayerBeforeSpawning);
        SubscribeLocalEvent<RoundEndMessageEvent>(OnRoundEnd);
    }

    private void OnPlayerSpawning(RulePlayerSpawningEvent args)
    {
        var pool = args.PlayerPool;

        PermaIndividuals = new List<ICommonSession>();

        if (!_ticker.IsGameRuleActive<PermaBrigComponent>())
            return;

        foreach (var session in pool)
        {
            if (_permaBrigManager.GetBrigSentence(session.UserId) == 0)
                continue;
            PermaIndividuals.Add(session);
            Logger.Debug($"Player being sent to perma: {session}");
        }

        foreach (var player in PermaIndividuals)
        {
            pool.Remove(player);
            GameTicker.PlayerJoinGame(player);

            SpawnPrisonerPlayer(player);

            Logger.Debug($"Player sent to perma: {player}");
        }
    }

    private void OnPlayerBeforeSpawning(PlayerBeforeSpawnEvent ev)
    {
        if (!_ticker.IsGameRuleActive<PermaBrigComponent>())
            return;

        if (!ev.LateJoin) //OnPlayerSpawning handles the start round spawning, before traitor picking, so this just needs to handle late joiners.
            return;

        if (_permaBrigManager.GetBrigSentence(ev.Player.UserId) == 0)
            return;

        PermaIndividuals.Add(ev.Player);

        SpawnPrisonerPlayer(ev.Player);

        ev.Handled = true;

        Logger.Debug($"Player sent to perma: {ev.Player}");
    }

    private void SpawnPrisonerPlayer(ICommonSession player)
    {
        var stations = _ticker.GetSpawnableStations();
        _robustRandom.Shuffle(stations);
        var station = EntityUid.Invalid;
        if (stations.Count != 0)
            station = stations[0];

        var character = _ticker.GetPlayerProfile(player);

        var data = player.ContentData();

        var newMind = _mind.CreateMind(data!.UserId, character.Name);
        _mind.SetUserId(newMind, data.UserId);

        var jobPrototype = _prototypeManager.Index<JobPrototype>("Prisoner");

        _playTimeTrackings.PlayerRolesChanged(player);

        var mobMaybe = _stationSpawning.SpawnPlayerCharacterOnStation(station, "Prisoner", character);
        DebugTools.AssertNotNull(mobMaybe);
        var mob = mobMaybe!.Value;

        _mind.TransferTo(newMind, mob);
        _admin.UpdatePlayerList(player);

        _roles.MindAddJobRole(newMind, silent: false, jobPrototype:"Prisoner");
        var jobName = _jobs.MindTryGetJobName(newMind);
        _admin.UpdatePlayerList(player);
    }
}
