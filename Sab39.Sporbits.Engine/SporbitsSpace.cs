using System.Numerics;

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
        World.Add(Gravity);

        Add(Player);
        Add(Puck);

        World.Add(PlayerInput);
    }

    /// <summary>
    /// True once the puck has crashed into the player's planet.
    /// </summary>
    public bool IsOver { get; private set; }

    /// <remarks>
    /// Slack on the crash test, so contact registers on the frame it happens rather than a frame
    /// or two later.
    ///
    /// Testing by distance at all is a placeholder for real collision events, which are wanted and
    /// wait on deciding how a collision reaches game code through GameObjectBase without the
    /// abstract layer learning that Aether exists. It survives in the meantime only because nothing
    /// here is bouncy: two planets that touch stay touching, so there is no way to be in contact
    /// between one tick and the next without still being in contact at the next tick.
    /// </remarks>
    private const float CrashTolerance = 0.1f;

    protected override void OnAdvance(long delta)
    {
        base.OnAdvance(delta);

        // After the base advance, not before: SyncFromWorld is what makes these positions this
        // tick's rather than last tick's.
        var crashDistance = Player.Radius + Puck.Radius + CrashTolerance;
        if (Vector2.Distance(Player.Position, Puck.Position) <= crashDistance) IsOver = true;
    }

    protected override void OnAdd(AetherObjectBase obj) => Gravity.AddBody(obj.Body);

    // Aether's GravityController has no RemoveBody, only the list AddBody appends to.
    protected override void OnRemove(AetherObjectBase obj) => Gravity.Bodies.Remove(obj.Body);
}
