using Godot;
using System;

public class ActionTimeComponent : Node {
  private int _ticksUntilTurn = int.MinValue;
  public int TicksUntilTurn { get => _ticksUntilTurn; }

  public void Init(int ticksUntilTurn = 0) {
    this._ticksUntilTurn = ticksUntilTurn;
  }

  public override void _Ready() {
    if (this._ticksUntilTurn == int.MinValue) {
      throw new NotImplementedException();
    }
  }

  public bool IsReady() { return _ticksUntilTurn == 0; }

  public void PassTime(int ticks) {
    if (this._ticksUntilTurn < 0) {
      throw new NotImplementedException("Couldn't pass ticks!");
    }
    this._ticksUntilTurn -= ticks;
  }

  public void EndTurn(SpeedComponent speedComponent) {
    this._ticksUntilTurn = speedComponent.Speed;
  }
}
