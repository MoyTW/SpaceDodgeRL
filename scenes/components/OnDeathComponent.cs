using Godot;
using SpaceDodgeRL.scenes.entities;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SpaceDodgeRL.scenes.components {

  public static class OnDeathEffectType {
    public static string PLAYER_VICTORY = "ON_DEATH_EFFECT_TYPE_PLAYER_VICTORY";
    public static string PLAYER_DEFEAT = "ON_DEATH_EFFECT_TYPE_PLAYER_DEFEAT";
  }

  public class OnDeathComponent : Component {
    public static readonly string ENTITY_GROUP = "ON_DEATH_COMPONENT_GROUP";
    public string EntityGroup => ENTITY_GROUP;

    [JsonInclude] public List<string> ActiveEffectTypes { get; private set; } = new List<string>();

    public static OnDeathComponent Create(List<string> activeEffectTypes) {
      var component = new OnDeathComponent();

      component.ActiveEffectTypes = activeEffectTypes;

      return component;
    }

    public static OnDeathComponent Create(string saveData) {
      return JsonSerializer.Deserialize<OnDeathComponent>(saveData);
    }

    public string Save() {
      return JsonSerializer.Serialize(this);
    }

    public void NotifyAttached(Entity parent) { }

    public void NotifyDetached(Entity parent) { }
  }
}