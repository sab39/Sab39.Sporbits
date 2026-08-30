using Microsoft.Extensions.DependencyInjection;

namespace Sab39.Sporbits.Engine;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        /// <summary>
        /// Adds a level to the game. Registration order is the order they appear on the menu.
        /// </summary>
        /// <remarks>
        /// Written out one call at a time rather than found by scanning. The container is asked for
        /// every ISporbitsLevel and can only answer for what it was told about; the alternative is
        /// assembly scanning, which means reflection, and the view seam next door went to real
        /// trouble to have none - see the Sabric repo's Docs/architecture.md.
        ///
        /// Registration order is also better than sorting by name: a game's levels have an order its
        /// author meant, and alphabetical is not it.
        ///
        /// A singleton because a level has nothing to remember - see <see cref="ISporbitsLevel"/>.
        /// </remarks>
        public IServiceCollection AddSporbitsLevel<TLevel>()
            where TLevel : class, ISporbitsLevel
            => services.AddSingleton<ISporbitsLevel, TLevel>();
    }
}
