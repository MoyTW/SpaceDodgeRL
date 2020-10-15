using Godot;

namespace SpaceDodgeRL.scenes.components {

  public class PlayerComponent : Component {
    private static PackedScene _componentPrefab = GD.Load<PackedScene>("res://scenes/components/PlayerComponent.tscn");

    public static readonly string ENTITY_GROUP = "PLAYER_COMPONENT_GROUP";
    public override string EntityGroup => ENTITY_GROUP;

    public static PlayerComponent Create() {
      return _componentPrefab.Instance() as PlayerComponent;
    }
  }
}