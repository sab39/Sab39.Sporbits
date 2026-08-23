using System.Numerics;

namespace Sab39.Sporbits.Engine;

/// <summary>
/// The ball. Smaller than the player, and moved only by gravity and collisions.
/// </summary>
public sealed class PuckPlanet(SporbitsGame game, Vector2 initialPosition, float initialRadius = 0.5f)
    : PlanetBase(game, initialPosition, initialRadius)
{
}
