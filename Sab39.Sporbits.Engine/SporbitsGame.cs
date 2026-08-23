using Sab39.Sabric.Engine.Aether;

using nkast.Aether.Physics2D.Controllers;
using Sab39.Sabric.Engine;

namespace Sab39.Sporbits.Engine;

/// <summary>
/// The game itself: a player planet, a puck orbiting it, and the mutual gravity between them.
/// </summary>
public sealed class SporbitsGame : AetherGameBase
{
    public PlayerPlanet Player => field ??= new(this, default);
    public PuckPlanet Puck => field ??= new(this, new(10, 0)) { Velocity = new(0, -4) };

    public GravityController Gravity => field ??= new(8);
    public PlayerInputController PlayerInput => field ??= new(Player, 16);

    public override void Init()
    {
        base.Init();

        World.Add(Gravity);

        AddGameObject(Player);
        AddGameObject(Puck);

        World.Add(PlayerInput);
    }

    public override void AddGameObject(GameObjectBase obj)
    {
        if (obj is not AetherGameObjectBase aetherObj) throw new ArgumentException("Not a valid object for this game", nameof(obj));
        base.AddGameObject(aetherObj);
        Gravity.AddBody(aetherObj.Body);
    }
}
