using System;
using System.Collections.Generic;

namespace SpaceDodgeRL.library {

  public class RPASCalculator {
    private class CellAngles {
      public double Near { get; set; }
      public double Center { get; set; }
      public double Far { get; set; }

      public CellAngles(double near, double center, double far) {
        this.Near = near;
        this.Center = center;
        this.Far = far;
      }
    }

    public double RadiusFudge { get; }
    public bool NotVisibleBlocksVision { get; }
    public int Restrictiveness { get; }
    public bool VisibleOnEqual { get; }

    public RPASCalculator(
      double radiusFudge = 1f / 3f,
      bool notVisibleBlocksVision = true,
      int restrictiveness = 1,
      bool visibleOnEqual = true
    ) {
      this.RadiusFudge = radiusFudge;
      this.NotVisibleBlocksVision = notVisibleBlocksVision;
      this.Restrictiveness = restrictiveness;
      this.VisibleOnEqual = visibleOnEqual;
    }

    public HashSet<(int, int)> CalcVisibleCellsFrom(int x, int y, int radius, Func<int, int, bool> isTransparent) {
      var cells = VisibleCellsInQuadrantFrom(x, y, 1, 1, radius, isTransparent);
      cells.UnionWith(VisibleCellsInQuadrantFrom(x, y, 1, -1, radius, isTransparent));
      cells.UnionWith(VisibleCellsInQuadrantFrom(x, y, -1, 1, radius, isTransparent));
      cells.UnionWith(VisibleCellsInQuadrantFrom(x, y, -1, -1, radius, isTransparent));
      return cells;
    }

    private HashSet<(int, int)> VisibleCellsInQuadrantFrom(int centerX, int centerY, int quadX, int quadY, int radius, Func<int, int, bool> isTransparent) {
      var cells = VisibleCellsInOctantFrom(centerX, centerY, quadX, quadY, radius, isTransparent, true);
      cells.UnionWith(VisibleCellsInOctantFrom(centerX, centerY, quadX, quadY, radius, isTransparent, false));
      return cells;
    }

    private HashSet<(int, int)> VisibleCellsInOctantFrom(int centerX, int centerY, int quadX, int quadY, int radius, Func<int, int, bool> isTransparent, bool isVertical) {
      int iteration = 1;
      HashSet<(int, int)> visibleCells = new HashSet<(int, int)>();
      List<CellAngles> obstructions = new List<CellAngles>();

      while (iteration <= radius && !(obstructions.Count == 1 && obstructions[0].Near == 0.0 && obstructions[0].Far == 1.0)) {
        int numCellsInRow = iteration + 1;
        double angleAllocation = 1.0f / (double)numCellsInRow;

        for (int step = 0; step < numCellsInRow; step++) {
          var cell = CellAt(centerX, centerY, quadX, quadY, step, iteration, isVertical);

          if (CellInRadius(centerX, centerY, cell, radius)) {
            CellAngles angles = new CellAngles((double)step * angleAllocation,
                                               ((double)step + 0.5) * angleAllocation,
                                               ((double)step + 1.0) * angleAllocation);
            if (CellIsVisible(angles, obstructions)) {
              visibleCells.Add(cell);
              if (!isTransparent(cell.Item1, cell.Item2)) {
                obstructions = AddObstruction(obstructions, angles);
              }
            } else if (this.NotVisibleBlocksVision) {
              obstructions = AddObstruction(obstructions, angles);
            }
          }
        }

        iteration += 1;
      }

      return visibleCells;
    }

    private (int, int) CellAt(int centerX, int centerY, int quadX, int quadY, int step, int iteration, bool isVertical) {
      if (isVertical) {
        return (centerX + step * quadX, centerY + iteration * quadY);
      } else {
        return (centerX + iteration * quadX, centerY + step * quadY);
      }
    }

    private bool CellInRadius(int centerX, int centerY, (int, int) cell, int radius) {
      double distance = Math.Sqrt((centerX - cell.Item1) * (centerX - cell.Item1) + (centerY - cell.Item2) * (centerY - cell.Item2));
      return distance <= radius + RadiusFudge;
    }

    private bool CellIsVisible(CellAngles angles, List<CellAngles> obstructions) {
      bool nearVisible = true;
      bool centerVisible = true;
      bool farVisible = true;

      foreach (CellAngles obstruction in obstructions) {
        if (this.VisibleOnEqual) {
          if (obstruction.Near < angles.Near && angles.Near < obstruction.Far) {
            nearVisible = false;
          }
          if (obstruction.Near < angles.Center && angles.Center < obstruction.Far) {
            centerVisible = false;
          }
          if (obstruction.Near < angles.Far && angles.Far < obstruction.Far) {
            farVisible = false;
          }
        } else {
          if (obstruction.Near <= angles.Near && angles.Near <= obstruction.Far) {
            nearVisible = false;
          }
          if (obstruction.Near <= angles.Center && angles.Center <= obstruction.Far) {
            centerVisible = false;
          }
          if (obstruction.Near <= angles.Far && angles.Far <= obstruction.Far) {
            farVisible = false;
          }
        }
      }

      if (this.Restrictiveness == 0) {
        return centerVisible || nearVisible || farVisible;
      } else if (this.Restrictiveness == 1) {
        return (centerVisible && nearVisible) || (centerVisible && farVisible);
      } else {
        return centerVisible && nearVisible && farVisible;
      }
    }

    private List<CellAngles> AddObstruction(List<CellAngles> obstructions, CellAngles newObstruction) {
      List<CellAngles> newList = new List<CellAngles>();
      CellAngles newObject = new CellAngles(newObstruction.Near, newObstruction.Center, newObstruction.Far);
      foreach(var oldObstruction in obstructions) {
        if (!CombineObstructions(oldObstruction, newObject)) {
          newList.Add(oldObstruction);
        }
      }
      obstructions.Add(newObject);
      return obstructions;
    }

    private bool CombineObstructions(CellAngles oldObstruction, CellAngles newObstruction) {
      CellAngles lowObstruction, highObstruction;
      if (oldObstruction.Near < newObstruction.Near) {
        lowObstruction = oldObstruction;
        highObstruction = newObstruction;
      } else if (newObstruction.Near < oldObstruction.Near) {
        lowObstruction = newObstruction;
        highObstruction = oldObstruction;
      } else {
        newObstruction.Far = Math.Max(oldObstruction.Far, newObstruction.Far);
        return true;
      }

      if (lowObstruction.Far >= highObstruction.Near) {
        newObstruction.Near = Math.Min(lowObstruction.Near, highObstruction.Near);
        newObstruction.Far = Math.Max(lowObstruction.Far, highObstruction.Far);
        return true;
      } else {
        return false;
      }
    }
  }
}