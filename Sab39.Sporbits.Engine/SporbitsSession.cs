using Sab39.Sabric.Engine;

namespace Sab39.Sporbits.Engine;

/// <summary>
/// A playthrough of Sporbits.
/// </summary>
public sealed class SporbitsSession : GameSessionBase
{
    public override SporbitsSpace CurrentSpace => field ??= new(this);

    /// <summary>
    /// True once the puck has crashed into the player's planet. A session that is over stops
    /// advancing, whatever keeps calling <see cref="GameSessionBase.Tick"/>.
    /// </summary>
    /// <remarks>
    /// Read from the space, because crashing is a thing that happens in one. How a space says it
    /// has finished, and what a session does about it, is an open question - see the Sabric repo's
    /// Docs/architecture.md.
    /// </remarks>
    public bool IsOver => CurrentSpace.IsOver;

    protected override void OnInit() => CurrentSpace.Populate();

    protected override void OnTick(long tickStamp)
    {
        if (IsOver) return;

        base.OnTick(tickStamp);
    }
}
