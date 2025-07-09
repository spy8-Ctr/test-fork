using Content.Shared.Interaction.Events;
using Content.Shared.Popups;
using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Weapons.Ranged.Events;
using Content.Shared.Weapons.Ranged.Systems;
using Content.Shared.Whitelist;

namespace Content.Shared._BRatbite.LimitWeapon;

/// <summary>
/// This handles...
/// </summary>
public sealed class LimitWeaponSystem : EntitySystem
{
    [Dependency] private EntityWhitelistSystem _entityWhitelistSystem = default!;
    [Dependency] private SharedPopupSystem _popupSystem = default!;
    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<ShotAttemptedEvent>(OnAttemptShoot);
        SubscribeLocalEvent<LimitWeaponComponent, AttackAttemptEvent>(OnAttemptAttack);
    }

    private void OnAttemptAttack(Entity<LimitWeaponComponent> ent, ref AttackAttemptEvent args)
    {
        if (ent.Comp.Whitelist == null)
        {
            args.Cancel();
            _popupSystem.PopupClient(ent.Comp.MeleeFail, ent, ent);
            return;
        }

        if (!_entityWhitelistSystem.IsValid(ent.Comp.Whitelist, args.Weapon))
        {
            args.Cancel();
            _popupSystem.PopupClient(ent.Comp.MeleeFail, ent, ent);
            return;
        }
    }

    private void OnAttemptShoot(ref ShotAttemptedEvent args)
    {
        if (!TryComp<LimitWeaponComponent>(args.User, out var comp))
            return;

        if (comp.Whitelist == null)
        {
            args.Cancel();
            _popupSystem.PopupClient(comp.GunFail, args.User, args.User);
            return;
        }

        if (!_entityWhitelistSystem.IsValid(comp.Whitelist, args.Used))
        {
            args.Cancel();
            _popupSystem.PopupClient(comp.GunFail, args.User, args.User);
            return;
        }
    }
}
