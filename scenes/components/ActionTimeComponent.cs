using Godot;
using System;

namespace SpaceDodgeRL.scenes.components {

  public class ActionTimeComponent : Component {
    private static PackedScene _componentPrefab = GD.Load<PackedScene>("res://scenes/components/ActionTimeComponent.tscn");

    public static readonly string ENTITY_GROUP = "ACTION_TIME_COMPONENT_GROUP";
    public override string EntityGroup => ENTITY_GROUP;

    public int NextTurnAtTick { get; private set; }

    public static ActionTimeComponent Create(int currentTick, int ticksUntilTurn = 0) {
      var component = _componentPrefab.Instance() as ActionTimeComponent;

      component.NextTurnAtTick = currentTick + ticksUntilTurn;

      return component;
    }

    public void EndTurn(SpeedComponent speedComponent) {
      this.NextTurnAtTick = this.NextTurnAtTick + speedComponent.Speed;
    }
  }
}