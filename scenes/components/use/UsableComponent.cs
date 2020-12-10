using System.Text.Json;
using System.Text.Json.Serialization;
using SpaceDodgeRL.scenes.entities;

namespace SpaceDodgeRL.scenes.components.use {

  public class UsableComponent : Component {
    public static readonly string ENTITY_GROUP = "USABLE_COMPONENT_GROUP";
    public string EntityGroup => ENTITY_GROUP;

    [JsonInclude] public bool UseOnGet { get; private set; }

    public static UsableComponent Create(bool useOnGet) {
      var component = new UsableComponent();
      component.UseOnGet = useOnGet;
      return component;
    }

    public static UsableComponent Create(string saveData) {
      return JsonSerializer.Deserialize<UsableComponent>(saveData);
    }

    public string Save() {
      return JsonSerializer.Serialize(this);
    }

    public void NotifyAttached(Entity parent) { }

    public void NotifyDetached(Entity parent) { }
  }
}