namespace Sab39.Sporbits.Engine;

/// <summary>
/// A goal a long way to the left, and a steady stream of rubble coming the other way.
/// </summary>
/// <remarks>
/// The puck starts in a close, slow orbit so that it comes along rather than having to be fetched.
/// Carrying it is meant to be the easy part; the asteroids are the level.
/// </remarks>
public sealed class AsteroidStreamLevel : ISporbitsLevel
{
    public string Name => "Asteroid stream";

    public void Populate(SporbitsSpace space)
    {
        space.Orbit(space.Puck, space.Player, 8);

        // Far enough left that the stream has plenty of chances at you on the way.
        space.Goal = new() { Position = new(-90, 0) };

        space.Add(space.Puck);
        space.Add(space.Goal);

        space.AddRule(new AsteroidStreamRule
        {
            IntervalMillis = 900,
            Speed = 25,
            Radius = 0.6f,
        });
    }
}
