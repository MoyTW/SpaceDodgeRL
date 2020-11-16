using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SpaceDodgeRL.scenes.components {

  public static class LevelUpBonus {
    public static string MAX_HP = "LEVEL_UP_BONUS_MAX_HP";
    public static string ATTACK_POWER = "LEVEL_UP_BONUS_ATTACK_POWER";
    public static string REPAIR = "LEVEL_UP_BONUS_REPAIR";
  }

  /**
   * Tracks levels and level-up bonuses. TODO: track levels, level-up bonuses
   */
  public class XPTrackerComponent : Component {
    public static readonly string ENTITY_GROUP = "XP_TRACKER_COMPONENT_GROUP";
    public string EntityGroup => ENTITY_GROUP;

    // Level curve variables
    public int LevelUpBase { get; private set; }
    public int LevelUpFactor { get; private set; }
    // Current state data
    public int XP { get; private set; }
    public int Level { get; private set; }
    private List<int> _unusedLevelUps;
    public ReadOnlyCollection<int> UnusedLevelUps { get => _unusedLevelUps.AsReadOnly(); }
    public int NextLevelAtXP { get => this.LevelUpBase + this.Level * this.LevelUpFactor; }
    public int XPToNextLevel { get => this.NextLevelAtXP - this.XP; }
    // Historical data
    private Dictionary<int, string> _chosenLevelUps;
    public IReadOnlyDictionary<int, string> ChosenLevelUps { get => _chosenLevelUps; }

    public static XPTrackerComponent Create(int levelUpBase, int levelUpFactor, int startingLevel = 1,
      List<int> unusedLevelUps = null, int startingXP = 0, Dictionary<int, string> chosenLevelUps = null
    ) {
      var component = new XPTrackerComponent();

      component.LevelUpBase = levelUpBase;
      component.LevelUpFactor = levelUpFactor;
      component.Level = startingLevel;
      component._unusedLevelUps = unusedLevelUps != null ? unusedLevelUps : new List<int>();
      component.XP = startingXP;
      component._chosenLevelUps = chosenLevelUps != null ? chosenLevelUps : new Dictionary<int, string>();

      return component;
    }

    // TODO: Display this to the user & make them pick their options!
    public void AddXP(int xp) {
      this.XP += xp;
      while (this.XP >= this.NextLevelAtXP) {
        this.XP -= this.NextLevelAtXP;
        this.Level += 1;
        this._unusedLevelUps.Add(this.Level);
        Godot.GD.Print("You levelled up!");
      }
      Godot.GD.Print("XP gained, XP total now at " + this.XP);
    }

    public void RegisterLevelUpChoice(string chosenLevelUp) {
      Godot.GD.Print("YOU REGISTERED A LEVEL-UP");
      this._chosenLevelUps.Add(this._unusedLevelUps[0], chosenLevelUp);
      this._unusedLevelUps.RemoveAt(0);
    }
  }
}