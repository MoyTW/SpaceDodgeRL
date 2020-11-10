using System;
using Godot;

namespace SpaceDodgeRL.scenes.components {

  public class DisplayComponent : Component {
    public static readonly string ENTITY_GROUP = "SPRITE_DATA_COMPONENT_GROUP";
    public string EntityGroup => ENTITY_GROUP;

    public string TexturePath { get; private set; }
    public Texture Texture { get; private set; }
    public bool VisibleInFoW { get; private set; }

    public static DisplayComponent Create(string texturePath, bool visibleInFoW) {
      var component = new DisplayComponent();

      component.TexturePath = texturePath;
      component.Texture = GD.Load<Texture>(texturePath);
      component.VisibleInFoW = visibleInFoW;

      return component;
    }
  }
}