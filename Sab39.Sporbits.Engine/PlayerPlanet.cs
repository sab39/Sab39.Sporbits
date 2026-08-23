using Sab39.Sabric.Engine.Aether;

using nkast.Aether.Physics2D.Common;

namespace Sab39.Sporbits.Engine;

/// <summary>
/// The planet the player steers.
/// </summary>
public sealed class PlayerPlanet(GameBase game, Vector2 initialPosition) : PlanetBase(game, initialPosition)
{
}
