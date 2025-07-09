using Content.Shared.Whitelist;
using Robust.Shared.GameStates;

namespace Content.Shared._BRatbite.LimitWeapon;

/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class LimitWeaponComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite), DataField, AutoNetworkedField]
    public EntityWhitelist? Whitelist;
}
