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
- **Input control is split across the two repos.** `AetherInputControllerBase` in
  `Sabric.Engine.Aether` collects `IPlayerInputSource`s and reduces them to a clamped
  `MovementDirection`; it leaves Aether's abstract `Controller.Update` unimplemented, so
  every derived controller is forced to say what that direction actually does. Sporbits'
  own `PlayerInputController` supplies the answer — a force on the player planet — and
  is typed on `PlayerPlanet` rather than on the base object.

  Its placement in `Sporbits.Engine` is provisional and flagged in the source: it is the
  translation from player intent into a force, which is arguably an input concern rather
  than an engine one. See the input open question in Sabric's architecture doc.
- **The planet views own their colour** via scoped `.razor.css` files, so each renders
  `class="planet"` and they differ only in their own stylesheet.

## Game lifecycle as it stands

The general design question — what start, end and levels should look like in Sabric — is an open
item in Sabric's architecture doc. What exists now is entirely Sporbits-side:

**A game exists only while `SporbitsUI` is in the render tree.** `SporbitsShell` is the outermost
component and holds a three-state enum: not started, playing, over. `SporbitsUI` builds its own
`SporbitsSession` as a field initializer, so rendering it *is* starting a game and dropping it out of
the tree *is* ending one. Blazor's component lifetime is the entire mechanism — there is no reset
path, nothing to tear down by hand, and no state that can survive a round trip through the start
screen. Playing and game-over share a render branch, so the crashed frame stays on screen behind
the notice; only returning to the start screen removes the game.

**`SporbitsUI` is `IDisposable` solely to stop the tick loop.** A scheduled animation frame cannot
be cancelled, so the loop stops by declining to schedule the next one. Game over already stops it;
the flag is what makes any *other* way of leaving a game safe.

`IsOver` lives on the space because crashing is something that happens in one; the session reads it
and declines to advance, whatever keeps calling `Tick`. The UI polls it after each tick rather than
subscribing: the tick loop is already there, and an event would have to be raised from inside `Tick`.

**The gate on dismissing the game-over notice is load-bearing, not decoration.** Crashing while
holding an arrow key is the normal way to lose, and `keydown` auto-repeats at the OS rate — so an
ungated notice would be dismissed before it had finished fading in. Its duration is one constant on
the shell, handed to the stylesheet as a custom property on the element, so the gate and the fade
cannot drift apart.

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

**There is a start screen and a game over.** `SporbitsShell` wraps everything, a game and its view
are built when the start button is pressed, and crashing the puck into the player's planet freezes
the game behind a notice that fades in and then goes back to the start screen on a click or a key.

**`PlainPlanetView` has nothing registered against it yet**, and that's expected: both planets that
exist have dedicated roles. The puck in particular needs its own view whatever it currently looks
like — the moment there's a third planet it has to be distinguishable, and a field of grey obstacle
planets is exactly the case that would force it. The fallback is for those obstacles, not for the
puck.

The rest of the frontend is still knowingly placeholder, kept because being able to run the thing
and watch the tick counter move is load-bearing rather than decorative.
