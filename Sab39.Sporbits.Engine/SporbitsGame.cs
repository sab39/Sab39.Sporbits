using Sab39.Sabric.Engine.Aether;

using nkast.Aether.Physics2D.Controllers;

namespace Sab39.Sporbits.Engine;

/// <summary>
/// The game itself: a player planet, a puck orbiting it, and the mutual gravity between them.
/// </summary>
public sealed class SporbitsGame : GameBase
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
        base.AddGameObject(obj);
        Gravity.AddBody(obj.Body);
    }
}
