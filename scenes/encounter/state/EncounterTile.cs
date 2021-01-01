
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

    // For reasons of save size, we only save explored data from our tile map. This is because the entity list can be derived
    // from the components of the entities trivially, so we don't need to store them in the tiles as well.
    public static EncounterTile FromSaveData(bool explored, List<Entity> entities) {
      var tile = new EncounterTile();
      tile.Explored = explored;
      if (entities != null) {
        foreach (var entity in entities) {
          tile.AddEntity(entity);
        }
      }
      return tile;
    }
  }
}