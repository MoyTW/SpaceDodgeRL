
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using SpaceDodgeRL.scenes.components;
using SpaceDodgeRL.scenes.entities;

namespace SpaceDodgeRL.scenes.encounter.state {

  public class EncounterTile {
    public bool BlocksMovement {
      get {
        return _entities.Any(e => {
          var component = e.GetComponent<CollisionComponent>();
          return component != null && component.BlocksMovement;
        });
      }
    }
    public bool BlocksVision {
      get {
        return _entities.Any(e => {
          var component = e.GetComponent<CollisionComponent>();
          return component != null && component.BlocksVision;
        });
      }
    }
    public bool Explored { get; set; } = false;

    private List<Entity> _entities = new List<Entity>();
    public ReadOnlyCollection<Entity> Entities { get => _entities.AsReadOnly(); }

    public void AddEntity(Entity entity) {
      _entities.Add(entity);
    }
    public void RemoveEntity(Entity entity) {
      _entities.Remove(entity);
    }

    public class SaveData {
      public int X { get; set; }
      public int Y { get; set; }
      public bool Explored { get; set; }
      public List<string> EntityIds { get; set; }

      public SaveData(int x, int y, bool explored, List<string> entityIds) {
        this.X = x;
        this.Y = y;
        this.Explored = explored;
        this.EntityIds = entityIds;
      }
    }

    public static EncounterTile FromSaveData(SaveData data, Dictionary<string, Entity> entitiesById) {
      var tile = new EncounterTile();
      tile.Explored = data.Explored;
      foreach (var entityId in data.EntityIds) {
        tile.AddEntity(entitiesById[entityId]);
      }
      return tile;
    }

    // Returns null if there's nothing interesting in the array
    public SaveData ToSaveData(int x, int y) {
      if (!this.Explored && this.Entities.Count == 0) {
        return null;
      } else {
        var entityIds = this._entities.Select(e => e.EntityId).ToList();
        return new SaveData(x, y, this.Explored, entityIds);
      }
    }
  }
}