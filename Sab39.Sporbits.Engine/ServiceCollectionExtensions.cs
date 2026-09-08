using Microsoft.Extensions.DependencyInjection;

namespace Sab39.Sporbits.Engine;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        /// <summary>
        /// Registers the game itself: every level Sporbits ships, in menu order.
        /// </summary>
        /// <remarks>
        /// What a host needs to have a game at all, and nothing about how it is displayed - a
        /// frontend that was not Blazor at all would want exactly this line and none of the one
        /// next to it. Adding a level to Sporbits is a line here rather than a line in every host.
        /// </remarks>
        public IServiceCollection AddSporbitsGame()
            => services.AddSporbitsLevel<EmptySpaceLevel>()
                .AddSporbitsLevel<AsteroidStreamLevel>()
                .AddSporbitsLevel<SolarSystemLevel>();

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
