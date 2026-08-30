namespace Sab39.Sporbits.Engine;

/// <summary>
/// How a game of Sporbits stands: still being played, or over and which way.
/// </summary>
/// <remarks>
/// The smallest thing that tells a win from a loss, which is what a goal makes necessary - a bool
/// cannot say which happened. How a space reports that it has finished, and what a session does
/// about it, is an open question in the Sabric repo's Docs/architecture.md, and this is not an
/// answer to it.
/// </remarks>
public enum Outcome
{
    Playing,
    Won,
    Lost,
}
