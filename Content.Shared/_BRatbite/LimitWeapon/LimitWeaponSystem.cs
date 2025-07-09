using Content.Shared.Interaction.Events;
using Content.Shared.Popups;
using Content.Shared.Weapons.Melee.Events;
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
        SubscribeLocalEvent<LimitWeaponComponent, ShotAttemptedEvent>(OnAttemptShoot);
        SubscribeLocalEvent<LimitWeaponComponent, AttemptMeleeEvent>(OnAttemptAttack);
    }

    private void OnAttemptAttack(Entity<LimitWeaponComponent> ent, ref AttemptMeleeEvent args)
    {
        if (args.Weapon == ent.Owner)
            return;

        if (ent.Comp.Whitelist == null)
        {
            args.Cancel();
            _popupSystem.PopupClient(ent.Comp.MeleeFail, ent, ent);
            args.Cancelled = true;
            return;
        }

        if (!_entityWhitelistSystem.IsValid(ent.Comp.Whitelist, args.Weapon))
        {
            args.Cancel();
            _popupSystem.PopupClient(ent.Comp.MeleeFail, ent, ent);
            args.Cancelled = true;
            return;
        }
    }

    private void OnAttemptShoot(Entity<LimitWeaponComponent> ent, ref ShotAttemptedEvent args)
    {
        if (args.Used.Owner == ent.Owner)
            return;

        if (ent.Comp.Whitelist == null)
        {
            args.Cancel();
            _popupSystem.PopupClient(comp.GunFail, args.User, args.User);
            return;
        }

        if (!_entityWhitelistSystem.IsValid(ent.Comp.Whitelist, args.Used))
        {
            args.Cancel();
            _popupSystem.PopupClient(comp.GunFail, args.User, args.User);
            return;
        }
    }
}
