using Sab39.Sabric.UI.BlazorSVG;

using Microsoft.Extensions.DependencyInjection;

namespace Sab39.Sporbits.UI.BlazorSVG;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        /// <summary>
        /// Registers what this frontend needs in order to draw a game.
        /// </summary>
        /// <remarks>
        /// Deliberately does not bring the game along with it: a host says
        /// <c>AddSporbitsGame()</c> too, and says it itself. The two are separate because they
        /// answer separate questions - what the game is, and what draws it - and a frontend that
        /// was not this one would replace only the second.
        ///
        /// The generated <c>AddGameObjectViews()</c> cannot be folded in here. It is emitted into
        /// whichever project declares the partial that asks for it, and that cannot be this one:
        /// the Blazor view components it registers are generated in this project too, and a
        /// generator cannot read another generator's output from the same compilation.
        /// </remarks>
        public IServiceCollection AddSporbitsUI()
            => services.AddGameObjectViewResolver();
    }
}
