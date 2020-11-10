using Godot;

namespace SpaceDodgeRL.scenes.components {

  public class AttackerComponent : Component {
    public static readonly string ENTITY_GROUP = "ATTACKER_COMPONENT_GROUP";
    public string EntityGroup => ENTITY_GROUP;

    public int Power { get; private set; }

    public static AttackerComponent Create(int power) {
      var component = new AttackerComponent();
      component.Power = power;
      return component;
    }
  }
}