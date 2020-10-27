using Godot;
using SpaceDodgeRL.library.encounter;

namespace SpaceDodgeRL.scenes.components {

  public class PlayerComponent : Component {
    private static PackedScene _componentPrefab = GD.Load<PackedScene>("res://scenes/components/PlayerComponent.tscn");

    public static readonly string ENTITY_GROUP = "PLAYER_COMPONENT_GROUP";
    public override string EntityGroup => ENTITY_GROUP;

    // Right now the player is a special case in that they're the only entity with variable-power weaponry!
    public int CuttingLaserRange { get; private set; }
    public int CuttingLaserPower { get; private set; }

    // Autopilot data
    public bool IsAutopiloting { get => this.AutopilotPath != null; }
    public EncounterPath AutopilotPath { get; private set; }

    public static PlayerComponent Create(int cuttingLaserRange = 3, int cuttingLaserPower = 26, EncounterPath autopilotPath = null) {
      var component = _componentPrefab.Instance() as PlayerComponent;

      component.CuttingLaserRange = cuttingLaserRange;
      component.CuttingLaserPower = cuttingLaserPower;
      component.AutopilotPath = autopilotPath;

      return component;
    }

    public void LayInAutopilotPath(EncounterPath path) {
      this.AutopilotPath = path;
    }

    public void StopAutopiloting() {
      this.AutopilotPath = null;
    }
  }
}