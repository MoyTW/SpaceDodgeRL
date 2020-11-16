using Godot;
using System;
using System.Linq;

namespace SpaceDodgeRL.scenes.components {

  public class SpeedComponent : Component {
    public static readonly string ENTITY_GROUP = "SPEED_COMPONENT_GROUP";
    public string EntityGroup => ENTITY_GROUP;

    public int BaseSpeed { get; private set; }

    public static SpeedComponent Create(int baseSpeed) {
      var component = new SpeedComponent();

      component.BaseSpeed = baseSpeed;

      return component;
    }

    public int CalculateSpeed(StatusEffectTrackerComponent statusEffectTrackerComponent) {
      int totalBoost = 0;
      if (statusEffectTrackerComponent != null) {
        totalBoost = statusEffectTrackerComponent.GetTotalBoost(StatusEffectType.BOOST_SPEED);
      }

      if (this.BaseSpeed - totalBoost <= 0) {
        return 0;
      } else {
        return this.BaseSpeed - totalBoost;
      }
    }
  }
}