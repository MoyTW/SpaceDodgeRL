using SpaceDodgeRL.scenes.entities;

namespace SpaceDodgeRL.scenes.components {

  public class AttackerComponent : Component {
    public static readonly string ENTITY_GROUP = "ATTACKER_COMPONENT_GROUP";
    public string EntityGroup => ENTITY_GROUP;

    public Entity Source { get; private set; }
    public int Power { get; private set; }

    public static AttackerComponent Create(Entity source, int power) {
      var component = new AttackerComponent();

      component.Source = source;
      component.Power = power;

      return component;
    }
  }
}