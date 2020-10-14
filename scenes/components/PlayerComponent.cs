using Godot;

namespace SpaceDodgeRL.scenes.components {

  public class PlayerComponent : Node, Component {
    public static string ENTITY_GROUP = "PLAYER_COMPONENT_GROUP";
    public string EntityGroup => ENTITY_GROUP;
  }
}