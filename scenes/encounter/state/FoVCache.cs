using SpaceDodgeRL.library.encounter;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SpaceDodgeRL.scenes.encounter.state {

  public class FoVCache {
    private int timesCalledContains = 0;
    private int _x, _y;
    private bool[,] _visibleCells;
    public ReadOnlyCollection<EncounterPosition> VisibleCells { get {
      List<EncounterPosition> asPositions = new List<EncounterPosition>();
      for (int x = 0; x < _visibleCells.GetLength(0); x++) {
        for (int y = 0; y < _visibleCells.GetLength(1); y++) {
          if (_visibleCells[x, y]) {
            asPositions.Add(new EncounterPosition(_x + x, _y + y));
          }
        }
      }
      return asPositions.AsReadOnly();
    } }

    public bool Contains(int x, int y) {
      int cX = x - _x;
      int cY = y - _y;
      if (cX >= 0 && cX < _visibleCells.GetLength(0) && cY >= 0 && cY < _visibleCells.GetLength(1)) {
        return this._visibleCells[cX, cY];
      } else {
        return false;
      }
    }

    protected FoVCache(int x, int y, bool[,] visibleCells) {
      this._x = x;
      this._y = y;
      this._visibleCells = visibleCells;
    }

    public static FoVCache ComputeFoV(EncounterState state, EncounterPosition center, int radius) {
      int _x = center.X - radius;
      int _y = center.Y - radius;
      bool[,] visibleCells = new bool[radius * 2 + 1, radius * 2 + 1];

      // TODO: Implement actual FoV calculations
      for (int x = center.X - radius; x <= center.X + radius; x++) {
        for (int y = center.Y - radius; y <= center.Y + radius; y++) {
          if (center.DistanceTo(x, y) <= radius && state.IsInBounds(x, y)) {
            visibleCells[x - _x, y - _y] = true;
          }
        }
      }

      return new FoVCache(_x, _y, visibleCells);
    }
  }
}