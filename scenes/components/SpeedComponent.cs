using Godot;
using System;

namespace SpaceDodgeRL.scenes.components {

  public class SpeedComponent : Node, Component {
    public static readonly string ENTITY_GROUP = "SPEED_COMPONENT_GROUP";
    public string EntityGroup => ENTITY_GROUP;

    private int _baseSpeed = int.MinValue;
    public int BaseSpeed { get => _baseSpeed; }
    // TODO: Buffs
    public int Speed { get => _baseSpeed; }

    public void Init(int baseSpeed) {
      _baseSpeed = baseSpeed;
    }

    public override void _Ready() {
      if (_baseSpeed == int.MinValue) {
        throw new NotImplementedException();
      }
    }
  }
}