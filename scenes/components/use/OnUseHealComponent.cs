using Godot;

namespace SpaceDodgeRL.scenes.components.use {

  public class OnUseHealComponent: Component {
    private static PackedScene _componentPrefab = GD.Load<PackedScene>("res://scenes/components/OnUseHealComponent.tscn");

    public static readonly string ENTITY_GROUP = "USABLE_COMPONENT_GROUP";
    public override string EntityGroup => ENTITY_GROUP;

    public int Healpower { get; private set; }

    public static OnUseHealComponent Create(int healpower) {
      var component = _componentPrefab.Instance() as OnUseHealComponent;

      component.Healpower = healpower;

      return component;
    }
  }
}