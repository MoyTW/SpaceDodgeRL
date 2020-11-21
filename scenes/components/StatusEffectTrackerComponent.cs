using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using SpaceDodgeRL.scenes.entities;

namespace SpaceDodgeRL.scenes.components {

  public static class StatusEffectType {
    public static string BOOST_POWER = "STATUS_EFFECT_BOOST_POWER";
    public static string BOOST_SPEED = "STATUS_EFFECT_BOOST_SPEED";
  }

  [JsonConverter(typeof(StatusEffectConverter))]
  public class StatusEffect {
    [JsonInclude] public string Type { get; private set; }

    public StatusEffect(string type) {
      this.Type = type;
    }
  }

  public class StatusEffectConverter : JsonConverter<StatusEffect> {

    public override StatusEffect Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
      string type = "nosuch";
      int boostPower = int.MinValue;
      int startTick = int.MinValue;
      int endTick = int.MinValue;

      while (reader.Read() && reader.TokenType != JsonTokenType.EndObject) {
        string property = reader.GetString();
        reader.Read();
        switch (property) {
          case "Type":
            type = reader.GetString();
            break;
          case "BoostPower":
            boostPower = reader.GetInt16();
            break;
          case "StartTick":
            startTick = reader.GetInt16();
            break;
          case "EndTick":
            endTick = reader.GetInt16();
            break;
        }
      }

      if (type == StatusEffectType.BOOST_POWER) {
        return new StatusEffectTimedPowerBoost(boostPower: boostPower, startTick: startTick, endTick: endTick);
      } else if (type == StatusEffectType.BOOST_SPEED) {
        return new StatusEffectTimedSpeedBoost(boostPower: boostPower, startTick: startTick, endTick: endTick);
      } else {
        throw new NotImplementedException("mysterious type");
      }
    }

    public override void Write(Utf8JsonWriter writer, StatusEffect value, JsonSerializerOptions options) {
      // JsonSerializer.Serialize<object>(writer, value as object, options);
      if (value.Type == StatusEffectType.BOOST_POWER) {
        JsonSerializer.Serialize<StatusEffectTimedPowerBoost>(writer, value as StatusEffectTimedPowerBoost);
      } else if (value.Type == StatusEffectType.BOOST_SPEED) {
        JsonSerializer.Serialize<StatusEffectTimedSpeedBoost>(writer, value as StatusEffectTimedSpeedBoost);
      }
    }
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
    public int BoostPower { get; }
    public int StartTick { get; }
    public int EndTick { get; }

    public StatusEffectTimedPowerBoost(int boostPower, int startTick, int endTick) : base(StatusEffectType.BOOST_POWER) {
      this.BoostPower = boostPower;
      this.StartTick = startTick;
      this.EndTick = endTick;
    }
  }

  public class StatusEffectTimedSpeedBoost : StatusEffect, StatusEffectTimed, StatusEffectBoostStat {
    public int BoostPower { get; }
    public int StartTick { get; }
    public int EndTick { get; }

    public StatusEffectTimedSpeedBoost(int boostPower, int startTick, int endTick) : base(StatusEffectType.BOOST_SPEED) {
      this.BoostPower = boostPower;
      this.StartTick = startTick;
      this.EndTick = endTick;
    }
  }

  public class StatusEffectTrackerComponent : Component, Savable {
    public static readonly string ENTITY_GROUP = "STATUS_EFFECT_TRACKER_COMPONENT_GROUP";
    public string EntityGroup => ENTITY_GROUP;

    [JsonInclude] public List<StatusEffect> _StatusEffects { get; set; } = new List<StatusEffect>();

    public static StatusEffectTrackerComponent Create() {
      var component = new StatusEffectTrackerComponent();
      return component;
    }

    public static StatusEffectTrackerComponent Create(string saveData) {
      return JsonSerializer.Deserialize<StatusEffectTrackerComponent>(saveData);
    }

    public void AddEffect(StatusEffect effect) {
      this._StatusEffects.Add(effect);
    }

    public void RemoveEffect(StatusEffect effect) {
      this._StatusEffects.Remove(effect);
    }

    public void UpdateStatusEffectTimers(int currentTick) {
      var expiredEffects = this._StatusEffects
        .Where(effect => effect is StatusEffectTimed && ((StatusEffectTimed)effect).EndTick < currentTick)
        .ToArray();
      foreach(StatusEffect expired in expiredEffects) {
        this.RemoveEffect(expired);
      }
    }

    public IEnumerable<StatusEffect> GetStatusEffectsOfType(string statusEffectType) {
      return this._StatusEffects.Where(effect => effect.Type == statusEffectType);
    }

    public int GetTotalBoost(string statusEffectType) {
      return GetStatusEffectsOfType(statusEffectType).Sum(e => ((StatusEffectBoostStat)e).BoostPower);
    }

    public string Save() {
      return JsonSerializer.Serialize(this);
    }

    public void NotifyAttached(Entity parent) { }

    public void NotifyDetached(Entity parent) { }
  }
}