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

    /// <remarks>
    /// A planet puts itself into the space's gravity, rather than the space doing it to everything
    /// it holds. Mass is a planet's own claim about itself, and a space is meant to be able to hold
    /// things that have no such claim - a goal, a boundary, a marker.
    ///
    /// The two halves run either side of base on purpose, and the asymmetry is the point: attaching
    /// is what creates the body and detaching is what destroys it, so registering has to follow the
    /// first and unregistering has to precede the second. Reversed, either half reads Body when
    /// there isn't one.
    /// </remarks>
    protected override void OnAttached()
    {
        base.OnAttached();
        Space.Gravity.AddBody(Body);
    }

    // Aether's GravityController has no RemoveBody, only the list AddBody appends to.
    protected override void OnDetached()
    {
        Space.Gravity.Bodies.Remove(Body);
        base.OnDetached();
    }
}
