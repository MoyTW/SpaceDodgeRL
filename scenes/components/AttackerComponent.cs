using System.Text.Json;
using System.Text.Json.Serialization;
using SpaceDodgeRL.scenes.entities;

namespace SpaceDodgeRL.scenes.components {

  public class AttackerComponent : Component, Savable {
    public static readonly string ENTITY_GROUP = "ATTACKER_COMPONENT_GROUP";
    public string EntityGroup => ENTITY_GROUP;

    [JsonInclude] public string SourceEntityId { get; private set; }
    [JsonInclude] public int Power { get; private set; }

    public static AttackerComponent Create(string sourceEntityId, int power) {
      var component = new AttackerComponent();

      component.SourceEntityId = sourceEntityId;
      component.Power = power;

      return component;
    }

    public static AttackerComponent Create(string saveData) {
      return JsonSerializer.Deserialize<AttackerComponent>(saveData);
    }

    public string Save() {
      return JsonSerializer.Serialize(this);
    }

    public void NotifyAttached(Entity parent) { }

    public void NotifyDetached(Entity parent) { }
  }
}