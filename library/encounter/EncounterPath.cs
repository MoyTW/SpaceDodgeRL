using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SpaceDodgeRL.library.encounter {

  /**
   * Data class to represent a series of [x, y] coordinate pairs over a time period.
   */
  public class EncounterPath {

    [JsonInclude] public int CurrentStep { get; private set; }
    [JsonInclude] public List<EncounterPosition> FullPath { get; private set; }

    public EncounterPosition CurrentPosition { get => FullPath[CurrentStep]; }
    public bool AtEnd { get => CurrentStep >= FullPath.Count - 1; }

    public EncounterPath(List<EncounterPosition> fullPath, int currentStep = 0) {
      if (fullPath == null || fullPath.Count == 0) {
        throw new NotImplementedException("You can't make a path with a list that's null!");
      }
      this.CurrentStep = currentStep;
      this.FullPath = fullPath;
    }

    public EncounterPosition Step() {
      if (this.AtEnd) {
        throw new InvalidOperationException("Can't step, already at end");
      }
      CurrentStep += 1;
      return CurrentPosition;
    }

    public List<EncounterPosition> Project(int steps) {
      var stepsLeft = FullPath.Count - (CurrentStep + 1);
      return FullPath.GetRange(CurrentStep + 1, Math.Min(steps, stepsLeft));
    }
  }
}