namespace Sab39.Sporbits.Engine;

/// <summary>
/// Something a space checks every tick, on settled state, once the physics has stepped.
/// </summary>
/// <remarks>
/// The mirror of an effect on the far side of the step, and Sporbits' own rather than Sabric's.
/// Whether a rule is a category the framework wants, or whether an effect simply absorbs it, is an
/// open question in the Sabric repo's Docs/architecture.md; one that exists is better evidence for
/// answering it than another argument would be.
///
/// What forces the distinction is spawning. An effect runs inside World.Step, where Aether's world
/// is locked and adding a body is the thing that same doc says should not stay allowed. A rule runs
/// after the step, where the world is settled and adding one is ordinary.
/// </remarks>
public interface ISporbitsRule
{
    void Update(long delta, SporbitsSpace space);
}
