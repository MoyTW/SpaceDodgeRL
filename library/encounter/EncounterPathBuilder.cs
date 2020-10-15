using System;
using System.Collections.Generic;

namespace SpaceDodgeRL.library.encounter {

  public static class EncounterPathBuilder {

    public static EncounterPath BuildStraightLinePath(EncounterPosition start, EncounterPosition target, int numSteps = 100) {
      return new EncounterPath(StraightLine(start, target, numSteps));
    }

    private static List<EncounterPosition> StraightLine(EncounterPosition start, EncounterPosition end, int numSteps) {
      List<EncounterPosition> acc = new List<EncounterPosition>();
      acc.Add(start);

      bool isVertical = start.X == end.X;
      float dError = isVertical ? float.MaxValue : Math.Abs(((float)end.Y - (float)start.Y) / ((float)end.X - (float)start.X));

      int yErr = end.Y - start.Y > 0 ? 1 : -1;
      int xDiff = end.X - start.X > 0 ? 1 : -1;

      float error = 0.0f;
      int cX = start.X;
      int cY = start.Y;
      int steps = 0;

      while (steps < numSteps) {
        if (isVertical) {
          cY += yErr;
          acc.Add(new EncounterPosition(cX, cY));
        } else if (error >= 0.5f) {
          cY += yErr;
          error -= 1f;
          acc.Add(new EncounterPosition(cX, cY));
        } else {
          cX += xDiff;
          error += dError;
          acc.Add(new EncounterPosition(cX, cY));
        }
        steps += 1;
      }

      return acc;
    }
  }
}