using Sab39.Sabric.Core;

namespace Sab39.Sporbits.Engine;

/// <summary>
/// Keeps a steady stream of asteroids crossing the player from the right.
/// </summary>
/// <remarks>
/// Anchored to the player rather than to a fixed corridor, so the stream is still coming wherever
/// the player has got to. What that costs is that outrunning it isn't a thing you can do.
///
/// It keeps its own list of what it spawned rather than sweeping the space's. That list is what it
/// is allowed to remove from, and walking a collection while removing from it would need a copy
/// every tick.
/// </remarks>
public sealed class AsteroidStreamRule : ISporbitsRule
{
    /// <summary>
    /// How far ahead of the player an asteroid appears, and how far behind it is dropped again.
    /// </summary>
    public float Lead { get; init; } = 110;

    /// <summary>
    /// How far either side of the player one can appear.
    /// </summary>
    public float Spread { get; init; } = 60;

    public long IntervalMillis { get; init; } = 900;

    public float Speed { get; init; } = 25;

    public float Radius { get; init; } = 0.6f;

    /// <remarks>
    /// Unseeded. There is nothing here worth reproducing exactly, and a fixed seed would make every
    /// run of the level identical.
    /// </remarks>
    private readonly Random random = new();

    private readonly List<ObstaclePlanet> spawned = [];

    private long sinceLast;

    public void Update(long delta, SporbitsSpace space)
    {
        this.sinceLast += delta;

        if (this.sinceLast >= IntervalMillis)
        {
            this.sinceLast = 0;
            Spawn(space);
        }

        Sweep(space);
    }

    /// <remarks>
    /// A little vertical drift as well as the crossing speed, so that the stream is a hazard to fly
    /// through rather than a set of rails with gaps in it.
    /// </remarks>
    private void Spawn(SporbitsSpace space)
    {
        var (x, y) = space.Player.Position;

        ObstaclePlanet asteroid = new()
        {
            Radius = Radius,
            Position = new(x + Lead, y + Scatter(Spread)),
            Velocity = new(-Speed, Scatter(3)),
        };

        space.Add(asteroid);
        this.spawned.Add(asteroid);
    }

    /// <remarks>
    /// Backwards, so removing one doesn't move the next out from under the index. Without this the
    /// asteroid count - and the cost of the gravity between all of them - grows for as long as the
    /// game lasts.
    /// </remarks>
    private void Sweep(SporbitsSpace space)
    {
        for (var i = this.spawned.Count - 1; i >= 0; i--)
        {
            var asteroid = this.spawned[i];
            if (asteroid.Position.X > space.Player.Position.X - Lead) continue;

            space.Remove(asteroid);
            this.spawned.RemoveAt(i);
        }
    }

    /// <summary>
    /// A random offset somewhere between plus and minus <paramref name="extent"/>.
    /// </summary>
    private float Scatter(float extent) => ((this.random.NextSingle() * 2) - 1) * extent;
}
