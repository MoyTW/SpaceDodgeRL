using Godot;
using System;
using System.Linq;

namespace SpaceDodgeRL.scenes.components {

  public class SpeedComponent : Component {
    public static readonly string ENTITY_GROUP = "SPEED_COMPONENT_GROUP";
    public string EntityGroup => ENTITY_GROUP;

    private StatusEffectTrackerComponent _statusEffectTrackerComponent;
    public int BaseSpeed { get; private set; }

    public static SpeedComponent Create(StatusEffectTrackerComponent statusEffectTrackerComponent, int baseSpeed) {
      var component = new SpeedComponent();

      component._statusEffectTrackerComponent = statusEffectTrackerComponent;
      component.BaseSpeed = baseSpeed;

      return component;
    }

    public int CalculateSpeed() {
      int totalBoost = 0;
      if (this._statusEffectTrackerComponent != null) {
        totalBoost = this._statusEffectTrackerComponent.GetTotalBoost(StatusEffectType.BOOST_SPEED);
      }

      if (this.BaseSpeed - totalBoost <= 0) {
        return 0;
      } else {
        return this.BaseSpeed - totalBoost;
      }
    }
  }
}