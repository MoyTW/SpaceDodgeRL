using Godot;
using System;

namespace SpaceDodgeRL.scenes.components {

  public class SpeedComponent : Component {
    private static PackedScene _componentPrefab = GD.Load<PackedScene>("res://scenes/components/SpeedComponent.tscn");

    public static readonly string ENTITY_GROUP = "SPEED_COMPONENT_GROUP";
    public string EntityGroup => ENTITY_GROUP;

    public int BaseSpeed { get; private set; }
    // TODO: Buffs
    public int Speed { get => BaseSpeed; }

    public static SpeedComponent Create(int baseSpeed) {
      var component = new SpeedComponent();

      component.BaseSpeed = baseSpeed;

      return component;
    }
  }
}