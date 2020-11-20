using System;
using System.Text.Json.Serialization;

namespace SpaceDodgeRL.library.encounter {

  public struct EncounterPosition: IEquatable<EncounterPosition> {
    [JsonInclude] public int X { get; private set; }
    [JsonInclude] public int Y { get; private set; }

    public EncounterPosition(int x, int y) {
      X = x;
      Y = y;
    }

    /**
     * Straight-line distance, not path distance. Thankfullky there are no walls in space! Also, as implied by float, it's NOT
     * Fire Emblem distance ("int representing number of steps to get here").
     */
    public float DistanceTo(int x, int y) {
      float dx = (this.X - x);
      float dy = (this.Y - y);
      // RIP MathF, casulaty of the runtime confusion.
      return (float)Math.Sqrt(dx * dx + dy * dy);
    }

    public float DistanceTo(EncounterPosition other) {
      return this.DistanceTo(other.X, other.Y);
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