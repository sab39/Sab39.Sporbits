using Sab39.Sabric.UI.BlazorSVG;
using Sab39.Sporbits.Engine;

using Microsoft.Extensions.DependencyInjection;

namespace Sab39.Sporbits.UI.BlazorSVG;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        /// <summary>
        /// Registers the plain circle view for a planet that hasn't been given a look of its own.
        /// </summary>
        public IServiceCollection AddPlanetView<TPlanet>()
            where TPlanet : PlanetBase
            => services.AddGameObjectView<TPlanet, PlainPlanetView<TPlanet>>();
    }
}
