using Sab39.Sabric.UI.BlazorSVG;
using Sab39.Sporbits.Engine;
using Sab39.Sporbits.UI.BlazorSVG;

using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// Every type argument here is closed by the compiler, so the whole seam stays reflection-free.
// The view registrations are what a source generator is eventually meant to emit.
builder.Services.AddGameObjectViewResolver()
    .AddGameObjectView<PlayerPlanet, PlayerPlanetView>()
    .AddGameObjectView<PuckPlanet, PuckPlanetView>();

await builder.Build().RunAsync();
