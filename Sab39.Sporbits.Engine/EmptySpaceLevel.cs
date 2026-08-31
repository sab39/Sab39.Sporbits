namespace Sab39.Sporbits.Engine;

/// <summary>
/// Nothing out there but the player, the puck, and somewhere to put it.
/// </summary>
/// <remarks>
/// The pair start in a settled, noticeably elliptical orbit around each other, and the puck has to
/// be prised out of it and walked all the way to the goal. That is the whole level: there is nothing
/// else out there to help or to get in the way.
/// </remarks>
public sealed class EmptySpaceLevel : ISporbitsLevel
{
    public string Name => "Empty space";

    public void Populate(SporbitsSpace space)
    {
        space.Puck.Position = new(10, 0);

        // Measured rather than derived: 1.5 against a circular speed of 1.77 swings between 10 and
        // 5.6 units apart, which is elliptical enough to read as one and never near touching. Held
        // for ten minutes of ticks. Arithmetic is what got this wrong before - see AetherGravityEffect.
        space.Puck.Velocity = new(0, -1.5f);

        // Behind the player and square across the puck's path, so that no amount of waiting helps.
        space.Goal = new() { Position = new(-45, 0) };

        space.Add(space.Puck);
        space.Add(space.Goal);
    }
}
