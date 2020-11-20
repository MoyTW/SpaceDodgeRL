using Godot;
using SpaceDodgeRL.scenes.entities;
using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SpaceDodgeRL.scenes.components {

  public class SpeedComponent : Component, Savable {
    public static readonly string ENTITY_GROUP = "SPEED_COMPONENT_GROUP";
    public string EntityGroup => ENTITY_GROUP;

    private Entity _parent;

    [JsonInclude] public int BaseSpeed { get; private set; }
    [JsonIgnore] public int Speed { get {
      int totalBoost = 0;
      var tracker = _parent.GetComponent<StatusEffectTrackerComponent>();
      if (tracker != null) {
        totalBoost = tracker.GetTotalBoost(StatusEffectType.BOOST_SPEED);
      }

      if (this.BaseSpeed - totalBoost <= 0) {
        return 0;
      } else {
        return this.BaseSpeed - totalBoost;
      }
    } }

    public static SpeedComponent Create(int baseSpeed) {
      var component = new SpeedComponent();

      component.BaseSpeed = baseSpeed;

      return component;
    }

    public static SpeedComponent Create(string saveData) {
      return JsonSerializer.Deserialize<SpeedComponent>(saveData);
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