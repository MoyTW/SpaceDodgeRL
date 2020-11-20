using Godot;
using SpaceDodgeRL.scenes.entities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SpaceDodgeRL.scenes.components {

  public class ActionTimeComponent : Component, Savable {
    public static readonly string ENTITY_GROUP = "ACTION_TIME_COMPONENT_GROUP";
    public string EntityGroup => ENTITY_GROUP;

    [JsonInclude] public int NextTurnAtTick { get; private set; }
    [JsonInclude] public int LastTurnAtTick { get; private set; }

    public static ActionTimeComponent Create(int currentTick, int ticksUntilTurn = 0) {
      var component = new ActionTimeComponent();

      component.NextTurnAtTick = currentTick + ticksUntilTurn;
      component.LastTurnAtTick = int.MinValue;

      return component;
    }

    public static ActionTimeComponent Create(string saveData) {
      return JsonSerializer.Deserialize<ActionTimeComponent>(saveData);
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
      this.NextTurnAtTick = currentTick + speedComponent.Speed;
    }

    public string Save() {
      return JsonSerializer.Serialize(this);
    }

    public void NotifyAttached(Entity parent) { }

    public void NotifyDetached(Entity parent) { }
  }
}