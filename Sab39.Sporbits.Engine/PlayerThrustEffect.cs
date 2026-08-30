using Sab39.Sabric.Engine;
using Sab39.Sabric.Engine.Aether;

namespace Sab39.Sporbits.Engine;

/// <summary>
/// Pushes the player's planet in whatever direction the player is asking for.
/// </summary>
/// <remarks>
/// Living in Sporbits.Engine is provisional. This is the translation from player intent into a
/// force on the world, which is arguably an input concern rather than an engine one - it may well be
/// the thing that belongs in Sporbits.UI, which is otherwise an empty project nobody has found a use
/// for. See the open questions in Docs/architecture.md in the Sabric repo.
///
/// A force rather than an acceleration, which is what makes it an Aether effect rather than a plain
/// one: the push is the same whatever the planet weighs, so a heavier planet answers it less.
/// </remarks>
public sealed class PlayerThrustEffect(PlayerPlanet planet, PlayerInput input, float strength)
    : AetherEffectBase
{
    public PlayerPlanet Planet { get; } = planet;

    /// <remarks>
    /// Handed the input rather than being a kind of input, so that the space owns it and this is
    /// just one of the things reading it.
    /// </remarks>
    public PlayerInput Input { get; } = input;

    public float Strength { get; } = strength;

    protected override void Update(long delta, IAetherEffectContext context)
        => context.ApplyForce(Planet, Input.MovementDirection * Strength);
}
