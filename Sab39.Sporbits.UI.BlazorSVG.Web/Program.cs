using Sab39.Sabric.UI.BlazorSVG;
using Sab39.Sporbits.UI.BlazorSVG.Web.Client;
using Sab39.Sporbits.UI.BlazorSVG.Web.Components;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveWebAssemblyComponents();

// Unused today: the only interactive component renders on WebAssembly with prerendering off, so
// nothing is ever rendered in this container. It's here because that is a one-word change away -
// prerendering is the default - and every render mode except that one needs the seam on this side
// too. Reusing the client's generated list rather than generating a second one keeps the two
// containers from disagreeing about what renders what, which is a miserable class of bug.
//
// If anything ever does render here, GameObjectViewResolver's singleton lifetime needs revisiting
// first: one per server is shared across every connected user, not one per game.
builder.Services.AddGameObjectViewResolver()
    .AddGeneratedViews();

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
    .AddAdditionalAssemblies(typeof(Sab39.Sporbits.UI.BlazorSVG.Web.Client._Imports).Assembly);

app.Run();
