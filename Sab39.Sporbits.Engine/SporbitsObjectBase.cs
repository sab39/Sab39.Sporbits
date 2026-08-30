using Sab39.Sabric.Engine.Aether;

namespace Sab39.Sporbits.Engine;

/// <summary>
/// Anything that exists in a game of Sporbits.
/// </summary>
/// <remarks>
/// Everything so far is a planet, but a sport needs things that aren't - goals, boundaries - and
/// this is where they and the planets meet.
/// </remarks>
public abstract class SporbitsObjectBase : AetherObjectBase
{
    public override SporbitsSpace Space => (SporbitsSpace)base.Space;
}
