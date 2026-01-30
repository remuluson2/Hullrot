using Content.Shared._Crescent.Overlays;
using Content.Shared._Crescent.SpaceBiomes;
using Robust.Shared.GameStates;
using Robust.Shared.Network;

namespace Content.Shared._Crescent.HadalCorrosion;

/// <summary>
///     get the FUCK out of the hadal like what are you doing
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class HadalCorrosionComponent : Component
{
    [DataField, AutoNetworkedField]
    public float CorrosionLevel = 0.001f;
}

/// <summary>
///     okay maybe you can have a little fog. as a treat
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class CorrosionResistanceComponent : Component
{
    [DataField, AutoNetworkedField]
    public float ResistanceMultiplier = 0.5f;
}

public sealed class HadalCorrosionSystem : EntitySystem
{
    [Dependency] private readonly INetManager _net = default!;
    public override void Initialize()
    {
        base.Initialize();
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (!_net.IsServer)
            return;

        var query = EntityManager.EntityQueryEnumerator<HadalCorrosionComponent, SpaceBiomeTrackerComponent>();
        while (query.MoveNext(out var ent, out var hadalCorrosion, out var biomeTracker))
        {
            var inHadal = biomeTracker.Biome == "default";

            if (!TryComp<CorrosionResistanceComponent>(ent, out var resistance))
                resistance = new CorrosionResistanceComponent { ResistanceMultiplier = 1f };

            var corrosionAmount = resistance.ResistanceMultiplier * 0.0005f + hadalCorrosion.CorrosionLevel;
            if (!inHadal)
                corrosionAmount = corrosionAmount * -5;

            hadalCorrosion.CorrosionLevel = Math.Min(1f, Math.Max(0f, corrosionAmount));

            var staticomp = EnsureComp<StaticOverlayComponent>(ent);
            staticomp.AdditionLevel = hadalCorrosion.CorrosionLevel;

            Dirty(ent, hadalCorrosion);
            Dirty(ent, staticomp);
        }
    }
}
