using nkast.Aether.Physics2D.Dynamics;

namespace Sab39.Sporbits.Engine;

/// <summary>
/// A star: heavy, bright, and exactly where it was put.
/// </summary>
/// <remarks>
/// Static, which costs it nothing. Aether's GravityController takes a static body's mass into
/// account like any other, so a sun that cannot be moved still pulls on everything around it - and
/// a system built on one that can be moved drifts, because the player thrusting adds momentum to it.
///
/// Partial because the Accept override is generated. Nothing else is expected in the other part.
/// </remarks>
public sealed partial class Sun : PlanetBase
{
    public Sun()
    {
        BodyType = BodyType.Static;
        Radius = 5;
        Density = 4;
    }
}
