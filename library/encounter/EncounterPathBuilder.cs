using System;
using System.Collections.Generic;

namespace SpaceDodgeRL.library.encounter {

  public static class EncounterPathBuilder {

    public static EncounterPath BuildStraightLinePath(EncounterPosition start, EncounterPosition target, int maxSteps=100, bool endsAtTarget=false) {
      return new EncounterPath(StraightLine(start, target, maxSteps, endsAtTarget));
    }

    public static EncounterPath BuildReverseLinePath(EncounterPosition start, EncounterPosition target, int overshoot) {
      var distance = (int)Math.Ceiling(start.DistanceTo(target));

      var outwardPath = StraightLine(start, target, numSteps: distance + overshoot, endsAtTarget: false);
      var reversePath = StraightLine(outwardPath[outwardPath.Count - 2], start, numSteps: distance + overshoot, endsAtTarget: false);
      reversePath.RemoveAt(reversePath.Count - 1);
      outwardPath.AddRange(reversePath);
      return new EncounterPath(outwardPath);
    }

    private static List<EncounterPosition> StraightLine(EncounterPosition start, EncounterPosition end, int numSteps, bool endsAtTarget) {
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
        EncounterPosition newPosition;
        if (isVertical) {
          cY += yErr;
          newPosition = new EncounterPosition(cX, cY);
        } else if (error >= 0.5f) {
          cY += yErr;
          error -= 1f;
          newPosition = new EncounterPosition(cX, cY);
        } else {
          cX += xDiff;
          error += dError;
          newPosition = new EncounterPosition(cX, cY);
        }

        acc.Add(newPosition);
        if (endsAtTarget && newPosition == end) {
          break;
        }

        steps += 1;
      }

      return acc;
    }
  }
}