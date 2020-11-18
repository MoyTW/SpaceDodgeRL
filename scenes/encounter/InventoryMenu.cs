using Godot;
using SpaceDodgeRL.scenes;
using SpaceDodgeRL.scenes.components;
using SpaceDodgeRL.scenes.entities;
using System;
using System.Collections.Generic;
using System.Linq;

public class InventoryMenu : VBoxContainer {
  private static PackedScene _inventoryPrefab = GD.Load<PackedScene>("res://scenes/encounter/InventoryEntry.tscn");

  private Button _closeButton;
  private Dictionary<string, InventoryEntry> _displayedIdsToEntries;

  public override void _Ready() {
    _closeButton = this.GetNode<Button>("Columns/CloseButton");
    _closeButton.Connect("pressed", this, nameof(OnCloseButtonPressed));

    _displayedIdsToEntries = new Dictionary<string, InventoryEntry>();
  }

  public void PrepMenu(InventoryComponent inventory) {
    _closeButton.GrabFocus();

    // Do a dumb full pass on both to diff & add/remove
    var inventoryIdsToEntities = inventory.StoredItems.ToDictionary(e => e.EntityId, e => e);

    var toRemoveThese = _displayedIdsToEntries.Where(e => !inventoryIdsToEntities.ContainsKey(e.Key)).Select(e => e.Key).ToList();
    foreach(string toRemove in toRemoveThese) {
      _displayedIdsToEntries[toRemove].QueueFree();
      _displayedIdsToEntries.Remove(toRemove);
    }

    var toAddThese = inventoryIdsToEntities.Where(e => !_displayedIdsToEntries.ContainsKey(e.Key)).ToList();
    foreach(KeyValuePair<string, Entity> held in toAddThese) {
      var newEntry = _inventoryPrefab.Instance() as InventoryEntry;
      newEntry.PopulateData(held.Key, held.Value.EntityName, "TODO: Item descriptions");

      // TODO: Put only [n] entries on left column
      GetNode<VBoxContainer>("Columns/LeftColumn").AddChild(newEntry);
      this._displayedIdsToEntries[held.Key] = newEntry;
    }
  }

  private void OnCloseButtonPressed() {
    var sceneManager = (SceneManager)GetNode("/root/SceneManager");
    sceneManager.CloseInventoryMenu();
  }
}
