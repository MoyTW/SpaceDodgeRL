using System;
using Godot;
using SpaceDodgeRL.scenes.entities;

namespace SpaceDodgeRL.scenes.components {

  public class InventoryComponent : Component {
    private static PackedScene _componentPrefab = GD.Load<PackedScene>("res://scenes/components/InventoryComponent.tscn");

    public static readonly string ENTITY_GROUP = "INVENTORY_COMPONENT_GROUP";
    public override string EntityGroup => ENTITY_GROUP;
    public static readonly string ENTITY_IN_INVENTORY_GROUP = "ENTITY_IN_INVENTORY_GROUP";

    public int InventorySize { get; private set; }
    public bool InventoryFull { get => this.GetChildCount() >= this.InventorySize; }

    public static InventoryComponent Create(int inventorySize) {
      var component = _componentPrefab.Instance() as InventoryComponent;

      component.InventorySize = inventorySize;

      return component;
    }

    public void AddEntity(Entity entity) {
      if (this.InventoryFull) {
        throw new InventoryFullException();
      }
      base.AddChild(entity);
      entity.AddToGroup(ENTITY_IN_INVENTORY_GROUP);
    }
    public class InventoryFullException : Exception {}

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