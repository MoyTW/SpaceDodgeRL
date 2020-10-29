using Godot;
using System;

namespace SpaceDodgeRL.scenes.components {

  public class StorableComponent : Component {
    private static PackedScene _componentPrefab = GD.Load<PackedScene>("res://scenes/components/StorableComponent.tscn");

    public static readonly string ENTITY_GROUP = "STORABLE_COMPONENT_GROUP";
    public override string EntityGroup => ENTITY_GROUP;

    public int Size { get; private set; }

    public static StorableComponent Create(int size = 1) {
      var component = _componentPrefab.Instance() as StorableComponent;

      component.Size = size;

      return component;
    }
  }
}