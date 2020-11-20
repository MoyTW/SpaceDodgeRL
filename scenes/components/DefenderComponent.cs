using System.Text.Json;
using System.Text.Json.Serialization;
using Godot;
using SpaceDodgeRL.scenes.entities;

namespace SpaceDodgeRL.scenes.components {

  public class DefenderComponent : Component, Savable {
    public static readonly string ENTITY_GROUP = "DEFENDER_COMPONENT_GROUP";
    public string EntityGroup => ENTITY_GROUP;

    [JsonInclude] public int BaseDefense { get; private set; }
    // Right now we don't do defense buffs, but we could later!
    public int Defense { get => this.BaseDefense; }
    [JsonInclude] public int MaxHp { get; private set; }
    [JsonInclude] public int CurrentHp { get; private set; }
    [JsonInclude] public bool ShouldLogDamage { get; private set; }
    [JsonInclude] public bool IsInvincible { get; private set; }

    public static DefenderComponent Create(int baseDefense, int maxHp, int currentHp = int.MinValue, bool logDamage = true, bool isInvincible = false) {
      var component = new DefenderComponent();

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

    public static DefenderComponent Create(string saveData) {
      return JsonSerializer.Deserialize<DefenderComponent>(saveData);
    }

    public void AddBaseMaxHp(int hp, bool alsoAddCurrentHp = true) {
      this.MaxHp += hp;
      if (alsoAddCurrentHp) {
        this.CurrentHp += hp;
      }
    }

    /**
     * Returns the number of HP restored.
     */
    public int RestoreHP(int hp, bool overheal = false) {
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

    public string Save() {
      return JsonSerializer.Serialize(this);
    }

    public void NotifyAttached(Entity parent) { }

    public void NotifyDetached(Entity parent) { }
  }
}