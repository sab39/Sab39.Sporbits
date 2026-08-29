using Sab39.Sabric.UI.BlazorSVG;
using Sab39.Sporbits.UI.BlazorSVG.Web.Client;

using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// Every type argument in the generated registrations is closed by the compiler, so the whole seam
// stays reflection-free. The using above is for AddGeneratedViews, which the generator writes into
// this project's own root namespace.
builder.Services.AddGameObjectViewResolver()
    .AddGeneratedViews();

await builder.Build().RunAsync();
