using Robust.Shared.GameStates;

namespace Content.Shared._BRatbite.FiringPin;

/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class FiringPinComponent : Component
{
    [DataField, AutoNetworkedField]
    public bool Locked = true;
}
