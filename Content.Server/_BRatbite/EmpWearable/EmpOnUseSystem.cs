using Content.Server.Damage.Systems;
using Content.Server.Emp;
using Content.Shared._BRatbite.EmpGlove;
using Content.Shared.Interaction;
using Robust.Server.Containers;
using Robust.Server.GameObjects;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;

namespace Content.Server._BRatbite.EmpWearable;

/// <summary>
/// This handles items that apply an EMP effect when used on a target.
/// </summary>
public sealed class EmpOnUseSystem : EntitySystem
{
    [Dependency] private readonly EmpSystem _empSystem = default!;
    [Dependency] private readonly ContainerSystem _container = default!;
    [Dependency] private readonly StaminaSystem _stamina = default!;
    [Dependency] private readonly SharedAudioSystem _audioSystem = default!;
    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<EmpOnUseComponent, AfterInteractEvent>(OnAfterInteract);
    }

    private void OnAfterInteract(Entity<EmpOnUseComponent> ent, ref AfterInteractEvent args)
    {
        if (!args.CanReach
            || args.Target is not { } target
            || args.Target == args.User)
            return;

        if (TryComp<ContainerManagerComponent>(target, out var containerManager))
        {
            var containers = _container.GetAllContainers(target, containerManager);
            foreach (var container in containers)
                foreach (var entity in container.ContainedEntities)
                    _empSystem.TryEmpEffects(entity, ent.Comp.EmpDrain, ent.Comp.EmpDuration);
        }

        _empSystem.TryEmpEffects(target, ent.Comp.EmpDrain, ent.Comp.EmpDuration);

        Spawn(EmpSystem.EmpPulseEffectPrototype, Transform(target).Coordinates);
        _audioSystem.PlayPvs(ent.Comp.EmpSound, ent);

        args.Handled = true;
    }
}
