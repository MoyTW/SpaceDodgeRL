using SpaceDodgeRL.library.encounter;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SpaceDodgeRL.scenes.encounter.state {

  public class FoVCache {
    private List<EncounterPosition> _visibleCells;
    public ReadOnlyCollection<EncounterPosition> VisibleCells { get => this._visibleCells.AsReadOnly(); }

    public bool Contains(int x, int y) {
      return this._visibleCells.Contains(new EncounterPosition(x, y));
    }

    protected FoVCache(List<EncounterPosition> visibleCells) {
      this._visibleCells = visibleCells;
    }

    public static FoVCache ComputeFoV(EncounterState state, EncounterPosition center, int radius) {
      List<EncounterPosition> visibleCells = new List<EncounterPosition>();

      // TODO: Implement actual FoV calculations
      for (int x = center.X - radius; x <= center.X + radius; x++) {
        for (int y = center.Y - radius; y <= center.Y + radius; y++) {
          if (center.DistanceTo(x, y) <= radius && state.IsInBounds(x, y)) {
            visibleCells.Add(new EncounterPosition(x, y));
          }
        }
      }

      return new FoVCache(visibleCells);
    }
  }
}