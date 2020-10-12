using Godot;
using System;

public class PositionComponent : Sprite {
  // TODO: Don't put this here
  public const int START_X = 50;
  public const int START_Y = 54;
  public const int STEP_X = 26;
  public const int STEP_Y = 34;

  private Position _position;

  public void Init(Position pos) {
    this.SetPosition(pos);
  }

  public void SetPosition(Position pos) {
    this._position = pos;
    this.Position = IndexToVector(pos.X, pos.Y);
  }

  private static Vector2 IndexToVector(int x, int y, int xOffset=0, int yOffset=0) {
    return new Vector2(START_X + STEP_X * x, START_Y + STEP_Y * y);
  }
}
