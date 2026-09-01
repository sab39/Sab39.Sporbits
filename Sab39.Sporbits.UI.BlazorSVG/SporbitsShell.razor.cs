using Sab39.Sporbits.Engine;

using Microsoft.AspNetCore.Components;

namespace Sab39.Sporbits.UI.BlazorSVG;

/// <summary>
/// The outermost thing on screen: the start screen, the level menu, a game being played, and the
/// notice that says how it went.
/// </summary>
/// <remarks>
/// There is nothing here that resets a game, because nothing needs one. SporbitsUI builds its own
/// SporbitsSession around the level it is handed, so rendering it is what starts a game and dropping
/// it out of the tree is what ends one - Blazor's component lifetime is the entire mechanism.
/// </remarks>
public sealed partial class SporbitsShell
{
    private enum ShellState { NotStarted, ChoosingLevel, Playing, GameOver }

    private ShellState state;

    private ElementReference startButton;
    private ElementReference levelMenu;
    private ElementReference gameOverNotice;

    /// <summary>
    /// Every level the game was built with, in the order they were registered.
    /// </summary>
    /// <remarks>
    /// Registration order is menu order: MS.DI hands these back in the order they were added, which
    /// is an order the game's author meant, where alphabetical would be one nobody chose.
    /// </remarks>
    [Inject]
    private IEnumerable<ISporbitsLevel> levels { get; set; } = [];

    private ISporbitsLevel level = null!;

    private void ChooseLevel()
    {
        this.state = ShellState.ChoosingLevel;
        this.needsFocus = true;
    }

    private void StartGame(ISporbitsLevel choice)
    {
        this.level = choice;
        this.state = ShellState.Playing;
    }

    /// <summary>
    /// How long the notice takes to fade in, and how long it ignores being dismissed for.
    /// </summary>
    /// <remarks>
    /// Handed to the stylesheet as a custom property rather than written out in both places. The
    /// two have to agree, and the gate is not decoration: crashing while holding an arrow key is
    /// the normal way to lose, and key repeat would dismiss the notice before it had finished
    /// appearing.
    /// </remarks>
    private const int FadeMillis = 1000;

    private bool isDismissable;

    private Outcome outcome;

    private string outcomeClass => this.outcome is Outcome.Won ? "won" : "lost";

    private async Task HandleGameOver(Outcome result)
    {
        this.outcome = result;
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

    /// <remarks>
    /// Back to the menu rather than to the start screen: the level just played is one click away
    /// again, and so is the next one, which is what trying levels out wants.
    /// </remarks>
    private void Dismiss()
    {
        if (!this.isDismissable) return;

        this.state = ShellState.ChoosingLevel;
        this.needsFocus = true;
    }

    /// <summary>
    /// Abandoning a game part-way through, which lands in the same place finishing one does.
    /// </summary>
    /// <remarks>
    /// No notice on the way out, unlike a game that ended on its own: there is no result to read,
    /// and the player asked to leave.
    /// </remarks>
    private void Quit()
    {
        this.isPaused = false;
        this.state = ShellState.ChoosingLevel;
        this.needsFocus = true;
    }

    /// <summary>
    /// Whether the game on screen is paused, which is only ever known here because the game said so.
    /// </summary>
    /// <remarks>
    /// Held out here rather than in SporbitsUI because that component renders once and never again,
    /// so it is in no position to put anything on screen that changes.
    /// </remarks>
    private bool isPaused;

    private void HandlePausedChanged(bool paused) => this.isPaused = paused;

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
            case ShellState.ChoosingLevel: await this.levelMenu.FocusAsync(); break;
            case ShellState.GameOver: await this.gameOverNotice.FocusAsync(); break;
        }
    }
}
