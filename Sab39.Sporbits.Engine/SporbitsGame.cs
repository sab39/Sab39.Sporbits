using System.Numerics;

using Sab39.Sabric.Engine.Aether;

using nkast.Aether.Physics2D.Controllers;
using Sab39.Sabric.Engine;

namespace Sab39.Sporbits.Engine;

/// <summary>
/// A game of Sporbits.
/// </summary>
public sealed class SporbitsGame : AetherGameBase
{
    public PlayerPlanet Player => field ??= new(this, default);
    public PuckPlanet Puck => field ??= new(this, new(10, 0)) { Velocity = new(0, -4) };

    public GravityController Gravity => field ??= new(8);
    public PlayerInputController PlayerInput => field ??= new(Player, 16);

    /// <summary>
    /// True once the puck has crashed into the player's planet. A game that is over stops
    /// advancing, whatever keeps calling <see cref="GameBase.Tick"/>.
    /// </summary>
    public bool IsOver { get; private set; }

    protected override void OnInit()
    {
        World.Add(Gravity);

        AddGameObject(Player);
        AddGameObject(Puck);

        World.Add(PlayerInput);
    }

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

    protected override void OnTick(long tickStamp)
    {
        if (IsOver) return;

        base.OnTick(tickStamp);

        // After the base tick, not before: SyncFromWorld is what makes these positions this
        // tick's rather than last tick's.
        var crashDistance = Player.Radius + Puck.Radius + CrashTolerance;
        if (Vector2.Distance(Player.Position, Puck.Position) <= crashDistance) IsOver = true;
    }

    protected override void OnAddGameObject(AetherGameObjectBase obj) => Gravity.AddBody(obj.Body);

    // Aether's GravityController has no RemoveBody, only the list AddBody appends to.
    protected override void OnRemoveGameObject(AetherGameObjectBase obj)
    {
        Gravity.Bodies.Remove(obj.Body);
        base.OnRemoveGameObject(obj);
    }
}
