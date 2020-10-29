using System;
using Godot;
using SpaceDodgeRL.scenes.entities;

namespace SpaceDodgeRL.scenes.components {

  public class InventoryComponent : Component {
    private static PackedScene _componentPrefab = GD.Load<PackedScene>("res://scenes/components/InventoryComponent.tscn");

    public static readonly string ENTITY_GROUP = "INVENTORY_COMPONENT_GROUP";
    public override string EntityGroup => ENTITY_GROUP;
    public static readonly string ENTITY_IN_INVENTORY_GROUP = "ENTITY_IN_INVENTORY_GROUP";

    public int InventoryUsed { get => this.GetChildCount(); }
    public int InventorySize { get; private set; }

    public static InventoryComponent Create(int inventorySize) {
      var component = _componentPrefab.Instance() as InventoryComponent;

      component.InventorySize = inventorySize;

      return component;
    }

    public bool CanFit(Entity entity) {
      var storableComponent = entity.GetComponent<StorableComponent>();
      return storableComponent != null && this.InventoryUsed + storableComponent.Size <= this.InventorySize;
    }

    public void AddEntity(Entity entity) {
      if (!this.CanFit(entity)) {
        throw new InventoryCannotStoreItemException();
      }
      base.AddChild(entity);
      entity.AddToGroup(ENTITY_IN_INVENTORY_GROUP);
    }
    public class InventoryCannotStoreItemException : Exception {}

    public void RemoveEntity(Entity entity) {
      base.RemoveChild(entity);
      entity.RemoveFromGroup(ENTITY_IN_INVENTORY_GROUP);
    }

    [Obsolete("Use AddEntity(...) instead!")]
    public new void AddChild(Node node, bool legibleUniqueName = false) {
      throw new InvalidOperationException("Use AddEntity(...) instead!");
    }

    [Obsolete("Use RemoveEntity(...) instead!")]
    public new void RemoveChild(Node node) {
      throw new InvalidOperationException("Use RemoveEntity(...) instead!");
    }
  }
}