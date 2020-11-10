using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Godot;
using SpaceDodgeRL.scenes.entities;

namespace SpaceDodgeRL.scenes.components {

  public class InventoryComponent : Component {
    public static readonly string ENTITY_GROUP = "INVENTORY_COMPONENT_GROUP";
    public string EntityGroup => ENTITY_GROUP;
    public static readonly string ENTITY_IN_INVENTORY_GROUP = "ENTITY_IN_INVENTORY_GROUP";

    private List<Entity> _storedEntities = new List<Entity>();
    public int InventoryUsed { get => this._storedEntities.Count; }
    public int InventorySize { get; private set; }

    public static InventoryComponent Create(int inventorySize) {
      var component = new InventoryComponent();

      component.InventorySize = inventorySize;

      return component;
    }

    public ReadOnlyCollection<Entity> StoredItems { get => this._storedEntities.AsReadOnly(); }

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
      } else if (this._storedEntities.Contains(entity)) {
        throw new InventoryAlreadyHasItemException();
      }
      this._storedEntities.Add(entity);
      entity.AddToGroup(ENTITY_IN_INVENTORY_GROUP);
    }
    public class InventoryFullCannotStoreItemException : Exception {}
    public class InventoryAlreadyHasItemException : Exception {}

    public void RemoveEntity(Entity entity) {
      this._storedEntities.Remove(entity);
      entity.RemoveFromGroup(ENTITY_IN_INVENTORY_GROUP);
    }
  }
}