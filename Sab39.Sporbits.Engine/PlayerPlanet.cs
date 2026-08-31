namespace Sab39.Sporbits.Engine;

/// <summary>
/// The planet the player steers.
/// </summary>
/// <remarks>
/// Partial because the Accept override is generated. Nothing else is expected in the other part.
/// </remarks>
public sealed partial class PlayerPlanet : PlanetBase
{
    // Catching the puck is the whole point of this one, and a surface that pushes it away would make
    // that impossible.
    public override bool RepelsPuck => false;
}
