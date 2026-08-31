using System.Numerics;

using nkast.Aether.Physics2D.Dynamics;

using Sab39.Sabric.Engine.Aether;

namespace Sab39.Sporbits.Engine;

/// <summary>
/// A short-range push on the puck, away from the surface of every planet registered with it.
/// </summary>
/// <remarks>
/// Experimental, and Stuart's to keep or drop. What it is for: a puck that lands on a planet stays
/// there, and nothing the player can do detaches it. A push that is overwhelming at the surface and
/// negligible a planet's width away should leave the puck skimming rather than resting, and so still
/// catchable.
///
/// <strong>Separation everywhere in this type means the gap between the two surfaces, in units of the
/// planet's radius</strong> - centres, less both radii, over the planet's radius. Two things follow
/// from that one choice. Falling off with the fourth power of it is what makes the push short-ranged,
/// because a surface gap is small in absolute terms where a centre-to-centre distance never is; and
/// dividing by the planet's radius is what keeps a pebble from behaving like a star, since the whole
/// curve then sits at the scale of whatever it belongs to.
///
/// Mass is not in it. The force is between two <em>surfaces</em>, so what is behind either of them
/// does not come into it. That is not the same as saying size doesn't matter - size is in it twice,
/// through the radius the separation is measured in.
///
/// It does obey Newton's third law: the planet takes the opposite push, exactly as it does under
/// gravity, so nothing here gets to be immovable except by saying so with its body type.
/// </remarks>
public sealed class PuckRepulsionEffect(PuckPlanet puck, float strength) : AetherEffectBase
{
    public PuckPlanet Puck { get; } = puck;

    /// <summary>
    /// The force at a separation of one, which is a surface gap as wide as the planet's own radius.
    /// Closer in it is Strength/s^4.
    /// </summary>
    /// <remarks>
    /// No mass in it, so this is not comparable with <see cref="AetherGravityEffect.Strength"/>. What
    /// makes the two commensurable is a standoff: solve Strength/s^4 against gravity's own pull at
    /// the same place. At contact that pull is around 17 for an obstacle of radius 1.5, around 9 for
    /// an asteroid of radius 0.6, and around 98 for the sun.
    /// </remarks>
    public float Strength { get; } = strength;

    /// <summary>
    /// How small the separation is allowed to count as being, however small it actually is.
    /// </summary>
    /// <remarks>
    /// A floor is needed rather than nice to have: the separation reaches zero, where the force is
    /// infinite, and goes negative when the solver presses the puck into a planet within a step -
    /// and an even power makes a negative separation look like a positive one, so the force would
    /// come back down again on the wrong side of the surface.
    ///
    /// Clamping the separation is the same thing as capping the force, and is the half of it worth
    /// naming: it is where the push stops growing, so it governs how hard the hardest possible kick
    /// is. Being in radii like everything else, that cap is the same force for every planet - a
    /// deliberate consequence of scaling the curve rather than the ceiling, and reachable only by
    /// interpenetration, since a settled standoff sits well outside it.
    /// </remarks>
    public float MinimumSeparation { get; init; } = 0.2f;

    /// <remarks>
    /// Registered rather than swept up, and for a stronger reason than gravity's: the player's own
    /// planet is a planet like any other and must not repel, or catching the puck would be
    /// impossible. See <see cref="PlanetBase.RepelsPuck"/>.
    /// </remarks>
    private readonly List<PlanetBase> planets = [];

    public IReadOnlyList<PlanetBase> Planets => this.planets;

    public void Add(PlanetBase planet) => this.planets.Add(planet);

    public bool Remove(PlanetBase planet) => this.planets.Remove(planet);

    /// <remarks>
    /// The puck's position is read once for the whole sweep, and the attachment check is what makes
    /// reading it safe at all - a level is free to leave the puck out of the space, and an unattached
    /// object has no body for the context to read.
    /// </remarks>
    protected override void Update(long delta, IAetherEffectContext context)
    {
        if (!Puck.IsAttached) return;

        var position = context.GetPosition(Puck);

        foreach (var planet in this.planets) Repel(context, planet, position);
    }

    /// <remarks>
    /// Positions come from the context rather than from the objects because this runs inside
    /// World.Step, where the body is the live copy and an object's own Position is still the one the
    /// last sync left on it.
    ///
    /// Coincident bodies are skipped rather than clamped, as in gravity: the force is undefined there
    /// and clamping would invent a direction to point it in.
    /// </remarks>
    private void Repel(IAetherEffectContext context, PlanetBase planet, Vector2 position)
    {
        var offset = position - context.GetPosition(planet);
        var distance = offset.Length();

        if (distance < Epsilon) return;

        var gap = (distance - planet.Radius - Puck.Radius) / planet.Radius;
        var separation = float.Max(gap, MinimumSeparation);
        var falloff = separation * separation * separation * separation;

        // Divided by distance as well as by the falloff, because offset is the whole displacement
        // rather than a direction: the extra power is what normalises it.
        var force = offset * (Strength / (falloff * distance));

        if (IsMovable(Puck)) context.ApplyForce(Puck, force);
        if (IsMovable(planet)) context.ApplyForce(planet, -force);
    }

    private const float Epsilon = 1e-6f;

    // A kinematic body ignores a force exactly as a static one does, so neither is worth the arithmetic.
    private static bool IsMovable(PlanetBase planet) => planet.BodyType is BodyType.Dynamic;
}
