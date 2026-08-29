using System.Numerics;

namespace Sab39.Sporbits.Engine;

/// <summary>
/// The planet the player steers.
/// </summary>
/// <remarks>
/// Partial because the Accept override is generated. Nothing else is expected in the other part.
/// </remarks>
public sealed partial class PlayerPlanet(SporbitsGame game, Vector2 initialPosition) : PlanetBase(game, initialPosition);
