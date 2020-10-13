using Godot;
using System;

public class ActionTimeComponent : Node {
  private SpeedComponent _speedComponent = null;

  private int _ticksUntilTurn = 0;
  public int TicketsUntilTurn { get => _ticksUntilTurn; }

  public void Init(SpeedComponent speedComponent) {
    this._speedComponent = speedComponent;
  }

  public bool IsReady() { return _ticksUntilTurn == 0; }

  public void PassTime(int ticks) {
    if (this._ticksUntilTurn < 0) {
      throw new NotImplementedException("Couldn't pass ticks!");
    }
    this._ticksUntilTurn -= ticks;
  }

  public void EndTurn() {
    this._ticksUntilTurn = _speedComponent.Speed;
  }
}
