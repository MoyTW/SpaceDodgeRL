using System.Text.Json;
using System.Text.Json.Serialization;
using SpaceDodgeRL.scenes.entities;

namespace SpaceDodgeRL.scenes.components.use {

  public class UseEffectBoostPowerComponent: Component, Savable {
    public static readonly string ENTITY_GROUP = "USE_EFFECT_BOOST_POWER_COMPONENT_GROUP";
    public string EntityGroup => ENTITY_GROUP;

    // Rename "power" (as in, attack power) to something that's less generic
    [JsonInclude] public int BoostPower { get; private set; }
    [JsonInclude] public int Duration { get; private set; }

    public static UseEffectBoostPowerComponent Create(int boostPower, int duration) {
      var component = new UseEffectBoostPowerComponent();

      component.BoostPower = boostPower;
      component.Duration = duration;

      return component;
    }

    public static UseEffectBoostPowerComponent Create(string saveData) {
      return JsonSerializer.Deserialize<UseEffectBoostPowerComponent>(saveData);
    }

    public string Save() {
      return JsonSerializer.Serialize(this);
    }

    public void NotifyAttached(Entity parent) { }

    public void NotifyDetached(Entity parent) { }
  }
}