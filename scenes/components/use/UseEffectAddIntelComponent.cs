using Godot;
using System;

namespace SpaceDodgeRL.scenes.components.use {

  public class UseEffectAddIntelComponent : Component {
    private static PackedScene _componentPrefab = GD.Load<PackedScene>("res://scenes/components/use/UseEffectAddIntelComponent.tscn");

    public static readonly string ENTITY_GROUP = "USE_EFFECT_ADD_INTEL_COMPONENT_GROUP";
    public override string EntityGroup => ENTITY_GROUP;

    public int TargetDungeonLevel { get; private set; }

    public static UseEffectAddIntelComponent Create(int targetDungeonLevel) {
      var component = _componentPrefab.Instance() as UseEffectAddIntelComponent;

      component.TargetDungeonLevel = targetDungeonLevel;

      return component;
    }
  }
}