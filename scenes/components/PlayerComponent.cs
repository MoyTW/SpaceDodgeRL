using Godot;

namespace SpaceDodgeRL.scenes.components {

  public class PlayerComponent : Component {
    private static PackedScene _componentPrefab = GD.Load<PackedScene>("res://scenes/components/PlayerComponent.tscn");

    public static readonly string ENTITY_GROUP = "PLAYER_COMPONENT_GROUP";
    public override string EntityGroup => ENTITY_GROUP;

    // Right now the player is a special case in that they're the only entity with variable-power weaponry!
    public int Power { get; private set; }

    public static PlayerComponent Create(int power = 26) {
      var component = _componentPrefab.Instance() as PlayerComponent;
      component.Power = power;
      return component;
    }
  }
}