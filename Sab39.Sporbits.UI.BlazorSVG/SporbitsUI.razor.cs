using System.Numerics;

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
    /// Built in OnInitialized rather than as field initializers, because both need something that
    /// isn't there until the parameters are: the session needs the level, and the camera needs the
    /// session.
    /// </remarks>
    private SporbitsSession session = null!;

    private Camera camera = null!;

    /// <summary>
    /// Raised once, when the game ends, carrying which way it went. The tick loop stops in the same
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
        this.camera = new(this.session) { Extent = new(ViewHeight * 16 / 9, ViewHeight) };

        this.session.Init();

        this.camera.Behaviour = new FollowBehaviour(this.session.CurrentSpace.Player);

        KeyboardInputSource keyboard = new(this.pressedKeys.Keys, "ArrowUp", "ArrowDown", "ArrowLeft", "ArrowRight");
        this.session.CurrentSpace.PlayerInput.AddInputSource(keyboard);
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
            ScheduleGameTick();
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
            case "Escape": OnQuit.InvokeAsync(); break;
        }
    }

    private void OnKeyUp(KeyboardEventArgs args) => this.pressedKeys.Remove(args.Code);

    private bool isPaused;

    /// <remarks>
    /// Resuming has to restart the loop by hand, because pausing stopped it: a paused game costs
    /// nothing rather than idling through frames it has no use for, which matters most on the
    /// battery-powered things this is nicest to play on.
    /// </remarks>
    private void TogglePause()
    {
        this.isPaused = !this.isPaused;
        this.isResuming = !this.isPaused;

        OnPausedChanged.InvokeAsync(this.isPaused);

        if (!this.isPaused) ScheduleGameTick();
    }

    /// <summary>
    /// Whether a frame has been asked for and not yet arrived.
    /// </summary>
    /// <remarks>
    /// Pausing and resuming again inside a single frame's gap would otherwise ask for a second frame
    /// while the first is still in flight, and from then on every frame would schedule two - the
    /// loop doubling with each round trip.
    /// </remarks>
    private bool isFramePending;

    private void ScheduleGameTick()
    {
        if (this.isDisposed || this.isFramePending) return;

        this.isFramePending = true;
        BrowserEnvironment.RequestAnimationFrame(TriggerGameTick);
    }

    /// <summary>
    /// How far the browser's clock has run ahead of the game's, which is all the time the game has
    /// spent paused.
    /// </summary>
    /// <remarks>
    /// Subtracted from every stamp the session is handed, so that a pause takes no game time.
    /// Without it the first tick after a resume would arrive with the whole length of the pause in
    /// its delta and advance the world by all of it at once. The session measures no time of its own
    /// - what a tick is worth is the scheduler's to decide - so this is the right side of that line
    /// for it to happen on.
    /// </remarks>
    private long pausedMillis;

    /// <summary>
    /// The raw browser timestamp of the last frame that actually ticked.
    /// </summary>
    /// <remarks>
    /// Nothing measures the pause while it is happening, since there are no frames to measure it
    /// with. The gap only has to be known once, on the far side, and this is what it is measured
    /// against when it gets there.
    /// </remarks>
    private long lastStamp;

    private bool isResuming;

    private void TriggerGameTick(double tickStamp)
    {
        this.isFramePending = false;

        // The frame already in flight when the pause key was pressed still arrives. Ticking it
        // would be a free frame of play after the game was meant to have stopped.
        if (this.isPaused) return;

        var stamp = (long)tickStamp;

        if (this.isResuming)
        {
            this.isResuming = false;
            this.pausedMillis += stamp - this.lastStamp;
        }

        this.lastStamp = stamp;
        this.session.Tick(stamp - this.pausedMillis);

        if (this.session.IsOver)
        {
            // Nothing to await it with - the loop is driven by a void callback from JS - and
            // nothing left for this component to do once it has said so.
            OnGameOver.InvokeAsync(this.session.Outcome);
            return;
        }

        ScheduleGameTick();
    }

    private bool isDisposed;

    /// <remarks>
    /// A scheduled animation frame cannot be cancelled, so the loop is stopped by declining to
    /// schedule the next one. Without this, a component torn down mid-game would go on ticking a
    /// game nothing is rendering.
    ///
    /// The camera is disposed here for the same reason it is constructed here: it holds a
    /// subscription to the session, and this component is what owns its lifetime.
    /// </remarks>
    public void Dispose()
    {
        this.isDisposed = true;
        this.camera.Dispose();
    }
}
