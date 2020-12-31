using SpaceDodgeRL.scenes.components;
using SpaceDodgeRL.scenes.entities;
using System.Collections.Generic;
using System.Linq;

namespace SpaceDodgeRL.scenes.encounter.state {

  public class ActionTimeline {
    public int CurrentTick { get; private set; }
    public Entity NextEntity { get => _tickToEntities[_nextActiveTicks.Min][0]; }

    private SortedSet<int> _nextActiveTicks = new SortedSet<int>();
    private Dictionary<Entity, int> _entityToTick = new Dictionary<Entity, int>();
    // Used as a consistent secondary sort determiner; now it's "in order of addition to timeline" which may later need some
    // refinement. On the other hand I can't think of a good reason not to just do in creation order...
    private Dictionary<string, int> _entityIdToOrder = new Dictionary<string, int>();
    private Dictionary<int, List<Entity>> _tickToEntities = new Dictionary<int, List<Entity>>();

    public ActionTimeline(int startingTick = 0) {
      this.CurrentTick = startingTick;
    }

    public void AddEntityToTimeline(Entity entity, bool front=false) {
      if (!this._entityIdToOrder.ContainsKey(entity.EntityId)) {
        if (front) {
          this._entityIdToOrder[entity.EntityId] = -1;
        } else {
          this._entityIdToOrder[entity.EntityId] = this._entityIdToOrder.Count + 1;
        }
      }
      var secondarySortOrder = this._entityIdToOrder[entity.EntityId];

      var nextTurnAtTick = entity.GetComponent<ActionTimeComponent>().NextTurnAtTick;
      if (nextTurnAtTick < CurrentTick) {
        Godot.GD.PrintErr(
          string.Format("Entity {0} is somehow going back in time!? Next action at tick {1}, but current tick {2}!",
                        entity.EntityName, nextTurnAtTick, CurrentTick));
      }

      this._nextActiveTicks.Add(nextTurnAtTick);
      this._entityToTick[entity] = nextTurnAtTick;

      if (this._tickToEntities.ContainsKey(nextTurnAtTick)) {
        if (secondarySortOrder == -1) {
          this._tickToEntities[nextTurnAtTick].Insert(0, entity);
        } else {
          var existingList = this._tickToEntities[nextTurnAtTick];
          // I'm at least 90% sure there's some combination of LINQ and in-built functions that does this prettily.
          var idx = -1;
          for (int i = 0; i < existingList.Count; i++) {
            if (this._entityIdToOrder[existingList[i].EntityId] > secondarySortOrder) {
              idx = i;
              break;
            }
          }
          if (idx != -1) {
            existingList.Insert(idx, entity);
          } else {
            existingList.Add(entity);
          }
        }
      } else {
        this._tickToEntities[nextTurnAtTick] = new List<Entity>() { entity };
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
      }
    }

    /*
     * If you introduce turn manipulation, you will have to create a update function and call it after every manipulation.
     */
    public void EntityHasEndedTurn(Entity entity) {
      this.RemoveEntityFromTimeline(entity);
      this.AddEntityToTimeline(entity);

      this.CurrentTick = this.NextEntity.GetComponent<ActionTimeComponent>().NextTurnAtTick;
    }

    public class SaveData {
      public int CurrentTick { get; set; }
      public List<int> NextActiveTicks { get; set; }
      public Dictionary<string, int> EntityIdToTick { get; set; }
      public Dictionary<string, int> EntityIdToOrder { get; set; }
      public Dictionary<int, List<string>> TickToEntityIds { get; set; }
    }

    public static ActionTimeline FromSaveData(SaveData data, Dictionary<string, Entity> entitiesById) {
      var timeline = new ActionTimeline(data.CurrentTick);

      foreach (var tick in data.NextActiveTicks) {
        timeline._nextActiveTicks.Add(tick);
      }
      foreach(var kvp in data.EntityIdToTick) {
        timeline._entityToTick[entitiesById[kvp.Key]] = kvp.Value;
      }
      timeline._entityIdToOrder = data.EntityIdToOrder;
      foreach(var kvp in data.TickToEntityIds) {
        timeline._tickToEntities[kvp.Key] = kvp.Value.Select(id => entitiesById[id]).ToList();
      }
      return timeline;
    }

    public SaveData ToSaveData() {
      var data = new SaveData();
      data.CurrentTick = this.CurrentTick;
      data.NextActiveTicks = new List<int>(this._nextActiveTicks);
      // There's probably a fancy LINQ way to do this innit?
      data.EntityIdToTick = new Dictionary<string, int>();
      foreach (var kvp in this._entityToTick) {
        data.EntityIdToTick[kvp.Key.EntityId] = kvp.Value;
      }
      data.EntityIdToOrder = this._entityIdToOrder;
      data.TickToEntityIds = new Dictionary<int, List<string>>();
      foreach (var kvp in this._tickToEntities) {
        data.TickToEntityIds[kvp.Key] = kvp.Value.Select(e => e.EntityId).ToList();;
      }
      return data;
    }
  }
}