using System.Collections.Generic;
using Godot;
using SpaceDodgeRL.library.encounter;
using SpaceDodgeRL.resources.gamedata;

namespace SpaceDodgeRL.scenes.components {

  public class PlayerComponent : Component {
    public static readonly string ENTITY_GROUP = "PLAYER_COMPONENT_GROUP";
    public string EntityGroup => ENTITY_GROUP;

    private StatusEffectTrackerComponent statusEffectTrackerComponent;
    private HashSet<int> _dungeonLevelsWithIntel;

    // Right now the player is a special case in that they're the only entity with variable-power weaponry!
    public int CuttingLaserRange { get; private set; }
    private int _cuttingLaserBasePower;
    public int CuttingLaserPower { get => _cuttingLaserBasePower + statusEffectTrackerComponent.GetTotalBoost(StatusEffectType.BOOST_POWER); }

    // Autopilot data
    public bool IsAutopiloting { get => this.AutopilotPath != null; }
    public EncounterPath AutopilotPath { get; private set; }

    public static PlayerComponent Create(
      StatusEffectTrackerComponent statusEffectTrackerComponent,
      int cuttingLaserBasePower = 26,
      int cuttingLaserRange = 3,
      EncounterPath autopilotPath = null,
      HashSet<int> dungeonLevelsWithIntel = null
    ) {
      var component = new PlayerComponent();

      component.statusEffectTrackerComponent = statusEffectTrackerComponent;
      component._cuttingLaserBasePower = cuttingLaserBasePower;
      component.CuttingLaserRange = cuttingLaserRange;
      component.AutopilotPath = autopilotPath;
      component._dungeonLevelsWithIntel = dungeonLevelsWithIntel != null ?
        dungeonLevelsWithIntel :
        new HashSet<int>() { DungeonLevel.ONE };

      return component;
    }

    public void AddBaseCuttingLaserPower(int power) {
      this._cuttingLaserBasePower += power;
    }

    public void RegisterIntel(int dungeonLevel) {
      _dungeonLevelsWithIntel.Add(dungeonLevel);
    }

    public bool KnowsIntel(int dungeonLevel) {
      return _dungeonLevelsWithIntel.Contains(dungeonLevel);
    }

    public void LayInAutopilotPath(EncounterPath path) {
      this.AutopilotPath = path;
    }

    public void StopAutopiloting() {
      this.AutopilotPath = null;
    }
  }
}