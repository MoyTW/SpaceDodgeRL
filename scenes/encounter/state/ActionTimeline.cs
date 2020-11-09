using SpaceDodgeRL.scenes.components;
using SpaceDodgeRL.scenes.entities;
using System.Collections.Generic;

namespace SpaceDodgeRL.scenes.encounter.state {

  public class ActionTimeline {
    public int CurrentTick { get; private set; }
    public Entity NextEntity { get => _tickToEntities[_nextActiveTicks.Min][0]; }

    private SortedSet<int> _nextActiveTicks = new SortedSet<int>();
    private Dictionary<Entity, int> _entityToTick = new Dictionary<Entity, int>();
    private Dictionary<int, List<Entity>> _tickToEntities = new Dictionary<int, List<Entity>>();

    public ActionTimeline(int startingTick = 0) {
      this.CurrentTick = startingTick;
    }

    public void AddEntityToTimeline(Entity entity, bool front=false) {
    var nextTurnAtTick = entity.GetComponent<ActionTimeComponent>().NextTurnAtTick;

    _nextActiveTicks.Add(nextTurnAtTick);
    _entityToTick[entity] = nextTurnAtTick;

    if (_tickToEntities.ContainsKey(nextTurnAtTick)) {
      if (front) {
      _tickToEntities[nextTurnAtTick].Insert(0, entity);
      } else {
      _tickToEntities[nextTurnAtTick].Add(entity);
      }
    } else {
        _tickToEntities[nextTurnAtTick] = new List<Entity>() { entity };
      }
    }

    public void RemoveEntityFromTimeline(Entity entity) {
      var nextTurnAtTick = _entityToTick[entity];

      _entityToTick.Remove(entity);

      if (_tickToEntities[nextTurnAtTick].Count > 1) {
        _tickToEntities[nextTurnAtTick].Remove(entity);
      } else {
        _tickToEntities.Remove(nextTurnAtTick);
        _nextActiveTicks.Remove(nextTurnAtTick);
        this.CurrentTick = _nextActiveTicks.Min;
      }
    }

    /*
     * You have to remember to call this when an entity moves in time, which isn't great! Luckily right now we don't have any
     * turn manipulation going on but if that happens, you should revisit this.
     */
    public void UpdateTimelineForEntity(Entity entity) {
      this.RemoveEntityFromTimeline(entity);
      this.AddEntityToTimeline(entity);
    }
  }
}