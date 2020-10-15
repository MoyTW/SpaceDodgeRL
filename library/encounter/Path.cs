using System;
using System.Collections.Generic;

namespace SpaceDodgeRL.library.encounter {

  /**
   * Data class to represent a series of [x, y] coordinate pairs over a time period.
   */
  public class Path {
    private int _currentStep = 0;
    private List<EncounterPosition> _fullPath;

    public Path(List<EncounterPosition> path) {
      this._fullPath = path;
    }

    public EncounterPosition CurrentPosition { get => _fullPath[_currentStep]; }
    public bool AtEnd { get => _currentStep >= _fullPath.Count - 1; }

    public EncounterPosition Step() {
      if (this.AtEnd) {
        throw new InvalidOperationException("Can't step, already at end");
      }
      _currentStep += 1;
      return CurrentPosition;
    }

    public List<EncounterPosition> Project(int steps) {
      var stepsLeft = _fullPath.Count - (_currentStep + 1);
      return _fullPath.GetRange(_currentStep, Math.Min(steps, stepsLeft));
    }
  }
}