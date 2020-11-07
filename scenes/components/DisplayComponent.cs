using System;
using Godot;

namespace SpaceDodgeRL.scenes.components {

  public class DisplayComponent : Component {
    private static PackedScene _componentPrefab = GD.Load<PackedScene>("res://scenes/components/DisplayComponent.tscn");

    public static readonly string ENTITY_GROUP = "SPRITE_DATA_COMPONENT_GROUP";
    public override string EntityGroup => ENTITY_GROUP;

    public string TexturePath { get; private set; }
    public Texture Texture { get; private set; }
    public bool VisibleInFoW { get; private set; }

    public static DisplayComponent Create(string texturePath, bool visibleInFoW) {
      var component = _componentPrefab.Instance() as DisplayComponent;

      component.TexturePath = texturePath;
      component.Texture = GD.Load<Texture>(texturePath);
      component.VisibleInFoW = visibleInFoW;

      return component;
    }
  }
}