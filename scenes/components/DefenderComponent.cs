using Godot;

namespace SpaceDodgeRL.scenes.components {

  public class DefenderComponent : Component {
    private static PackedScene _componentPrefab = GD.Load<PackedScene>("res://scenes/components/DefenderComponent.tscn");

    public static readonly string ENTITY_GROUP = "DEFENDER_COMPONENT_GROUP";
    public override string EntityGroup => ENTITY_GROUP;

    public int BaseDefense { get; private set; }
    // Right now we don't do defense buffs, but we could later!
    public int Defense { get => this.BaseDefense; }
    public int MaxHp { get; private set; }
    public int CurrentHp { get; private set; }
    public bool LogDamage { get; private set; }
    public bool IsInvincible { get; private set; }

    public static DefenderComponent Create(int baseDefense, int maxHp, int currentHp = int.MinValue, bool logDamage = true, bool isInvincible = false) {
      var component = _componentPrefab.Instance() as DefenderComponent;

      component.BaseDefense = baseDefense;
      component.MaxHp = maxHp;
      if (currentHp == int.MinValue) {
        component.CurrentHp = component.MaxHp;
      } else {
        component.CurrentHp = currentHp;
      }
      component.LogDamage = logDamage;
      component.IsInvincible = isInvincible;

      return component;
    }

    public void RemoveHp(int hp) {
      this.CurrentHp -= hp;
    }
  }
}