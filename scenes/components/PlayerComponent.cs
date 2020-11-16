using System.Collections.Generic;
using Godot;
using SpaceDodgeRL.library.encounter;
using SpaceDodgeRL.resources.gamedata;

namespace SpaceDodgeRL.scenes.components {

  public class PlayerComponent : Component {
    public static readonly string ENTITY_GROUP = "PLAYER_COMPONENT_GROUP";
    public string EntityGroup => ENTITY_GROUP;

    private HashSet<int> _dungeonLevelsWithIntel;

    // Right now the player is a special case in that they're the only entity with variable-power weaponry!
    public int CuttingLaserRange { get; private set; }
    public int CuttingLaserPower { get; private set; }

    // Autopilot data
    public bool IsAutopiloting { get => this.AutopilotPath != null; }
    public EncounterPath AutopilotPath { get; private set; }

    public static PlayerComponent Create(
      int cuttingLaserRange = 3,
      int cuttingLaserPower = 26,
      EncounterPath autopilotPath = null,
      HashSet<int> dungeonLevelsWithIntel = null
    ) {
      var component = new PlayerComponent();

      component.CuttingLaserRange = cuttingLaserRange;
      component.CuttingLaserPower = cuttingLaserPower;
      component.AutopilotPath = autopilotPath;
      component._dungeonLevelsWithIntel = dungeonLevelsWithIntel != null ?
        dungeonLevelsWithIntel :
        new HashSet<int>() { DungeonLevel.ONE };

      return component;
    }

    public void AddBaseCuttingLaserPower(int power) {
      CuttingLaserPower += power;
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