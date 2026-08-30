using nkast.Aether.Physics2D.Collision.Shapes;
using nkast.Aether.Physics2D.Dynamics;

namespace Sab39.Sporbits.Engine;

/// <summary>
/// Where the puck has to end up. The puck reaching it wins the game; the player touching it loses.
/// </summary>
/// <remarks>
/// The first thing in the game that isn't a planet, and what SporbitsObjectBase was put there for:
/// it claims no gravity, weighs nothing worth speaking of, and nothing bounces off it.
///
/// Static and a sensor. Static because nothing should be able to shove the goal aside, and a sensor
/// because touching it is a rule being satisfied rather than an impact - a solid goal would bounce
/// the puck off the thing it is supposed to enter.
///
/// Partial because the Accept override is generated. Nothing else is expected in the other part.
/// </remarks>
public sealed partial class Goal : SporbitsObjectBase
{
    public Goal() => BodyType = BodyType.Static;

    /// <remarks>
    /// Its own shape rather than PlanetBase's, for the same reason it isn't a planet: being round is
    /// all the two have in common, and inheriting would bring the gravity registration with it.
    /// </remarks>
    public CircleShape Circle { get; } = new(4, 0);

    public float Radius { get => Circle.Radius; set => Circle.Radius = value; }

    protected override void InitializeBody() => Body.CreateFixture(Circle).IsSensor = true;
}
