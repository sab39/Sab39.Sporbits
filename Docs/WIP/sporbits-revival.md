# Sporbits — project context and target structure

> **Maintaining this doc.** Record decisions and measured facts — not transient status, not verification
> status, not history. It's re-read by every future agent before any work starts, so everything here
> has to earn that: "we considered X and rejected it" only does when it stops *every* future reader
> asking again.

## Framework architecture lives in the Sabric repo

**`C:\Code\Sab39.Sabric\Docs\architecture.md`** is the design document for the framework: the layer
structure across all three repos, the rendering seam, per-view invalidation, the physics sync sweep,
the roads not taken, and the open design questions about the Sabric/Aether/Sporbits seams.

Read it before working on anything that crosses out of Sporbits. This doc keeps only what is about
*this game* and about working in *this repo*.

## What this is

A game. The premise: we're ancient godlike beings who are bored and entertaining
ourselves by playing our favourite sport, which is a standard "get the ball in the
goal" kind of sport — except the ball is a planet or moon, and the means of control
is gravity. *Sporbits* = sport + orbits.

## Building: Sporbits only

`Sab39.Sporbits.slnx` is a superset of `Sab39.Sabric.slnx`, which is in turn a superset
of `Sab39.Core`. Building Sporbits therefore validates all three, and test-building the
other two separately proves nothing the Sporbits build hasn't already.

## Game-specific design decisions

Framework-level decisions are in Sabric's `Docs/architecture.md`. What's here is Sporbits' own.

- **No background image.** NASA public-domain space imagery was tried and dropped: it's
  more real than actual reality, and the effect is jarring rather than evocative — it
  doesn't look like what people picture when they picture space.
- **`Add` has no `hasGravity` parameter.** The old opt-out was never once used, and keeping it meant
  either the base knowing about gravity or a second overload. Gravity is a planet's own claim about
  itself instead: `PlanetBase` registers with `Space.Gravity` on attach and unregisters on detach.
  A space is therefore free to hold things mass has no opinion about — a goal, a boundary — with
  nothing to opt out of.
- **Input control is split across the two repos.** `PlayerInput` in `Sabric.Engine` collects
  `IPlayerInputSource`s and reduces them to a clamped `MovementDirection`, and `SporbitsSpace`
  holds one, which is what the UI hands the keyboard to. Sporbits' own `PlayerThrustEffect`
  says what that direction actually does — a force on the player planet — and is typed on
  `PlayerPlanet` rather than on the base object.

  Its placement in `Sporbits.Engine` is provisional and flagged in the source: it is the
  translation from player intent into a force, which is arguably an input concern rather
  than an engine one. See the input open question in Sabric's architecture doc.
- **The planet views own their colour** via scoped `.razor.css` files, so each renders
  `class="planet"` and they differ only in their own stylesheet.

## Game lifecycle as it stands

The general design question — what start, end and levels should look like in Sabric — is an open
item in Sabric's architecture doc. What exists now is entirely Sporbits-side:

**A game exists only while `SporbitsUI` is in the render tree.** `SporbitsShell` is the outermost
component and holds a four-state enum: not started, choosing a level, playing, over. `SporbitsUI`
builds its own `SporbitsSession` around the level it is handed, so rendering it *is* starting a game
and dropping it out of the tree *is* ending one. Blazor's component lifetime is the entire mechanism —
there is no reset path, nothing to tear down by hand, and no state that can survive a round trip
through the menu. Playing and game-over share a render branch, so the last frame stays on screen
behind the notice; only leaving removes the game. Dismissing goes back to the level menu rather than
the start screen, so replaying is one click.

**The session and the camera are built in `OnInitialized`, not as field initializers**, because each
needs something a field initializer cannot see: the session needs the `Level` parameter, and the
camera needs the session.

**`SporbitsUI` is `IDisposable` solely to stop the tick loop.** A scheduled animation frame cannot
be cancelled, so the loop stops by declining to schedule the next one. Game over already stops it;
the flag is what makes any *other* way of leaving a game safe.

`Outcome` lives on the space because winning and losing are things that happen in one; the session
reads it and declines to advance, whatever keeps calling `Tick`. An enum rather than a bool, because a
goal makes a win tellable from a loss and the notice has to say which. The UI polls it after each tick
rather than subscribing: the tick loop is already there, and an event would have to be raised from
inside `Tick`.

**The gate on dismissing the game-over notice is load-bearing, not decoration.** Crashing while
holding an arrow key is the normal way to lose, and `keydown` auto-repeats at the OS rate — so an
ungated notice would be dismissed before it had finished fading in. Its duration is one constant on
the shell, handed to the stylesheet as a custom property on the element, so the gate and the fade
cannot drift apart.

## Levels

**The shape of a level here is a first pass rather than a design, and Stuart's call as such.**
`ISporbitsLevel` is a `Name` and a `Populate(SporbitsSpace)` — a level that *is* the code populating a
space, which is the far end of the scale from the data-shaped level Sabric's `architecture.md` holds
up as the aspiration. It exists to get levels on screen and find out what they actually need. Nothing
about it is settled.

**Levels are registered one at a time, and registration order is menu order.** `AddSporbitsLevel<T>()`
lives in `Sporbits.Engine`; the shell injects `IEnumerable<ISporbitsLevel>` and renders a button each.
Scanning an assembly for implementations would mean reflection, which the view seam next door went to
real trouble to avoid. Registration order also beats sorting by name — a game's levels have an order
its author meant, and alphabetical is one nobody chose.

