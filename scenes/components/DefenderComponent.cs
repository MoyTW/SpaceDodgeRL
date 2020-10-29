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
    public bool ShouldLogDamage { get; private set; }
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
      component.ShouldLogDamage = logDamage;
      component.IsInvincible = isInvincible;

      return component;
    }

    /**
     * Returns the number of HP restored.
     */
    public int RestoreHP(int hp, bool overheal=false) {
      int startingHp = this.CurrentHp;

      this.CurrentHp += hp;
      if (!overheal && this.CurrentHp > this.MaxHp) {
        this.CurrentHp = this.MaxHp;
      }

      return this.CurrentHp - startingHp;
    }

    /**
     * Directly removes HP, without any checks
     */
    public void RemoveHp(int hp) {
      this.CurrentHp -= hp;
    }
  }
}