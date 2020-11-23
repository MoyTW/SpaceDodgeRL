using System.Text.Json;
using System.Text.Json.Serialization;
using SpaceDodgeRL.scenes.entities;

namespace SpaceDodgeRL.scenes.components {

  class XPValueComponent: Component, Savable {
    public static readonly string ENTITY_GROUP = "XP_VALUE_COMPONENT_GROUP";
    public string EntityGroup => ENTITY_GROUP;

    [JsonInclude] public int XPValue { get; private set; }

    public static XPValueComponent Create(int xpValue) {
      var component = new XPValueComponent();

      component.XPValue = xpValue;

      return component;
    }

    public static XPValueComponent Create(string saveData) {
      return JsonSerializer.Deserialize<XPValueComponent>(saveData);
    }

    public string Save() {
      return JsonSerializer.Serialize(this);
    }

    public void NotifyAttached(Entity parent) { }

    public void NotifyDetached(Entity parent) { }
  }
}