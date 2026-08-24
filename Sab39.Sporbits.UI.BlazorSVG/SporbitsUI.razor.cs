using Sab39.Sabric.UI.BlazorSVG;
using Sab39.Sporbits.Engine;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Sab39.Sporbits.UI.BlazorSVG;

public sealed partial class SporbitsUI
{
    private ElementReference containerDiv;

    private readonly SporbitsGame game = new();

    [Inject]
    private GameObjectViewResolver views { get; set; } = default!;

    private readonly SortedSet<string> pressedKeys = [];

    private string pressedKeysMsg => string.Join(",", this.pressedKeys);

    public float ViewWidth { get; } = 200;
    public float ViewHeight { get; } = 150;

    public float ViewLeft => -ViewWidth / 2;
    public float ViewTop => -ViewHeight / 2;

    protected override void OnInitialized()
    {
        this.game.Init();

        KeyboardInputSource keyboard = new(this.pressedKeys, "ArrowUp", "ArrowDown", "ArrowLeft", "ArrowRight");
        this.game.PlayerInput.AddInputSource(keyboard);
    }

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

        // Re-renders the whole component every frame. Deliberate for now - see the rendering
        // seam section of Docs/WIP/sporbits-revival.md, where per-object components taking
        // over their own invalidation is what replaces this.
        StateHasChanged();

        ScheduleGameTick();
    }
}
