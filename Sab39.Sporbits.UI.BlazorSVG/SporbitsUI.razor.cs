using Sab39.Sabric.UI.BlazorSVG;
using Sab39.Sporbits.Engine;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Sab39.Sporbits.UI.BlazorSVG;

public sealed partial class SporbitsUI
{
    private ElementReference containerDiv;

    private readonly SporbitsGame game = new();

    private readonly PressedKeys pressedKeys = new();

    public float ViewWidth { get; } = 200;
    public float ViewHeight { get; } = 150;

    public float ViewLeft => -ViewWidth / 2;
    public float ViewTop => -ViewHeight / 2;

    protected override void OnInitialized()
    {
        this.game.Init();

        KeyboardInputSource keyboard = new(this.pressedKeys.Keys, "ArrowUp", "ArrowDown", "ArrowLeft", "ArrowRight");
        this.game.PlayerInput.AddInputSource(keyboard);
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

    private void ScheduleGameTick() => BrowserEnvironment.RequestAnimationFrame(TriggerGameTick);

    private void TriggerGameTick(double tickStamp)
    {
        this.game.Tick((long)tickStamp);
        ScheduleGameTick();
    }
}
