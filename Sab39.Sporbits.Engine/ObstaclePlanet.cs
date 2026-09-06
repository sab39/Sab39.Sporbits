namespace Sab39.Sporbits.Engine;

/// <summary>
/// A planet that is only in the way: rubble in a stream, or something that was already in orbit
/// when you got there.
/// </summary>
/// <remarks>
/// Unsealed for <see cref="Asteroid"/>, which is the one thing that has turned out to tell them
/// apart. Anything that is only in the way and has nothing else to say is still one of these. This is
/// the field of grey planets PlainPlanetView was kept for.
///
/// Partial because the Accept override is generated. Nothing else is expected in the other part.
/// </remarks>
public partial class ObstaclePlanet : PlanetBase;
