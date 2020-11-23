using System.Text.Json;
using System.Text.Json.Serialization;
using SpaceDodgeRL.scenes.entities;

namespace SpaceDodgeRL.scenes.components.use {

  public class UseEffectBoostSpeedComponent: Component, Savable {
    public static readonly string ENTITY_GROUP = "USE_EFFECT_BOOST_SPEED_COMPONENT_GROUP";
    public string EntityGroup => ENTITY_GROUP;

    [JsonInclude] public int BoostPower { get; private set; }
    [JsonInclude] public int Duration { get; private set; }

    public static UseEffectBoostSpeedComponent Create(int boostPower, int duration) {
      var component = new UseEffectBoostSpeedComponent();

      component.BoostPower = boostPower;
      component.Duration = duration;

      return component;
    }

    public static UseEffectBoostSpeedComponent Create(string saveData) {
      return JsonSerializer.Deserialize<UseEffectBoostSpeedComponent>(saveData);
    }

    public string Save() {
      return JsonSerializer.Serialize(this);
    }

    public void NotifyAttached(Entity parent) { }

    public void NotifyDetached(Entity parent) { }
  }
}