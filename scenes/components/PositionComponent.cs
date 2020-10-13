using Godot;
using System;

public class PositionComponent : Sprite {
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
      this._gamePosition = value;
      this.Position = IndexToVector(value.X, value.Y);
    }
  }

  public void Init(GamePosition pos, Texture texture) {
    this.GamePosition = pos;
    this.Texture = texture;
  }

  public override void _Ready() {
    if (this._gamePosition.Equals(DefaultGamePosition)) {
      throw new NotImplementedException();
    }
  }

  private static Vector2 IndexToVector(int x, int y, int xOffset=0, int yOffset=0) {
    return new Vector2(START_X + STEP_X * x, START_Y + STEP_Y * y);
  }
}
