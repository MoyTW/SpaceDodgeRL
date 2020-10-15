using Godot;
using System;

namespace SpaceDodgeRL.scenes.components {

  public class ActionTimeComponent : Node, Component {
    public static readonly string ENTITY_GROUP = "ACTION_TIME_COMPONENT_GROUP";
    public string EntityGroup => ENTITY_GROUP;

    private int _ticksUntilTurn = int.MinValue;
    public int TicksUntilTurn { get => _ticksUntilTurn; }



    public void Init(int ticksUntilTurn = 0) {
      _ticksUntilTurn = ticksUntilTurn;
    }

    public override void _Ready() {
      if (_ticksUntilTurn == int.MinValue) {
        throw new NotImplementedException();
      }
    }

    public bool IsReady() { return _ticksUntilTurn == 0; }

    public void PassTime(int ticks) {
      if (_ticksUntilTurn < 0) {
        throw new NotImplementedException("Couldn't pass ticks!");
      }
      _ticksUntilTurn -= ticks;
    }

    public void EndTurn(SpeedComponent speedComponent) {
      _ticksUntilTurn = speedComponent.Speed;
    }
  }
}