using Sab39.Sabric.Engine;

namespace Sab39.Sporbits.Engine;

/// <summary>
/// A playthrough of Sporbits, on the level it was built with.
/// </summary>
/// <remarks>
/// One level per session, because nothing yet distinguishes a level being finished from the game
/// being over. What a session does when a space finishes is an open question in the Sabric repo's
/// Docs/architecture.md.
/// </remarks>
public sealed class SporbitsSession(ISporbitsLevel level) : GameSessionBase
{
    public ISporbitsLevel Level { get; } = level;

    public override SporbitsSpace CurrentSpace => field ??= new(this);

    /// <summary>
    /// How the game stands. A session that is over stops advancing, whatever keeps calling
    /// <see cref="GameSessionBase.Tick"/>.
    /// </summary>
    /// <remarks>
    /// Read from the space, because winning and losing are things that happen in one. How a space
    /// says it has finished, and what a session does about it, is an open question - see the Sabric
    /// repo's Docs/architecture.md.
    /// </remarks>
    public Outcome Outcome => CurrentSpace.Outcome;

    public bool IsOver => CurrentSpace.IsOver;

    protected override void OnInit() => CurrentSpace.Populate(Level);

    protected override void OnTick(long tickStamp)
    {
        if (IsOver) return;

        base.OnTick(tickStamp);
    }
}
