using Sab39.Sabric.UI.BlazorSVG;
using Sab39.Sporbits.Engine;
using Sab39.Sporbits.UI.BlazorSVG;
using Sab39.Sporbits.UI.BlazorSVG.Web.Client;

using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// Every type argument in the generated registrations is closed by the compiler, so the whole seam
// stays reflection-free. The using above is for AddGeneratedViews, which the generator writes into
// this project's own root namespace.
builder.Services.AddGameObjectViewResolver()
    .AddGeneratedViews()
    // The generator pairs a view with the object named in its base class, which a generic fallback
    // view doesn't name - so the planets that use one say so here.
    .AddPlanetView<ObstaclePlanet>();

// Registration order is menu order.
builder.Services.AddSporbitsLevel<EmptySpaceLevel>()
    .AddSporbitsLevel<AsteroidStreamLevel>()
    .AddSporbitsLevel<SolarSystemLevel>();

await builder.Build().RunAsync();
