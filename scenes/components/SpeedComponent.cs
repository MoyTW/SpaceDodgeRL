using Godot;
using System;

public class SpeedComponent : Node {
  private int _baseSpeed = int.MinValue;
  public int BaseSpeed { get => _baseSpeed; }
  // TODO: Buffs
  public int Speed { get => _baseSpeed ; }

  public void Init(int baseSpeed) {
    this._baseSpeed = baseSpeed;
  }

  public override void _Ready() {
    if (this._baseSpeed == int.MinValue) {
      throw new NotImplementedException();
    }
  }
}
