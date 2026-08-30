using Microsoft.AspNetCore.Components;

namespace Sab39.Sporbits.UI.BlazorSVG;

/// <summary>
/// The outermost thing on screen: the start screen, a game being played, and the game-over notice.
/// </summary>
/// <remarks>
/// There is nothing here that resets a game, because nothing needs one. SporbitsUI builds its own
/// SporbitsSession as a field, so rendering it is what starts a game and dropping it out of the
/// tree is what ends one - Blazor's component lifetime is the entire mechanism.
/// </remarks>
public sealed partial class SporbitsShell
{
    private enum ShellState { NotStarted, Playing, GameOver }

    private ShellState state;

    private ElementReference startButton;
    private ElementReference gameOverNotice;

    private void StartGame() => this.state = ShellState.Playing;

    /// <summary>
    /// How long the game-over notice takes to fade in, and how long it ignores being dismissed for.
    /// </summary>
    /// <remarks>
    /// Handed to the stylesheet as a custom property rather than written out in both places. The
    /// two have to agree, and the gate is not decoration: crashing while holding an arrow key is
    /// the normal way to lose, and key repeat would dismiss the notice before it had finished
    /// appearing.
    /// </remarks>
    private const int FadeMillis = 1000;

    private bool isDismissable;

    private async Task HandleGameOver()
    {
        this.state = ShellState.GameOver;
        this.isDismissable = false;
        this.needsFocus = true;

        // On screen before the wait starts, or the gate spends its second counting down against
        // a fade that hasn't begun.
        await InvokeAsync(StateHasChanged);
        await Task.Delay(FadeMillis);

        this.isDismissable = true;
        await InvokeAsync(StateHasChanged);
    }

    private void Dismiss()
    {
        if (!this.isDismissable) return;

        this.state = ShellState.NotStarted;
        this.needsFocus = true;
    }

    /// <remarks>
    /// Focus is moved by hand into every state that reads the keyboard from an element of its own.
    /// Playing is the one state this leaves alone: SporbitsUI focuses its own container on its
    /// first render, and it is the only thing on screen that wants the keys.
    /// </remarks>
    private bool needsFocus = true;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        if (!this.needsFocus) return;
        this.needsFocus = false;

        switch (this.state)
        {
            case ShellState.NotStarted: await this.startButton.FocusAsync(); break;
            case ShellState.GameOver: await this.gameOverNotice.FocusAsync(); break;
        }
    }
}
