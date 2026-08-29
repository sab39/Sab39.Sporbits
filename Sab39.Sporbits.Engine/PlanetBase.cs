using System.Numerics;

using Sab39.Sabric.Engine.Aether;

using nkast.Aether.Physics2D.Collision.Shapes;
using nkast.Aether.Physics2D.Dynamics;

namespace Sab39.Sporbits.Engine;

/// <summary>
/// A round body with mass. Everything in Sporbits is one of these.
/// </summary>
public abstract class PlanetBase(SporbitsGame game, Vector2 initialPosition = default, float initialRadius = 1, float initialDensity = 1)
    : AetherGameObjectBase(game, initialPosition)
{
    public override SporbitsGame Game => (SporbitsGame)base.Game;

    public CircleShape Circle { get; } = new(initialRadius, initialDensity);

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
