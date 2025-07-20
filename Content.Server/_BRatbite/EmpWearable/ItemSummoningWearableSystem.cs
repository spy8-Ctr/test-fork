using Content.Server.Emp;
using Content.Shared._BRatbite.EmpGlove;
using Content.Shared.Actions;
using Content.Shared.Hands.EntitySystems;
using Robust.Server.Containers;
using Robust.Shared.Containers;

namespace Content.Server._BRatbite.EmpWearable;

/// <summary>
/// This handles spawning in items for wearables.
/// Functions very similarly to mansus grasp, but can take any entity.
/// </summary>
public sealed class ItemSummoningWearableSystem : EntitySystem
{
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly ActionContainerSystem _actionContainer = default!;
    [Dependency] private readonly SharedActionsSystem _actionsSystem = default!;
    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ItemSummoningWearableComponent, ItemSummonActionEvent>(OnSummonEmp);
        SubscribeLocalEvent<ItemSummoningWearableComponent, GetItemActionsEvent>(OnGetActions);
        SubscribeLocalEvent<ItemSummoningWearableComponent, MapInitEvent>(OnMapInit);
    }

    private void OnMapInit(EntityUid uid, ItemSummoningWearableComponent component, MapInitEvent args)
    {
        _actionContainer.EnsureAction(uid, ref component.ActionEntity, component.Action);
        Dirty(uid, component);
    }

    private void OnGetActions(EntityUid uid, ItemSummoningWearableComponent component, GetItemActionsEvent args)
    {
        if (!args.SlotFlags.HasValue)
            return;

        if (!component.ValidSlots.HasFlag(args.SlotFlags))
            return;

        args.AddAction(component.ActionEntity);
    }

    private void OnSummonEmp(Entity<ItemSummoningWearableComponent> ent, ref ItemSummonActionEvent args)
    {
        if (ent.Comp.SummonedEntity.HasValue)
        {
            QueueDel(ent.Comp.SummonedEntity);
            ent.Comp.SummonedEntity = null;
            _actionsSystem.SetToggled(args.Action, false);
            return;
        }

        if (!_hands.TryGetEmptyHand(args.Performer, out var emptyHand))
            return;

        var summonedItem = Spawn(ent.Comp.SummonEntity, Transform(ent).Coordinates);

        if (!_hands.TryPickup(args.Performer, summonedItem, emptyHand, animate: false))
        {
            QueueDel(ent.Comp.SummonedEntity);
            return;
        }

        if (!TryComp<SummonedItemDespawnComponent>(summonedItem, out var summonedItemComp))
            return;

        ent.Comp.SummonedEntity = summonedItem;
        summonedItemComp.Creator = ent;

        _actionsSystem.SetToggled(args.Action, true);
    }
}
