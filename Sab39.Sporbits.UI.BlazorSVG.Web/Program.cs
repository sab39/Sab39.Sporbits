using Sab39.Sporbits.Engine;
using Sab39.Sporbits.UI.BlazorSVG;
using Sab39.Sporbits.UI.BlazorSVG.Web.Client;
using Sab39.Sporbits.UI.BlazorSVG.Web.Components;

var builder = WebApplication.CreateBuilder(args);

// Both modes, because which one a game runs under is decided per request rather than per build -
// see Home.razor. Registering one and rendering the other is a runtime error, so they travel
// together with their two AddInteractive*RenderMode counterparts below.
builder.Services.AddRazorComponents()
    .AddInteractiveWebAssemblyComponents()
    .AddInteractiveServerComponents();

builder.Services.AddSporbitsGame();

// Unused today: the only interactive component renders on WebAssembly with prerendering off, so
// nothing is ever rendered in this container. It's here because that is a one-word change away -
// prerendering is the default - and every render mode except that one needs the seam on this side
// too. Reusing the client's generated list rather than generating a second one keeps the two
// containers from disagreeing about what renders what, which is a miserable class of bug.
builder.Services.AddSporbitsUI()
    .AddGameObjectViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveWebAssemblyRenderMode()
    .AddInteractiveServerRenderMode()
    .AddAdditionalAssemblies(typeof(Sab39.Sporbits.UI.BlazorSVG.Web.Client._Imports).Assembly);

app.Run();
