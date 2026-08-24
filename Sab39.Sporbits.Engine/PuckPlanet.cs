using System.Numerics;

using Sab39.Sabric.Engine;

namespace Sab39.Sporbits.Engine;

/// <summary>
/// The ball. Smaller than the player, and moved only by gravity and collisions.
/// </summary>
public sealed class PuckPlanet(SporbitsGame game, Vector2 initialPosition, float initialRadius = 0.5f)
    : PlanetBase(game, initialPosition, initialRadius)
{
    public override TResult Accept<TResult>(IGameObjectVisitor<TResult> visitor) => visitor.Visit(this);
}
