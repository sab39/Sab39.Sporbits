using System.Numerics;

using Sab39.Sabric.Engine;
using Sab39.Sabric.UI;
using Sab39.Sabric.UI.BlazorSVG;
using Sab39.Sporbits.Engine;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Sab39.Sporbits.UI.BlazorSVG;

public sealed partial class SporbitsUI : IDisposable
{
    private ElementReference containerDiv;

    /// <summary>
    /// What this game is a game of. Rendering this component is what starts it, so the level has to
    /// be known before there is anything on screen.
    /// </summary>
    [Parameter]
    [EditorRequired]
    public ISporbitsLevel Level { get; set; } = null!;

    /// <remarks>
    /// Built in OnInitialized rather than as field initializers, because each needs something that
    /// isn't there until the parameters are: the session needs the level, and everything after it
    /// needs the one before.
    /// </remarks>
    private SporbitsSession session = null!;

    private GameClock clock = null!;

    private FrameTracker frames = null!;

    private BrowserFrameDriver driver = null!;

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
    /// once and then holds still - see <see cref="ShouldRender"/>.
    /// </remarks>
    [Parameter]
    public EventCallback<bool> OnPausedChanged { get; set; }

    private readonly PressedKeys pressedKeys = new();

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

    protected override void OnInitialized()
    {
        this.session = new(Level);
        this.clock = new(this.session);
        this.frames = new(this.clock);
        this.driver = new(this.frames);
        this.camera = new(this.frames) { Extent = new(ViewHeight * 16 / 9, ViewHeight) };

        this.session.Init();

        this.camera.Behaviour = new FollowBehaviour(this.session.CurrentSpace.Player);

        KeyboardInputSource keyboard = new(this.pressedKeys.Keys, "ArrowUp", "ArrowDown", "ArrowLeft", "ArrowRight");
        this.session.CurrentSpace.PlayerInput.AddInputSource(keyboard);

        this.frames.Framed += HandleFramed;
        this.frames.GapDetected += HandleGapDetected;
    }

    /// <remarks>
    /// The root renders once and then holds still, for good. Every part of it that changes is a
    /// child component that invalidates itself - the object list included, which is why this can be
    /// a flat false rather than something that has to notice a spawn. What it suppresses is the
    /// render Blazor raises automatically after the key handlers below, which would otherwise take
    /// the whole tree down with it at the OS key-repeat rate.
    /// </remarks>
    protected override bool ShouldRender() => false;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        if (firstRender)
        {
            await this.containerDiv.FocusAsync();
            this.driver.Start();
        }
    }

    /// <remarks>
    /// The keys that do something once, rather than for as long as they are held, are read from the
    /// press that adds the code to the set and not from the ones after it. keydown auto-repeats
    /// while a key is down, and Add returning false is exactly "this is a repeat", so a toggle
    /// without that gate would flicker at the OS repeat rate.
    /// </remarks>
    private void OnKeyDown(KeyboardEventArgs args)
    {
        if (!this.pressedKeys.Add(args.Code)) return;

        switch (args.Code)
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
        this.frames.ResetAverages();

        foreach (var effect in this.session.CurrentSpace.Effects) effect.ResetTimings();
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
        var gravity = this.session.CurrentSpace.Gravity;
        gravity.IsEnabled = !gravity.IsEnabled;
    }

    private void OnKeyUp(KeyboardEventArgs args) => this.pressedKeys.Remove(args.Code);

    /// <remarks>
    /// Nothing has to be rescheduled by hand. A resumed clock says it wants time, which is what asks
    /// for the frame that starts everything moving again - and a paused one stops asking, so the
    /// loop winds down on its own once nothing else wants frames either.
    /// </remarks>
    private void TogglePause() => SetPaused(!this.clock.IsPaused);

    private void SetPaused(bool isPaused)
    {
        if (this.clock.IsPaused == isPaused) return;

        this.clock.IsPaused = isPaused;
        OnPausedChanged.InvokeAsync(isPaused);
    }

    /// <remarks>
    /// A gap means a hidden tab or a sleeping machine, and the player is not at the keyboard when
    /// one of those ends - so the game waits to be resumed rather than starting to play itself while
    /// they are still finding the window. Sabric only announces the gap; that this is what it means
    /// is Sporbits' own decision.
    /// </remarks>
    private void HandleGapDetected(object? sender, EventArgs args) => SetPaused(true);

    private bool isOver;

    /// <remarks>
    /// Polled once a frame rather than subscribed to, because the outcome is a property of the space
    /// and an event would have to be raised from inside the tick that set it.
    /// </remarks>
    private void HandleFramed(object? sender, EventArgs args)
    {
        if (this.isOver || !this.session.IsOver) return;

        this.isOver = true;
        this.clock.IsPaused = true;

        // Nothing to await it with - this is a callback from a frame - and nothing left for this
        // component to do once it has said so.
        OnGameOver.InvokeAsync(this.session.Outcome);
    }

    /// <remarks>
    /// The driver stops the loop, and the tracker and the camera release the subscriptions they hold
    /// to the things above them. Without this, a component torn down mid-game would go on ticking a
    /// game nothing is rendering.
    /// </remarks>
    public void Dispose()
    {
        this.frames.Framed -= HandleFramed;
        this.frames.GapDetected -= HandleGapDetected;

        this.driver.Dispose();
        this.camera.Dispose();
        this.frames.Dispose();
    }
}
