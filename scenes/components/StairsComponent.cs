using Godot;
using System;

namespace SpaceDodgeRL.scenes.components {

  public class StairsComponent : Component {
    private static PackedScene _componentPrefab = GD.Load<PackedScene>("res://scenes/components/StairsComponent.tscn");

    public static readonly string ENTITY_GROUP = "STAIRS_COMPONENT_GROUP";
    public override string EntityGroup => ENTITY_GROUP;

    public static StairsComponent Create() {
      var component = _componentPrefab.Instance() as StairsComponent;
      return component;
    }
  }
}