namespace Sab39.Sporbits.Engine;

/// <summary>
/// A planet that is only in the way: rubble in a stream, or something that was already in orbit
/// when you got there.
/// </summary>
/// <remarks>
/// One type for both rather than an Asteroid as well, because nothing tells them apart - a size and
/// a place is the whole of the difference. This is the field of grey planets PlainPlanetView was
/// kept for.
///
/// Partial because the Accept override is generated. Nothing else is expected in the other part.
/// </remarks>
public sealed partial class ObstaclePlanet : PlanetBase;
