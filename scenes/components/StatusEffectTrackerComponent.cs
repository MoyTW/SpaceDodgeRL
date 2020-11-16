using System.Collections.Generic;
using System.Linq;

namespace SpaceDodgeRL.scenes.components {

  public static class StatusEffectType {
    public static string BOOST_POWER = "STATUS_EFFECT_BOOST_POWER";
    public static string BOOST_SPEED = "STATUS_EFFECT_BOOST_SPEED";
  }

  public interface StatusEffect {
    string Type { get; }
  }

  public interface StatusEffectBoostStat {
    int BoostPower { get; }
  }

  public interface StatusEffectTimed {
    int StartTick { get; }
    int EndTick { get; }
  }

  // TODO: Consider a builder or something so we don't need to enumerate every permutation of status effect?
  // TODO: Deconflict term "power"
  public class StatusEffectTimedPowerBoost : StatusEffect, StatusEffectTimed, StatusEffectBoostStat {
    public string Type { get; }
    public int BoostPower { get; }
    public int StartTick { get; }
    public int EndTick { get; }

    public StatusEffectTimedPowerBoost(int boostPower, int startTick, int endTick) {
      this.Type = StatusEffectType.BOOST_POWER;
      this.BoostPower = boostPower;
      this.StartTick = startTick;
      this.EndTick = endTick;
    }
  }

  public class StatusEffectTimedSpeedBoost : StatusEffect, StatusEffectTimed, StatusEffectBoostStat {
    public string Type { get; }
    public int BoostPower { get; }
    public int StartTick { get; }
    public int EndTick { get; }

    public StatusEffectTimedSpeedBoost(int boostPower, int startTick, int endTick) {
      this.Type = StatusEffectType.BOOST_SPEED;
      this.BoostPower = boostPower;
      this.StartTick = startTick;
      this.EndTick = endTick;
    }
  }

  public class StatusEffectTrackerComponent : Component {
    public static readonly string ENTITY_GROUP = "STATUS_EFFECT_TRACKER_COMPONENT_GROUP";
    public string EntityGroup => ENTITY_GROUP;

    private List<StatusEffect> _statusEffects = new List<StatusEffect>();

    public static StatusEffectTrackerComponent Create() {
      var component = new StatusEffectTrackerComponent();
      return component;
    }

    public void AddEffect(StatusEffect effect) {
      this._statusEffects.Add(effect);
    }

    public void RemoveEffect(StatusEffect effect) {
      this._statusEffects.Remove(effect);
    }

    public void UpdateStatusEffectTimers(int currentTick) {
      var expiredEffects = this._statusEffects
        .Where(effect => effect is StatusEffectTimed && ((StatusEffectTimed)effect).EndTick < currentTick)
        .ToArray();
      foreach(StatusEffect expired in expiredEffects) {
        this.RemoveEffect(expired);
      }
    }

    public IEnumerable<StatusEffect> GetStatusEffectsOfType(string statusEffectType) {
      return this._statusEffects.Where(effect => effect.Type == statusEffectType);
    }
  }
}