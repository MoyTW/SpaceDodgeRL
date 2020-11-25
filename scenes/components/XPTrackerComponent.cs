using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text.Json.Serialization;
using SpaceDodgeRL.scenes.entities;

namespace SpaceDodgeRL.scenes.components {

  public static class LevelUpBonus {
    public static string MAX_HP = "LEVEL_UP_BONUS_MAX_HP";
    public static int MAX_HP_BONUS = 15;

    public static string ATTACK_POWER = "LEVEL_UP_BONUS_ATTACK_POWER";
    public static int ATTACK_POWER_BONUS = 3;

    public static string REPAIR = "LEVEL_UP_BONUS_REPAIR";
    public static double REPAIR_PERCENTAGE = .5f;
  }

  /**
   * Tracks levels and level-up bonuses.
   */
  public class XPTrackerComponent : Component {
    public static readonly string ENTITY_GROUP = "XP_TRACKER_COMPONENT_GROUP";
    public string EntityGroup => ENTITY_GROUP;

    // Level curve variables
    [JsonInclude] public int LevelUpBase { get; private set; }
    [JsonInclude] public int LevelUpFactor { get; private set; }
    // Current state data
    [JsonInclude] public int XP { get; private set; }
    [JsonInclude] public int Level { get; private set; }
    [JsonInclude] public List<int> _UnusedLevelUps { get; private set; }
    public ReadOnlyCollection<int> UnusedLevelUps { get => _UnusedLevelUps.AsReadOnly(); }
    [JsonInclude] public int NextLevelAtXP { get => this.LevelUpBase + this.Level * this.LevelUpFactor; }
    [JsonInclude] public int XPToNextLevel { get => this.NextLevelAtXP - this.XP; }
    // Historical data
    [JsonInclude] public Dictionary<int, string> _ChosenLevelUps { get; private set; }
    public IReadOnlyDictionary<int, string> ChosenLevelUps { get => _ChosenLevelUps; }

    public static XPTrackerComponent Create(int levelUpBase, int levelUpFactor, int startingLevel = 1,
      List<int> unusedLevelUps = null, int startingXP = 0, Dictionary<int, string> chosenLevelUps = null
    ) {
      var component = new XPTrackerComponent();

      component.LevelUpBase = levelUpBase;
      component.LevelUpFactor = levelUpFactor;
      component.Level = startingLevel;
      component._UnusedLevelUps = unusedLevelUps != null ? unusedLevelUps : new List<int>();
      component.XP = startingXP;
      component._ChosenLevelUps = chosenLevelUps != null ? chosenLevelUps : new Dictionary<int, string>();

      return component;
    }

    public static XPTrackerComponent Create(string saveData) {
      return JsonSerializer.Deserialize<XPTrackerComponent>(saveData);
    }

    /**
     * Adds XP and returns true if the entity has levelled up.
     */
    public bool AddXP(int xp) {
      bool levelledUp = false;
      this.XP += xp;
      while (this.XP >= this.NextLevelAtXP) {
        this.XP -= this.NextLevelAtXP;
        this.Level += 1;
        this._UnusedLevelUps.Add(this.Level);
        levelledUp = true;
      }
      return levelledUp;
    }

    public void RegisterLevelUpChoice(Entity entity, string chosenLevelUp) {
      this._ChosenLevelUps.Add(this._UnusedLevelUps[0], chosenLevelUp);
      this._UnusedLevelUps.RemoveAt(0);

      if (chosenLevelUp == LevelUpBonus.MAX_HP) {
        // TODO: We can model persistent level-ups as status effects, can't we?
        entity.GetComponent<DefenderComponent>().AddBaseMaxHp(LevelUpBonus.MAX_HP_BONUS);
      } else if (chosenLevelUp == LevelUpBonus.ATTACK_POWER) {
        entity.GetComponent<PlayerComponent>().AddBaseCuttingLaserPower(LevelUpBonus.ATTACK_POWER_BONUS);
      } else if (chosenLevelUp == LevelUpBonus.REPAIR) {
        var defenderComponent = entity.GetComponent<DefenderComponent>();
        int repairValue = (int)((double)defenderComponent.MaxHp * LevelUpBonus.REPAIR_PERCENTAGE);
        defenderComponent.RestoreHP(repairValue);
      } else {
        throw new NotImplementedException("what is the level-up of " + chosenLevelUp);
      }
    }

    public string Save() {
      return JsonSerializer.Serialize(this);
    }

    public void NotifyAttached(Entity parent) { }

    public void NotifyDetached(Entity parent) { }
  }
}