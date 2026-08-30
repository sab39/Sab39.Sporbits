namespace Sab39.Sporbits.Engine;

/// <summary>
/// One arrangement of a space: what a game of Sporbits starts out containing.
/// </summary>
/// <remarks>
/// Code that populates a space, which is the "something working" answer rather than the one being
/// aimed at - a level being data, separate from the space that runs it, is the aspiration recorded
/// in the Sabric repo's Docs/architecture.md, and what a Level type ends up looking like is open
/// there.
///
/// An implementation has to be stateless. One is registered once and populates a fresh space for
/// every playthrough, so anything that has to remember something across ticks is an object, an
/// effect or a rule it puts into the space rather than a field of its own.
/// </remarks>
public interface ISporbitsLevel
{
    /// <summary>
    /// What this level is called on the menu.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Puts this level's own contents into a space that already has the parts every level shares.
    /// </summary>
    void Populate(SporbitsSpace space);
}
