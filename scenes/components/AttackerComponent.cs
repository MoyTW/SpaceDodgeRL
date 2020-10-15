using Godot;

namespace SpaceDodgeRL.scenes.components {

  public class AttackerComponent : Component {
    private static PackedScene _componentPrefab = GD.Load<PackedScene>("res://scenes/components/AttackerComponent.tscn");

    public static readonly string ENTITY_GROUP = "ATTACKER_COMPONENT_GROUP";
    public override string EntityGroup => ENTITY_GROUP;

    public int Power { get; private set; }

    public static AttackerComponent Create(int power) {
      var component = _componentPrefab.Instance() as AttackerComponent;
      component.Power = power;
      return component;
    }
  }
}