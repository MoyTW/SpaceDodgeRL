using System.Text.Json;
using Godot;
using SpaceDodgeRL.scenes.entities;

namespace SpaceDodgeRL.scenes.components.use {

  public class UsableComponent : Component, Savable {
    public static readonly string ENTITY_GROUP = "USABLE_COMPONENT_GROUP";
    public string EntityGroup => ENTITY_GROUP;

    public static UsableComponent Create() {
      var component = new UsableComponent();
      return component;
    }

    public static UsableComponent Create(string saveData) {
      return UsableComponent.Create();
    }

    public string Save() {
      return JsonSerializer.Serialize(this);
    }

    public void NotifyAttached(Entity parent) { }

    public void NotifyDetached(Entity parent) { }
  }
}