**A level has to be stateless.** One is registered once and populates a fresh space for every
playthrough, so anything that has to remember something across ticks is an object, an effect or a rule
it puts into the space rather than a field of its own.

**The space keeps the roles; a level fills them in.** `SporbitsSpace.Populate(level)` adds the gravity
controller and the thrust effect, runs the level, and adds `Player` last — last so that a level can
put the player somewhere before there is a body at the origin to move. `Player` and `Puck` are the
space's own lazily-built properties because the camera and the keyboard wiring reach for them by name
and would have nowhere to reach if a level owned them. `Goal` is settable and nullable instead, since
a level without one is a level with a different objective.

`SporbitsSpace.Orbit` places a planet in a circular orbit around another, and `OrbitalSpeed` is the
`sqrt(G(M+m)/r)` behind it. Both masses, because two bodies orbit their shared centre of mass — which
matters for a puck around a player's planet, where the ratio is nothing like a planet around a star.
**It does not work — see below.**

**A static body still pulls.** Aether's `GravityController` takes a static body's mass into account
like any other, so `Sun` is `BodyType.Static` and does not drift when the player thrusts.

## Open: `OrbitalSpeed` doesn't produce orbits

Two observations from playing it:

- **Empty space.** The puck starts at (10, 0) with velocity (0, -4) and settles into a stable orbit
  around the player's planet. The calculation says that speed is comfortably above escape velocity
  at that distance.
- **Solar system.** Everything `Orbit` placed falls straight into the sun, within about half a second
  of the level starting.

The levels' starting configurations are not the thing to adjust. What `OrbitalSpeed` is getting wrong
is the thing to find.

## Next: a headless mode

Deriving the orbit maths by arithmetic is what produced the wrong answer, and there is currently no
way to check it against what the physics actually does: the in-app browser doesn't drive
`requestAnimationFrame`, so an agent cannot watch a game run at all.

**A headless mode — a game driven and read back in text, with no browser — is the next thing to
build.** It is what would make the orbits observable rather than derived, and it is the tool the
question above needs before that question can be answered.

## Rules: the post-step counterpart to an effect

`ISporbitsRule` is `Update(delta, space)`, run from `SporbitsSpace.OnAdvance` after `base` — so on
settled state, with `World.Step` finished and the sync sweep done. The asteroid stream is the only
one.

**What forces it is spawning.** An effect runs inside `World.Step`, where Aether's world is locked;
"an effect adding game objects should not stay allowed" is an open question in Sabric's
`architecture.md`, and a rule sidesteps it rather than answering it. Whether Sabric wants a rule
concept at all, or whether an effect absorbs it, is open there too. This one is Sporbits' own, and
exists partly so that question has an instance to look at instead of only arguments.

## Curiosity: `field!` versus `= null!`

Not Sporbits-specific, and not blocking anything — parked here because it came up here. It belongs
with `Sab39.Core\Docs\csharp-style.md` eventually.

For a reference-typed member that is genuinely null for a short window but typed non-null so that
ordinary call sites don't pay for a state they're never in, there are at least three spellings:

```csharp
public GameSpaceBase Space { get; } = null!;
public GameSpaceBase Space { get => field!; }
private GameSpaceBase? space;
public GameSpaceBase Space => this.space!;
```

Stuart has always reached for the first. The question is whether the second does the same job — the
`field` keyword infers the backing field's nullability, so it may or may not be saying something
subtly different — and if it is equivalent, whether the style guide should prefer it.

The third is what `GameObjectBase` currently uses, for a reason that doesn't generalize: `Attach`
and `Detach` write it, `field` is only in scope inside the property's own accessors, and going
through a setter would mean a second null-forgiving operator to assign null on detach.

## Current state

All projects in the target structure exist and all three solutions build clean. Both
repos are public on GitHub (`sab39/Sab39.Sabric`, `sab39/Sab39.Sporbits`).

**The rendering seam is implemented, generators included.** `SporbitsUI` loops over the current
space's `GameObjects` and renders each through `GameObjectViewResolver`. The placeholder circle and
the position readouts came out and aren't missed.

**Per-view invalidation is implemented.** Game objects hold their own state, `AetherSpace`
syncs it to and from the Aether bodies around `World.Step`, `GameObjectBase` raises `Changed`, and
each view subscribes to its own object. The stats are three components — game stats and pressed
keys, both self-invalidating, inside a display-only wrapper — and the root overrides `ShouldRender`
to `false`, so it renders once and never again. The object list is a `GameObjectsView` that watches
the collection, so spawning and despawning work without the root ever rendering again.

**There is a start screen, a level menu and a game over.** `SporbitsShell` wraps everything, a game
and its view are built when a level is picked, and winning or losing freezes the game behind a notice
that fades in and then goes back to the menu on a click or a key.

**Three levels exist**: empty space, a solar system, and an asteroid stream, all of them with a goal.
Between them they are what put `Goal`, `Sun`, `ObstaclePlanet` and the rule concept in.

**`PlainPlanetView` is what `ObstaclePlanet` renders as**, which is the case it was kept for — a field
of grey planets that are only in the way. `Sun` and `Goal` have views of their own, and the puck keeps
its own however plain it currently looks, because it has to stay tellable from the obstacles.

The rest of the frontend is still knowingly placeholder, kept because being able to run the thing
and watch the tick counter move is load-bearing rather than decorative.
