namespace Sab39.Sporbits.Engine;

/// <summary>
/// Nothing out there but the player, the puck, and somewhere to put it.
/// </summary>
/// <remarks>
/// The puck starts well above escape speed for the player's own gravity and pointed nowhere near
/// the goal, so it leaves on its own and has to be chased down and turned around. That is the whole
/// level: there is nothing else out there to help or to get in the way.
/// </remarks>
public sealed class EmptySpaceLevel : ISporbitsLevel
{
    public string Name => "Empty space";

    public void Populate(SporbitsSpace space)
    {
        space.Puck.Position = new(10, 0);
        space.Puck.Velocity = new(0, -4);

        // Behind the player and square across the puck's path, so that no amount of waiting helps.
        space.Goal = new() { Position = new(-45, 0) };

        space.Add(space.Puck);
        space.Add(space.Goal);
    }
}
