using Godot;
using SpaceDodgeRL.library.encounter;
using System;

namespace SpaceDodgeRL.scenes.components {

  public class PositionComponent : Sprite, Component {
    private static PackedScene _componentPrefab = GD.Load<PackedScene>("res://scenes/components/PositionComponent.tscn");

    const string ENTITY_GROUP = "POSITION_COMPONENT_GROUP";
    public string EntityGroup => ENTITY_GROUP;

    // TODO: Don't put this here
    public const int START_X = 50;
    public const int START_Y = 54;
    public const int STEP_X = 26;
    public const int STEP_Y = 34;

    public static GamePosition DefaultGamePosition = new GamePosition(int.MinValue, int.MinValue);

    private GamePosition _gamePosition = DefaultGamePosition;
    public GamePosition GamePosition {
      get => _gamePosition;
      set {
        _gamePosition = value;
        Tween(IndexToVector(value.X, value.Y));
      }
    }

    public static PositionComponent Create(GamePosition position, Texture texture) {
      var component = _componentPrefab.Instance() as PositionComponent;

      component._gamePosition = position;
      component.Position = IndexToVector(position.X, position.Y);
      component.Texture = texture;

      return component;
    }

    private void Tween(Vector2 newPosition) {
      var tween = GetNode<Tween>("Tween");
      tween.InterpolateProperty(this, "position", Position, newPosition, 0.05f);
      tween.Start();
    }

    public override void _Ready() {
      if (_gamePosition.Equals(DefaultGamePosition)) {
        throw new NotImplementedException();
      }
    }

    private static Vector2 IndexToVector(int x, int y, int xOffset = 0, int yOffset = 0) {
      return new Vector2(START_X + STEP_X * x, START_Y + STEP_Y * y);
    }
  }
}