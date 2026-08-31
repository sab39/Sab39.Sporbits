using System.Numerics;

using Sab39.Sabric.Engine;
using Sab39.Sabric.Engine.Aether;

namespace Sab39.Sporbits.Engine;

/// <summary>
/// The space a game of Sporbits is played in: the planets, and the gravity between them.
/// </summary>
public sealed class SporbitsSpace(GameSessionBase session) : AetherSpace(session)
{
    /// <remarks>
    /// Built on demand and held by the space rather than by the level that arranges them, because
    /// everything else that reaches for the player - the camera, the thrust effect - asks the space
    /// for it and would have nowhere to ask if a level owned it. A level says where they go.
    /// </remarks>
    public PlayerPlanet Player => field ??= new();

    public PuckPlanet Puck => field ??= new();

    /// <summary>
    /// Where the puck has to end up, for a level that has one.
    /// </summary>
    /// <remarks>
    /// Settable and nullable where the other two are neither: a level with no goal is a level with
    /// some other objective, which is what the survival ideas in Docs/WIP are.
    /// </remarks>
    public Goal? Goal { get; set; }

    public AetherGravityEffect Gravity => field ??= new(12);

    /// <remarks>
    /// Held by the space rather than by the effect that reads it, so that the UI has somewhere to
    /// hand a source that isn't inside an effect - and so that more than one effect could read the
    /// same input if the game ever wanted that.
    /// </remarks>
    public PlayerInput PlayerInput => field ??= new();

    public PlayerThrustEffect PlayerThrust => field ??= new(Player, PlayerInput, 24);

    /// <summary>
    /// Fills the space: the parts every level has, and then the level's own.
    /// </summary>
    /// <remarks>
    /// Effects first, so that nothing attaching can find the gravity it registers with missing.
    /// Nothing today would - the property builds it on demand - but the order that needs no such
    /// argument is free.
    ///
    /// The player goes in last so that a level can put it somewhere before there is a body sitting
    /// at the origin to be moved. Every level has one, so remembering to add it is not a level's job.
    /// </remarks>
    public void Populate(ISporbitsLevel level)
    {
        AddEffect(Gravity);
        AddEffect(PlayerThrust);

        level.Populate(this);

        Add(Player);
    }

    /// <summary>
    /// The speed a body needs to hold a circular orbit at this distance from a central mass.
    /// </summary>
    /// <remarks>
    /// Both masses, because two bodies orbit their shared centre of mass rather than the heavier
    /// one - which matters for a puck around a player's planet, where the ratio is nothing like a
    /// planet around a star.
    ///
    /// Assumes gravity falling off with the square of distance, which is what
    /// <see cref="AetherGravityEffect"/> does - and what Aether's own GravityController does not, for
    /// which see the remarks on that type.
    /// </remarks>
    public float OrbitalSpeed(float centralMass, float orbitingMass, float distance)
        => float.Sqrt(Gravity.Strength * (centralMass + orbitingMass) / distance);

    /// <summary>
    /// Places a planet in a circular orbit around another, <paramref name="angle"/> radians round
    /// from it.
    /// </summary>
    /// <remarks>
    /// Only says where the two start; both still have to be added. Masses come from size and density
    /// rather than from the bodies, because neither planet has a body until it is attached.
    ///
    /// Circular at the moment it is set and rarely for long: everything else in the space pulls too,
    /// and the player thrusting is one more perturbation.
    /// </remarks>
    public void Orbit(PlanetBase planet, PlanetBase around, float distance, float angle = 0)
    {
        var (sin, cos) = float.SinCos(angle);
        var speed = OrbitalSpeed(around.ComputedMass, planet.ComputedMass, distance);

        planet.Position = around.Position + (new Vector2(cos, sin) * distance);

        // Across the pull rather than along it, and relative to whatever the centre is already doing.
        planet.Velocity = around.Velocity + (new Vector2(-sin, cos) * speed);
    }

    private readonly List<ISporbitsRule> rules = [];

    public IReadOnlyList<ISporbitsRule> Rules => this.rules;

    public void AddRule(ISporbitsRule rule) => this.rules.Add(rule);

    /// <remarks>
    /// The rules run after base, which is where the world steps and the objects are synced back out
    /// of it - so a rule sees settled state and can add and remove objects freely, which is the
    /// whole reason it isn't an effect. See <see cref="ISporbitsRule"/>.
    /// </remarks>
    protected override void OnAdvance(long delta)
    {
        base.OnAdvance(delta);

        foreach (var rule in this.rules) rule.Update(delta, this);
    }

    /// <summary>
    /// How the game stands, and the space's own answer rather than the session's, because winning
    /// and losing are things that happen in a space.
    /// </summary>
    public Outcome Outcome { get; private set; }

    public bool IsOver => Outcome is not Outcome.Playing;

    /// <remarks>
    /// The puck reaching the goal is the only way to win, and it is tested first because the goal is
    /// the puck's destination and the player's hazard at once. Everything else that touches the
    /// player's planet is a crash - the puck, an obstacle, the sun - which is what makes flying
    /// through a system dangerous rather than scenic.
    /// </remarks>
    protected override void OnCollision(CollisionInfo collision)
    {
        if (IsOver) return;

        if (Goal is { } goal && collision.Involves(Puck, goal))
        {
            Outcome = Outcome.Won;
        }
        else if (collision.Involves(Player))
        {
            Outcome = Outcome.Lost;
        }
    }
}
