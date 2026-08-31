namespace Sab39.Sporbits.Engine;

/// <summary>
/// A star with everything else going round it, the player included.
/// </summary>
/// <remarks>
/// The goal sits outside every orbit in the system, so winning means lifting the puck out of its
/// own - and the sun is the one thing here that will not move out of the way.
/// </remarks>
public sealed class SolarSystemLevel : ISporbitsLevel
{
    public string Name => "Solar system";

    public void Populate(SporbitsSpace space)
    {
        Sun sun = new();
        space.Add(sun);

        space.Orbit(space.Player, sun, 30);
        space.Orbit(space.Puck, space.Player, 2.7f, float.Pi / 3);

        ObstaclePlanet inner = new() { Radius = 1.5f };
        space.Orbit(inner, sun, 15, float.Pi / 2);
        space.Add(inner);

        ObstaclePlanet outer = new() { Radius = 2 };
        space.Orbit(outer, sun, 38, -float.Pi / 3);
        space.Add(outer);

        space.Goal = new() { Position = new(0, 52) };

        space.Add(space.Puck);
        space.Add(space.Goal);
    }
}
