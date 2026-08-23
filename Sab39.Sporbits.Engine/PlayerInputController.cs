using Sab39.Sabric.Engine;
using Sab39.Sabric.Engine.Aether;

namespace Sab39.Sporbits.Engine;

/// <summary>
/// Pushes the player's planet in whatever direction the player is asking for.
/// </summary>
/// <remarks>
/// Living in Sporbits.Engine is provisional. This is the translation from player intent into a
/// force on the world, which is arguably an input concern rather than an engine one - it may
/// well be the thing that belongs in Sporbits.UI, which is otherwise an empty project nobody
/// has found a use for. See the open questions in Docs/WIP/sporbits-revival.md.
/// </remarks>
public sealed class PlayerInputController(PlayerPlanet planet, float strength) : AetherInputControllerBase
{
    public PlayerPlanet Planet { get; } = planet;
    public float Strength { get; } = strength;

    public override void Update(float dt) => Planet.Body.ApplyForce((MovementDirection * Strength).AsAether());
}
