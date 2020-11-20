using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Godot;
using SpaceDodgeRL.library.encounter;
using SpaceDodgeRL.resources.gamedata;
using SpaceDodgeRL.scenes.entities;

namespace SpaceDodgeRL.scenes.components {

  public class PlayerComponent : Component, Savable {
    public static readonly string ENTITY_GROUP = "PLAYER_COMPONENT_GROUP";
    public string EntityGroup => ENTITY_GROUP;

    // StatusEffectTracker
    private Entity _parent;
    private StatusEffectTrackerComponent _statusEffectTrackerComponent;
    private StatusEffectTrackerComponent _TrackerComponent { get {
      if (_statusEffectTrackerComponent == null) {
        var tracker = _parent.GetComponent<StatusEffectTrackerComponent>();
        if (tracker == null) {
          throw new NotImplementedException("Can't get status effects for PlayerComponent: not attached to Entity");
        }
        _statusEffectTrackerComponent = tracker;
      }
      return _statusEffectTrackerComponent;
    } }

    [JsonInclude] public HashSet<int> DungeonLevelsWithIntel { get; private set; }

    // Right now the player is a special case in that they're the only entity with variable-power weaponry!
    [JsonInclude] public int CuttingLaserRange { get; private set; }
    [JsonInclude] public int BaseCuttingLaserPower;
    [JsonIgnore] public int CuttingLaserPower { get => BaseCuttingLaserPower + _TrackerComponent.GetTotalBoost(StatusEffectType.BOOST_POWER); }

    // Autopilot data
    public bool IsAutopiloting { get => this.AutopilotPath != null; }
    [JsonInclude] public EncounterPath AutopilotPath { get; private set; }

    public static PlayerComponent Create(
      int baseCuttingLaserPower = 26,
      int cuttingLaserRange = 3
    ) {
      var component = new PlayerComponent();

      component.BaseCuttingLaserPower = baseCuttingLaserPower;
      component.CuttingLaserRange = cuttingLaserRange;
      component.AutopilotPath = null;
      component.DungeonLevelsWithIntel = new HashSet<int>() { DungeonLevel.ONE };

      return component;
    }

    public static PlayerComponent Create(string saveData) {
      return JsonSerializer.Deserialize<PlayerComponent>(saveData);
    }

    public void AddBaseCuttingLaserPower(int power) {
      this.BaseCuttingLaserPower += power;
    }

    public void RegisterIntel(int dungeonLevel) {
      DungeonLevelsWithIntel.Add(dungeonLevel);
    }

    public bool KnowsIntel(int dungeonLevel) {
      return DungeonLevelsWithIntel.Contains(dungeonLevel);
    }

    public void LayInAutopilotPath(EncounterPath path) {
      this.AutopilotPath = path;
    }

    public void StopAutopiloting() {
      this.AutopilotPath = null;
    }

    public string Save() {
      return JsonSerializer.Serialize(this);
    }

    public void NotifyAttached(Entity parent) {
      this._parent = parent;
    }

    public void NotifyDetached(Entity parent) {
      this._parent = null;
    }
  }
}