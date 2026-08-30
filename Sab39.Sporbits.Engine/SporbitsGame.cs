using Sab39.Sabric.Engine.Aether;

using nkast.Aether.Physics2D.Controllers;
using Sab39.Sabric.Engine;

namespace Sab39.Sporbits.Engine;

/// <summary>
/// A game of Sporbits.
/// </summary>
public sealed class SporbitsGame : AetherGameBase
{
    public PlayerPlanet Player => field ??= new(this, default);
    public PuckPlanet Puck => field ??= new(this, new(10, 0)) { Velocity = new(0, -4) };

    public GravityController Gravity => field ??= new(8);
    public PlayerInputController PlayerInput => field ??= new(Player, 16);

    protected override void OnInit()
    {
        World.Add(Gravity);

        AddGameObject(Player);
        AddGameObject(Puck);

        World.Add(PlayerInput);
    }

    protected override void OnAddGameObject(AetherGameObjectBase obj) => Gravity.AddBody(obj.Body);

    // Aether's GravityController has no RemoveBody, only the list AddBody appends to.
    protected override void OnRemoveGameObject(AetherGameObjectBase obj)
    {
        Gravity.Bodies.Remove(obj.Body);
        base.OnRemoveGameObject(obj);
    }
}
