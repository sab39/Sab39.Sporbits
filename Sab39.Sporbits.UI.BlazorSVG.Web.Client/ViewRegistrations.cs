using Sab39.Core.TypedDispatch;
using Sab39.Sabric.UI.BlazorSVG;

using Microsoft.Extensions.DependencyInjection;

namespace Sab39.Sporbits.UI.BlazorSVG.Web.Client;

/// <summary>
/// Registers the view for every game object this project can see.
/// </summary>
/// <remarks>
/// It belongs here rather than in the UI.BlazorSVG class library because the views are .razor files:
/// their classes are another source generator's output, and a source generator cannot see one. They
/// are only visible as ordinary metadata from a project that references the assembly they ended up
/// in - which is also the composition root that needs them.
///
/// Naming the method here rather than having the generator invent one is what makes the whole
/// mechanism opt-in: a project that marks nothing gets nothing generated. The server host reuses
/// this one rather than declaring a second, so the two containers cannot disagree about what renders
/// what.
///
/// A partial member cannot live in an extension block - the extension declaration is itself the
/// containing type and cannot be partial (CS0751) - so this is a partial static class with classic
/// this-parameter syntax, against the usual preference.
/// </remarks>
public static partial class ViewRegistrations
{
    [RegisterDispatch(typeof(GameObjectViewBase<>))]
    public static partial IServiceCollection AddGameObjectViews(this IServiceCollection services);
}
