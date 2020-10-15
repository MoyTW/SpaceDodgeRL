using Godot;
using System;

namespace SpaceDodgeRL.scenes.components {

  public class ActionTimeComponent : Component {
    private static PackedScene _componentPrefab = GD.Load<PackedScene>("res://scenes/components/ActionTimeComponent.tscn");

    public static readonly string ENTITY_GROUP = "ACTION_TIME_COMPONENT_GROUP";
    public override string EntityGroup => ENTITY_GROUP;

    public int TicksUntilTurn { get; private set; }

    public static ActionTimeComponent Create(int ticksUntilTurn = 0) {
      var component = _componentPrefab.Instance() as ActionTimeComponent;

      component.TicksUntilTurn = ticksUntilTurn;

      return component;
    }

    public bool IsReady() { return this.TicksUntilTurn == 0; }

    public void PassTime(int ticks) {
      if (this.TicksUntilTurn < 0) {
        throw new NotImplementedException("Couldn't pass ticks!");
      }
      this.TicksUntilTurn -= ticks;
    }

    public void EndTurn(SpeedComponent speedComponent) {
      this.TicksUntilTurn = speedComponent.Speed;
    }
  }
}