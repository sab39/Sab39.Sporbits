namespace Sab39.Sporbits.Engine;

/// <summary>
/// The game itself. Currently only tick accounting - physics, the world and game objects
/// arrive once the Sabric.Engine seam is designed.
/// </summary>
public sealed class SporbitsGame
{
    public int Ticks { get; private set; }
    public long FirstTickStamp { get; private set; }
    public long LastTickStamp { get; private set; }
    public long Delta { get; private set; }
    public long TotalMillis => LastTickStamp - FirstTickStamp;

    public void Init() => Ticks = 0;

    public void Tick(long tickStamp)
    {
        if (Ticks == 0)
        {
            FirstTickStamp = tickStamp;
            LastTickStamp = tickStamp;
        }

        Ticks++;
        Delta = tickStamp - LastTickStamp;
        LastTickStamp = tickStamp;

        // TODO: step the physics world and update game objects.
    }
}
