using Godot;

namespace SpaceDodgeRL.scenes.components.use {

  public class UsableComponent: Component {
    private static PackedScene _componentPrefab = GD.Load<PackedScene>("res://scenes/components/use/UsableComponent.tscn");

    public static readonly string ENTITY_GROUP = "USABLE_COMPONENT_GROUP";
    public string EntityGroup => ENTITY_GROUP;

    public static UsableComponent Create() {
      var component = new UsableComponent();
      return component;
    }
  }
}