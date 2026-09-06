namespace Sab39.Sporbits.Engine;

/// <summary>
/// A piece of rubble in a stream: an obstacle that arrives, crosses, and is gone.
/// </summary>
/// <remarks>
/// Its own type rather than an <see cref="ObstaclePlanet"/> with a flag set, because what makes an
/// asteroid one is that it is one of very many rather than anything about its size or place - and
/// being one of many is what everything else about it is going to follow from.
///
/// Partial because the Accept override is generated.
/// </remarks>
public sealed partial class Asteroid : ObstaclePlanet
{
    /// <remarks>
    /// The one kind of planet that is pulled without pulling. Rubble massing a fraction of anything
    /// else in the space moves nothing, so what its pull buys is asteroids perturbing each other -
    /// and what it costs is the whole of the n-squared, because rubble is the only thing there is
    /// ever a lot of. Out of the source list, each one answers to the sun and the planets and nothing
    /// else, which is what it looked like it was doing anyway.
    /// </remarks>
    public override bool IsGravitySource => false;
}
