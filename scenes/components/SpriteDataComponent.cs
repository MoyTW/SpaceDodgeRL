using System;
using Godot;

namespace SpaceDodgeRL.scenes.components {

  public class SpriteDataComponent : Node, Component {
    private static PackedScene _componentPrefab = GD.Load<PackedScene>("res://scenes/components/SpriteDataComponent.tscn");

    public static readonly string ENTITY_GROUP = "SPRITE_DATA_COMPONENT_GROUP";
    public string EntityGroup => ENTITY_GROUP;

    public string TexturePath { get; private set; }
    public Texture Texture { get; private set; }

    public static SpriteDataComponent Create(string texturePath) {
      var component = _componentPrefab.Instance() as SpriteDataComponent;

      component.TexturePath = texturePath;
      component.Texture = GD.Load<Texture>(texturePath);

      return component;
    }

    public override void _Ready() {
      if (TexturePath == null || Texture == null) {
        throw new NotImplementedException();
      }
    }
  }
}