using Priority_Queue;
using System.Collections.Generic;
using SpaceDodgeRL.scenes.encounter;
using System.Linq;

namespace SpaceDodgeRL.library.encounter {

  public class Pathfinder {

    public List<EncounterPosition> AStar(EncounterPosition start, EncounterPosition end, EncounterState state) {
      return AStarWithNewGrid(start, end, state);
    }

    // TODO: If this ends up being slow, we can mark with a dirty bit
    /**
     * Returns a path from start to end, exclusive - that is, the start and end nodes are *not* listed in the path. Does a full
     * naive search every time.
     */
    public static List<EncounterPosition> AStarWithNewGrid(
      EncounterPosition start,
      EncounterPosition end,
      EncounterState state,
      int maxAreaToExplore = 500
    ) {
      SimplePriorityQueue<EncounterPosition> frontier = new SimplePriorityQueue<EncounterPosition>();
      frontier.Enqueue(start, 0f);

      var cameFrom = new Dictionary<EncounterPosition, EncounterPosition>();

      var costSoFar = new Dictionary<EncounterPosition, float>();
      costSoFar[start] = 0f;

      while (frontier.Count > 0 && cameFrom.Count < maxAreaToExplore) {
        var currentPosition = frontier.Dequeue();
        var adjacentPositions = state.AdjacentPositions(currentPosition);

        if (adjacentPositions.Contains(end)) {
          var path = new List<EncounterPosition>() { currentPosition };

          EncounterPosition cameFromPos;
          while (cameFrom.TryGetValue(path[path.Count - 1], out cameFromPos) && (cameFromPos != start)) {
            path.Add(cameFromPos);
          }
          path.Reverse();

          return path;
        }

        var adjacentUnblocked = adjacentPositions.Where(adjacent => !state.IsPositionBlocked(adjacent)).ToList();
        adjacentUnblocked.ForEach(adjacent => {
          var newNextPositionCost = costSoFar[currentPosition] + 1f;
          if (!costSoFar.ContainsKey(adjacent) || newNextPositionCost < costSoFar[adjacent]) {
            costSoFar[adjacent] = newNextPositionCost;
            // Uses straight-line distance as heuristic
            float priority = newNextPositionCost + adjacent.DistanceTo(end);
            frontier.Enqueue(adjacent, priority);
            cameFrom[adjacent] = currentPosition;
          }
        });
      }
      return null;
    }
  }
}