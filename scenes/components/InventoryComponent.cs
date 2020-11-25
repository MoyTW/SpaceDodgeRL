using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text.Json.Serialization;
using Godot;
using SpaceDodgeRL.scenes.entities;

namespace SpaceDodgeRL.scenes.components {

  public class InventoryComponent : Component {
    public static readonly string ENTITY_GROUP = "INVENTORY_COMPONENT_GROUP";
    public string EntityGroup => ENTITY_GROUP;
    public static readonly string ENTITY_IN_INVENTORY_GROUP = "ENTITY_IN_INVENTORY_GROUP";

    [JsonInclude] public List<Entity> _StoredEntities { get; private set; } = new List<Entity>();
    [JsonIgnore] public ReadOnlyCollection<Entity> StoredItems { get => this._StoredEntities.AsReadOnly(); }
    public int InventoryUsed { get => this._StoredEntities.Count; }
    [JsonInclude] public int InventorySize { get; private set; }

    public static InventoryComponent Create(int inventorySize) {
      var component = new InventoryComponent();

      component.InventorySize = inventorySize;

      return component;
    }

    public static InventoryComponent Create(string saveData) {
      return JsonSerializer.Deserialize<InventoryComponent>(saveData);
    }

    // If this gets slow we should store in a map not a list
    public Entity StoredEntityById(string entityId) {
      foreach (Entity e in StoredItems) {
        if (e.EntityId == entityId) {
          return e;
        }
      }
      return null;
    }

    public bool CanFit(Entity entity) {
      var storableComponent = entity.GetComponent<StorableComponent>();
      return storableComponent != null && this.InventoryUsed + storableComponent.Size <= this.InventorySize;
    }

    public void AddEntity(Entity entity) {
      if (!this.CanFit(entity)) {
        throw new InventoryFullCannotStoreItemException();
      } else if (this._StoredEntities.Contains(entity)) {
        throw new InventoryAlreadyHasItemException();
      }
      this._StoredEntities.Add(entity);
      entity.AddToGroup(ENTITY_IN_INVENTORY_GROUP);
    }
    public class InventoryFullCannotStoreItemException : Exception {}
    public class InventoryAlreadyHasItemException : Exception {}

    public void RemoveEntity(Entity entity) {
      this._StoredEntities.Remove(entity);
      // TODO: BUG - When you save/load it doesn't add in group; I mean we may as well just nuke the
      // ENTITY_IN_INVENTORY_GROUP tag altogether 'till we need it?
      entity.RemoveFromGroup(ENTITY_IN_INVENTORY_GROUP);
    }

    public string Save() {
      return JsonSerializer.Serialize(this);
    }

    public void NotifyAttached(Entity parent) { }

    public void NotifyDetached(Entity parent) { }
  }
}