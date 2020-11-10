using Godot;
using System;

namespace SpaceDodgeRL.scenes.components.use {

  public class UseEffectAddIntelComponent : Component {
    public static readonly string ENTITY_GROUP = "USE_EFFECT_ADD_INTEL_COMPONENT_GROUP";
    public string EntityGroup => ENTITY_GROUP;

    public int TargetDungeonLevel { get; private set; }

    public static UseEffectAddIntelComponent Create(int targetDungeonLevel) {
      var component = new UseEffectAddIntelComponent();

      component.TargetDungeonLevel = targetDungeonLevel;

      return component;
    }
  }
}