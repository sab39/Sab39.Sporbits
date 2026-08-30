namespace Sab39.Sporbits.Engine;

/// <summary>
/// The ball. Smaller than the player, and moved only by gravity and collisions.
/// </summary>
/// <remarks>
/// Partial because the Accept override is generated. Nothing else is expected in the other part.
/// </remarks>
public sealed partial class PuckPlanet : PlanetBase
{
    // Radius is inherited, so a smaller default can only be applied here rather than at the
    // declaration - and it belongs to the type rather than to every call site.
    public PuckPlanet() => Radius = 0.5f;
}
