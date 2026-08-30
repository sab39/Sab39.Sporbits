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

    private readonly PressedKeys pressedKeys = new();

    /// <remarks>
    /// Both derived from the camera's Extent, so the window the viewBox describes and the window
    /// the camera thinks it is looking through cannot drift apart. Invariant formatting because SVG
    /// attribute values are not localised and Blazor WASM takes its culture from the browser.
    /// </remarks>
    private string viewBox
        => FormattableString.Invariant(
            $"{-extent.X / 2} {-extent.Y / 2} {extent.X} {extent.Y}");

    private string aspectRatio => FormattableString.Invariant($"{extent.X}/{extent.Y}");

    private Vector2 extent => this.camera.Extent;

    protected override void OnInitialized()
    {
        this.session = new(Level);
        this.camera = new(this.session) { Extent = new(200, 150) };

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

    private void OnKeyDown(KeyboardEventArgs args) => this.pressedKeys.Add(args.Code);
    private void OnKeyUp(KeyboardEventArgs args) => this.pressedKeys.Remove(args.Code);

    private void ScheduleGameTick()
    {
        if (this.isDisposed) return;

        BrowserEnvironment.RequestAnimationFrame(TriggerGameTick);
    }

    private void TriggerGameTick(double tickStamp)
    {
        this.session.Tick((long)tickStamp);

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
