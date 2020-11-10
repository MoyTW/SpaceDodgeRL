using Godot;

namespace SpaceDodgeRL.scenes.components.use {

  public class UsableComponent: Component {
    public static readonly string ENTITY_GROUP = "USABLE_COMPONENT_GROUP";
    public string EntityGroup => ENTITY_GROUP;

    public static UsableComponent Create() {
      var component = new UsableComponent();
      return component;
    }
  }
}