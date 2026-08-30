using nkast.Aether.Physics2D.Collision.Shapes;
using nkast.Aether.Physics2D.Dynamics;

namespace Sab39.Sporbits.Engine;

/// <summary>
/// A round body with mass. Almost everything in Sporbits is one of these.
/// </summary>
public abstract class PlanetBase : SporbitsObjectBase
{
    /// <remarks>
    /// The shape exists from construction rather than being built on attach, because Radius and
    /// Density read and write straight through to it - so a detached planet still has a size.
    /// </remarks>
    public CircleShape Circle { get; } = new(1, 1);

    public Fixture Fixture => field ??= Body.FixtureList[0];

    public float Radius { get => Circle.Radius; set => Circle.Radius = value; }
    public float Density { get => Circle.Density; set => Circle.Density = value; }

    protected override void InitializeBody()
    {
        // Nothing to drag against in space.
        Body.AngularDamping = 0;
        Body.LinearDamping = 0;

        Body.CreateFixture(Circle);
    }
}
