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

    /// <summary>
    /// What this planet weighs from its size and density alone, body or no body.
    /// </summary>
    /// <remarks>
    /// Mass itself is only meaningful once the planet is attached, and a level has to work orbits
    /// out before anything is attached - so the computation is available up front, and
    /// <see cref="InitializeBody"/> is what commits it.
    /// </remarks>
    public float ComputedMass => float.Pi * Radius * Radius * Density;

    protected override void InitializeBody()
    {
        // Nothing to drag against in space.
        Body.AngularDamping = 0;
        Body.LinearDamping = 0;

        // And nothing stops of its own accord either. Aether sleeps a body whose velocity stays
        // under its tolerance for half a second, which is an ordinary state for a planet in a wide,
        // slow orbit - and a sleeping body ignores gravity. Measured: a puck placed by Orbit at a
        // separation of 20 held a circle with sleeping off, and with it on the player dropped off
        // and the orbit opened out by exactly the (M+m)/M its speed had been computed for.
        Body.SleepingAllowed = false;

        Body.CreateFixture(Circle);

        // After the fixture, not before: CreateFixture resets the body's mass from what its shapes
        // come to, so a value set first would be silently thrown away.
        Mass = ComputedMass;
    }

    /// <remarks>
    /// A planet puts itself into the space's gravity, rather than the space doing it to everything
    /// it holds. Mass is a planet's own claim about itself, and a space is meant to be able to hold
    /// things that have no such claim - a goal, a boundary, a marker.
    ///
    /// The two halves run either side of base on purpose, and the asymmetry is the point: attaching
    /// is what creates the body and detaching is what destroys it, so everything gravity is holding
    /// has a body for as long as it is holding it. Reversed, either half would leave a window where
    /// something registered has none to read.
    /// </remarks>
    protected override void OnAttached()
    {
        base.OnAttached();
        Space.Gravity.Add(this);
    }

    protected override void OnDetached()
    {
        Space.Gravity.Remove(this);
        base.OnDetached();
    }
}
