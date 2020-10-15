using System;

namespace SpaceDodgeRL.library.encounter {

  public struct GamePosition: IEquatable<GamePosition> {
    public int X { get; }
    public int Y { get; }

    public GamePosition(int x, int y) {
      X = x;
      Y = y;
    }

    public bool Equals(GamePosition other) {
      return other.X == this.X && other.Y == this.Y;
    }

    public override bool Equals(object obj) {
      if (!(obj is GamePosition)) { return false; }
      return ((GamePosition)obj).X == this.X && ((GamePosition)obj).Y == this.Y;
    }

    public override int GetHashCode() {
      return (this.X.GetHashCode() * 7) ^ this.Y.GetHashCode();
    }

    public override string ToString() {
      return string.Format("[{0}, {1}]", this.X, this.Y);
    }

    public static bool operator ==(GamePosition left, GamePosition right) {
      return left.Equals(right);
    }

    public static bool operator !=(GamePosition left, GamePosition right) {
      return !left.Equals(right);
    }
  }
}