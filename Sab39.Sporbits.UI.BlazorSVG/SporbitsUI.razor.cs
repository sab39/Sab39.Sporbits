using System.Numerics;

using Sab39.Sabric.Engine;
using Sab39.Sabric.UI;
using Sab39.Sabric.UI.BlazorSVG;
using Sab39.Sporbits.Engine;

using Microsoft.AspNetCore.Components;

namespace Sab39.Sporbits.UI.BlazorSVG;

public sealed partial class SporbitsUI : GameUIBase<SporbitsSession>
{
    /// <summary>
    /// What this game is a game of. Rendering this component is what starts it, so the level has to
    /// be known before there is anything on screen.
    /// </summary>
    [Parameter]
    [EditorRequired]
    public ISporbitsLevel Level { get; set; } = null!;

    private Camera camera = null!;

    /// <summary>
    /// Raised once, when the game ends, carrying which way it went. The clock stops in the same
    /// breath, so what stays on screen is the frame the game ended on.
    /// </summary>
    [Parameter]
    public EventCallback<Outcome> OnGameOver { get; set; }

    /// <summary>
    /// Raised when the player abandons the game rather than playing it out.
    /// </summary>
    /// <remarks>
    /// Nothing is done about it here. Whatever is listening is expected to take this component down
    /// in response, and dropping out of the tree is already the whole of how a game ends.
    /// </remarks>
    [Parameter]
    public EventCallback OnQuit { get; set; }

    /// <summary>
    /// Raised whenever the game is paused or resumed, carrying which it now is.
    /// </summary>
    /// <remarks>
    /// A paused game is indistinguishable on screen from a hung one, so something outside has to be
    /// able to say so. It is announced rather than displayed here because this component renders
    /// once and then holds still.
    /// </remarks>
    [Parameter]
    public EventCallback<bool> OnPausedChanged { get; set; }

    /// <summary>
    /// How tall a slice of the world the camera shows, in world units. The width follows from it at
    /// 16:9.
    /// </summary>
    /// <remarks>
    /// Height first, rather than width, because the height is what the levels were tuned against:
    /// deriving the width from it means the widescreen shape fills in what used to be letterbox
    /// instead of changing how big anything looks.
    /// </remarks>
    private const float ViewHeight = 150;

    /// <summary>
    /// The window the camera is looking through, written as an SVG viewBox.
    /// </summary>
    /// <remarks>
    /// Derived from the camera's Extent, so the window this describes and the window the camera
    /// thinks it is looking through cannot drift apart.
    ///
    /// Paired with preserveAspectRatio="slice" in the markup, which scales it to *cover* the browser
    /// window rather than fit inside it. A real window is never exactly 16:9 - toolbars see to that
    /// - so a sliver of one axis is always cropped and the camera believes it can see very slightly
    /// more than it can. That is the price of a view with no edges, and it is worth paying: fitting
    /// instead would letterbox, and no amount of fading hides a boundary the content genuinely
    /// stops at.
    ///
    /// Invariant formatting because SVG attribute values are not localised and Blazor WASM takes its
    /// culture from the browser.
    /// </remarks>
    private string viewBox
        => FormattableString.Invariant(
            $"{-extent.X / 2} {-extent.Y / 2} {extent.X} {extent.Y}");

    private Vector2 extent => this.camera.Extent;

    protected override SporbitsSession CreateSession() => new(Level);

    protected override bool IsGameOver => Session.IsOver;

    // Nothing to await it with - this is a callback from a frame - and nothing left for this
    // component to do once it has said so.
    protected override void NotifyGameOver() => OnGameOver.InvokeAsync(Session.Outcome);

    /// <remarks>
    /// The camera needs the frame tracker the base built, and the follow behaviour needs a player,
    /// which is there because the base has already run the level's Populate.
    /// </remarks>
    protected override void OnInitialized()
    {
        base.OnInitialized();

        this.camera = new(Frames)
        {
            Extent = new(ViewHeight * 16 / 9, ViewHeight),
            Behaviour = new FollowBehaviour(Session.CurrentSpace.Player),
        };

        KeyboardInputSource keyboard = new(PressedKeys.Keys, "ArrowUp", "ArrowDown", "ArrowLeft", "ArrowRight");
        Session.CurrentSpace.PlayerInput.AddInputSource(keyboard);

        Frames.GapDetected += HandleGapDetected;
    }

    /// <remarks>
    /// The base has already filtered out OS auto-repeat, so a toggle here fires once per press
    /// rather than at the repeat rate.
    /// </remarks>
    protected override void OnKeyPressed(string code)
    {
        switch (code)
        {
            case "KeyP": TogglePause(); break;
            case "KeyG": ToggleGravity(); break;
            case "KeyR": ResetAverages(); break;
            case "Escape": OnQuit.InvokeAsync(); break;
        }
    }

    /// <remarks>
    /// The effect timings go with the frame averages, so that R means one thing: everything on the
    /// stats panel now measures from here. That is what makes it usable either side of the G toggle.
    /// </remarks>
    private void ResetAverages()
    {
        Frames.ResetAverages();

        foreach (var effect in Session.CurrentSpace.Effects) effect.ResetTimings();
    }

    /// <remarks>
    /// A diagnostic, not a mechanic. Gravity is the only thing in a space costing anything per pair
    /// of objects, so turning it off is how to tell a frame rate that gravity is responsible for from
    /// one it isn't - and leaving it off is not a game, since nothing orbits anything.
    ///
    /// Deliberately does not touch the averages. Reading what the toggle did means resetting them
    /// separately, once, at whichever point the comparison is meant to start from.
    /// </remarks>
    private void ToggleGravity()
    {
        var gravity = Session.CurrentSpace.Gravity;
        gravity.IsEnabled = !gravity.IsEnabled;
    }

    /// <remarks>
    /// Nothing has to be rescheduled by hand. A resumed clock says it wants time, which is what asks
    /// for the frame that starts everything moving again - and a paused one stops asking, so the
    /// loop winds down on its own once nothing else wants frames either.
    /// </remarks>
    private void TogglePause() => SetPaused(!Clock.IsPaused);

    private void SetPaused(bool isPaused)
    {
        if (Clock.IsPaused == isPaused) return;

        Clock.IsPaused = isPaused;
        OnPausedChanged.InvokeAsync(isPaused);
    }

    /// <remarks>
    /// A gap means a hidden tab or a sleeping machine, and the player is not at the keyboard when
    /// one of those ends - so the game waits to be resumed rather than starting to play itself while
    /// they are still finding the window. Sabric only announces the gap; that this is what it means
    /// is Sporbits' own decision.
    /// </remarks>
    private void HandleGapDetected(object? sender, EventArgs args) => SetPaused(true);

    /// <remarks>
    /// The base stops the loop and releases what it built; the gap subscription and the camera are
    /// this component's own. Base first, so the loop is stopped before anything it might still be
    /// driving goes away.
    /// </remarks>
    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();

        Frames.GapDetected -= HandleGapDetected;
        this.camera.Dispose();
    }
}
