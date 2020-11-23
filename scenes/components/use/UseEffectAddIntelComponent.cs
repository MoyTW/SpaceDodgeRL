using System.Text.Json;
using System.Text.Json.Serialization;
using SpaceDodgeRL.scenes.entities;

namespace SpaceDodgeRL.scenes.components.use {

  public class UseEffectAddIntelComponent : Component, Savable {
    public static readonly string ENTITY_GROUP = "USE_EFFECT_ADD_INTEL_COMPONENT_GROUP";
    public string EntityGroup => ENTITY_GROUP;

    [JsonInclude] public int TargetDungeonLevel { get; private set; }

    public static UseEffectAddIntelComponent Create(int targetDungeonLevel) {
      var component = new UseEffectAddIntelComponent();

      component.TargetDungeonLevel = targetDungeonLevel;

      return component;
    }
    
    public static UseEffectAddIntelComponent Create(string saveData) {
      return JsonSerializer.Deserialize<UseEffectAddIntelComponent>(saveData);
    }

    public string Save() {
      return JsonSerializer.Serialize(this);
    }

    public void NotifyAttached(Entity parent) { }

    public void NotifyDetached(Entity parent) { }
  }
}