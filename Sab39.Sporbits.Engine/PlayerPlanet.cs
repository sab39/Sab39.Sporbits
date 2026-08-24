using System.Numerics;

using Sab39.Sabric.Engine;

namespace Sab39.Sporbits.Engine;

/// <summary>
/// The planet the player steers.
/// </summary>
public sealed class PlayerPlanet(SporbitsGame game, Vector2 initialPosition) : PlanetBase(game, initialPosition)
{
    public override TResult Accept<TResult>(IGameObjectVisitor<TResult> visitor) => visitor.Visit(this);
}
