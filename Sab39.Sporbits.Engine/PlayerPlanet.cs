using System.Numerics;

namespace Sab39.Sporbits.Engine;

/// <summary>
/// The planet the player steers.
/// </summary>
public sealed class PlayerPlanet(SporbitsGame game, Vector2 initialPosition) : PlanetBase(game, initialPosition)
{
}
