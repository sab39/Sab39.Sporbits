using Sab39.Sabric.UI.BlazorSVG;
using Sab39.Sporbits.Engine;

namespace Sab39.Sporbits.UI.BlazorSVG;

/// <summary>
/// Shared geometry for the planet views - everything an SVG circle needs, and nothing about
/// how any particular planet looks.
/// </summary>
public abstract class PlanetView<TPlanet> : GameObjectView<TPlanet>
    where TPlanet : PlanetBase
{
    public TPlanet Planet => GameObject;

    public float X => Planet.Position.X;
    public float Y => Planet.Position.Y;
    public float Radius => Planet.Radius;
}
