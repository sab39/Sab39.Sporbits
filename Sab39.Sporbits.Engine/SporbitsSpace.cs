using Sab39.Sabric.Engine;
using Sab39.Sabric.Engine.Aether;

using nkast.Aether.Physics2D.Controllers;

namespace Sab39.Sporbits.Engine;

/// <summary>
/// The space a game of Sporbits is played in: the planets, and the gravity between them.
/// </summary>
public sealed class SporbitsSpace(GameSessionBase session) : AetherSpace(session)
{
    public PlayerPlanet Player => field ??= new();
    public PuckPlanet Puck => field ??= new() { Position = new(10, 0), Velocity = new(0, -4) };

    public GravityController Gravity => field ??= new(8);
    public PlayerInputController PlayerInput => field ??= new(Player, 16);

    /// <summary>
    /// Fills the space with what a game of Sporbits starts with.
    /// </summary>
    /// <remarks>
    /// Standing in for a level. This is the one seam where "what is in this space" is decided, and
    /// it is where the level machinery attaches once there is any - see the open questions in the
    /// Sabric repo's Docs/architecture.md.
    /// </remarks>
    public void Populate()
    {
        // Controllers first, so that nothing attaching can find the gravity it registers with
        // missing. Nothing today would - the property builds it on demand - but the order that
        // needs no such argument is free.
        AddController(Gravity);
        AddController(PlayerInput);

        Add(Player);
        Add(Puck);
    }

    /// <summary>
    /// True once the puck has crashed into the player's planet.
    /// </summary>
    public bool IsOver { get; private set; }

    /// <remarks>
    /// Between the two of them rather than "something hit the player", because a goal and an
    /// obstacle will both want their own answer to being hit and neither of them is this one.
    /// </remarks>
    protected override void OnCollision(CollisionInfo collision)
    {
        if (collision.Involves(Player, Puck)) IsOver = true;
    }
}
