using System;

namespace SpaceDodgeRL.library.encounter {

  public struct EncounterPosition: IEquatable<EncounterPosition> {
    public int X { get; }
    public int Y { get; }

    public EncounterPosition(int x, int y) {
      X = x;
      Y = y;
    }

    public bool Equals(EncounterPosition other) {
      return other.X == this.X && other.Y == this.Y;
    }

    public override bool Equals(object obj) {
      if (!(obj is EncounterPosition)) { return false; }
      return ((EncounterPosition)obj).X == this.X && ((EncounterPosition)obj).Y == this.Y;
    }

    public override int GetHashCode() {
      return (this.X.GetHashCode() * 7) ^ this.Y.GetHashCode();
    }

    public override string ToString() {
      return string.Format("[{0}, {1}]", this.X, this.Y);
    }

    public static bool operator ==(EncounterPosition left, EncounterPosition right) {
      return left.Equals(right);
    }

    public static bool operator !=(EncounterPosition left, EncounterPosition right) {
      return !left.Equals(right);
    }
  }
}