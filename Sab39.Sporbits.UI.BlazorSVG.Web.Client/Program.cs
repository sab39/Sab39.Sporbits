using Sab39.Sabric.UI.BlazorSVG;
using Sab39.Sporbits.Engine;
using Sab39.Sporbits.UI.BlazorSVG;
using Sab39.Sporbits.UI.BlazorSVG.Web.Client;

using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// Every type argument in the generated registrations is closed by the compiler, so the whole seam
// stays reflection-free. Planets with no view of their own need no line here: the generator closes
// the generic fallback view over each of them.
builder.Services.AddGameObjectViewResolver()
    .AddGameObjectViews();

// Registration order is menu order.
builder.Services.AddSporbitsLevel<EmptySpaceLevel>()
    .AddSporbitsLevel<AsteroidStreamLevel>()
    .AddSporbitsLevel<SolarSystemLevel>();

await builder.Build().RunAsync();
