using Godot;

namespace SpaceDodgeRL.scenes.components.use {

  public class UseEffectHealComponent: Component {
    private static PackedScene _componentPrefab = GD.Load<PackedScene>("res://scenes/components/use/UseEffectHealComponent.tscn");

    public static readonly string ENTITY_GROUP = "USE_EFFECT_HEAL_COMPONENT_GROUP";
    public override string EntityGroup => ENTITY_GROUP;

    public int Healpower { get; private set; }

    public static UseEffectHealComponent Create(int healpower) {
      var component = _componentPrefab.Instance() as UseEffectHealComponent;

      component.Healpower = healpower;

      return component;
    }
  }
}