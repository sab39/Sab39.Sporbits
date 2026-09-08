using Sab39.Sporbits.Engine;
using Sab39.Sporbits.UI.BlazorSVG;
using Sab39.Sporbits.UI.BlazorSVG.Web.Client;

using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddSporbitsGame();

// Every type argument in the generated registrations is closed by the compiler, so the whole seam
// stays reflection-free. Planets with no view of their own need no line here: the generator closes
// the generic fallback view over each of them. AddGameObjectViews is the one part AddSporbitsUI
// cannot bring with it - see the remarks there.
builder.Services.AddSporbitsUI()
    .AddGameObjectViews();

await builder.Build().RunAsync();
