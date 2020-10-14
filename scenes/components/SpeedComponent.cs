using Godot;
using System;

public class SpeedComponent : Node, Component {
  public static string ENTITY_GROUP = "SPEED_COMPONENT_GROUP";
  public string EntityGroup => ENTITY_GROUP;

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
