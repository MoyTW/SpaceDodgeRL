using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Godot;
using SpaceDodgeRL.scenes.entities;

namespace SpaceDodgeRL.scenes.components {

  public class DisplayComponent : Component {
    public static readonly string ENTITY_GROUP = "SPRITE_DATA_COMPONENT_GROUP";
    public string EntityGroup => ENTITY_GROUP;

    [JsonInclude] public string TexturePath { get; private set; }
    [JsonInclude] public string Description { get; private set; }
    [JsonInclude] public bool VisibleInFoW { get; private set; }
    [JsonInclude] public int ZIndex { get; private set; }

    public static DisplayComponent Create(string texturePath, string description, bool visibleInFoW, int zIndex) {
      var component = new DisplayComponent();

      component.TexturePath = texturePath;
      component.Description = description;
      component.VisibleInFoW = visibleInFoW;
      component.ZIndex = zIndex;

      return component;
    }

    public static DisplayComponent Create(string saveData) {
      return JsonSerializer.Deserialize<DisplayComponent>(saveData);
    }

    public string Save() {
      return JsonSerializer.Serialize(this);
    }

    public void NotifyAttached(Entity parent) { }

    public void NotifyDetached(Entity parent) { }
  }
}