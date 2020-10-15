using Godot;

namespace SpaceDodgeRL.scenes.components {

  public class DefenderComponent : Component {
    private static PackedScene _componentPrefab = GD.Load<PackedScene>("res://scenes/components/DefenderComponent.tscn");

    public static readonly string ENTITY_GROUP = "DEFENDER_COMPONENT_GROUP";
    public override string EntityGroup => ENTITY_GROUP;

    public int BaseDefense { get; private set; }
    public int MaxHp { get; private set; }
    public int CurrentHp { get; private set; }

    public static DefenderComponent Create(int baseDefense, int maxHP, int currentHP = int.MinValue) {
      var component = _componentPrefab.Instance() as DefenderComponent;

      component.BaseDefense = baseDefense;
      component.MaxHp = maxHP;
      if (currentHP == int.MinValue) {
        component.CurrentHp = component.MaxHp;
      } else {
        component.CurrentHp = currentHP;
      }

      return component;
    }
  }
}