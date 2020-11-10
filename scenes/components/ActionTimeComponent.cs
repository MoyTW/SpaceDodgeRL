using Godot;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace SpaceDodgeRL.scenes.components {

  // TODO: Move all this status stuff outta here!
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
      GD.Print("Adding ", effect.Type);
      this._statusEffects.Add(effect);
    }

    public void RemoveEffect(StatusEffect effect) {
      this._statusEffects.Remove(effect);
    }

    public void UpdateStatusEffectTimers(int currentTick) {
      this._statusEffects.RemoveAll(effect => effect is StatusEffectTimed && ((StatusEffectTimed)effect).EndTick < currentTick);
    }

    public IEnumerable<StatusEffect> GetStatusEffectsOfType(string statusEffectType) {
      GD.Print("Getting ", statusEffectType);
      return this._statusEffects.Where(effect => effect.Type == statusEffectType);
    }
  }

  public class ActionTimeComponent : Component {
    public static readonly string ENTITY_GROUP = "ACTION_TIME_COMPONENT_GROUP";
    public string EntityGroup => ENTITY_GROUP;

    public int NextTurnAtTick { get; private set; }
    public int LastTurnAtTick { get; private set; }

    public static ActionTimeComponent Create(int currentTick, int ticksUntilTurn = 0) {
      var component = new ActionTimeComponent();

      component.NextTurnAtTick = currentTick + ticksUntilTurn;
      component.LastTurnAtTick = int.MinValue;

      return component;
    }

    // An ugly hack right now to get the player from Encounter A -> Encounter B
    public void SetNextTurnAtTo(int nextTurnAtTick) {
      this.NextTurnAtTick = nextTurnAtTick;
      this.LastTurnAtTick = int.MinValue;
    }

    public void EndTurn(SpeedComponent speedComponent, StatusEffectTrackerComponent statusEffectTrackerComponent) {
      int currentTick = this.NextTurnAtTick;

      if (statusEffectTrackerComponent != null) {
        statusEffectTrackerComponent.UpdateStatusEffectTimers(currentTick);
      }
      
      this.LastTurnAtTick = currentTick;
      this.NextTurnAtTick = currentTick + speedComponent.CalculateSpeed(statusEffectTrackerComponent);
    }
  }
}