using Godot;
using System;

namespace SpaceDodgeRL.scenes.components {

  public class StairsComponent : Component {
    public static readonly string ENTITY_GROUP = "STAIRS_COMPONENT_GROUP";
    public string EntityGroup => ENTITY_GROUP;

    public static StairsComponent Create() {
      var component = new StairsComponent();
      return component;
    }
  }
}