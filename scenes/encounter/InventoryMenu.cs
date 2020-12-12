using Godot;
using SpaceDodgeRL.scenes.components;
using SpaceDodgeRL.scenes.entities;
using SpaceDodgeRL.scenes.singletons;
using System.Collections.Generic;
using System.Linq;

namespace SpaceDodgeRL.scenes.encounter {
  public class InventoryMenu : VBoxContainer {
    private static PackedScene _inventoryPrefab = GD.Load<PackedScene>("res://scenes/encounter/InventoryEntry.tscn");

    private Button _closeButton;
    private Dictionary<string, InventoryEntry> _displayedIdsToEntries;

    public override void _Ready() {
      _closeButton = GetNode<Button>("Columns/CloseButton");
      _closeButton.Connect("pressed", this, nameof(OnCloseButtonPressed));

      _displayedIdsToEntries = new Dictionary<string, InventoryEntry>();
    }

    private void DisplaySpace(InventoryComponent inventory) {
      var spaceLabel = GetNode<Label>("Header/HeaderHBox/SpaceHeader");
      spaceLabel.Text = string.Format("({0}/{1})", inventory.InventoryUsed, inventory.InventorySize);
    }

    public void PrepMenu(InventoryComponent inventory) {
      _closeButton.GrabFocus();

      DisplaySpace(inventory);

      // Do a dumb full pass on both to diff & add/remove
      var inventoryIdsToEntities = inventory.StoredItems.ToDictionary(e => e.EntityId, e => e);

      var toRemoveThese = _displayedIdsToEntries.Where(e => !inventoryIdsToEntities.ContainsKey(e.Key)).Select(e => e.Key).ToList();
      foreach (string toRemove in toRemoveThese) {
        _displayedIdsToEntries[toRemove].QueueFree();
        _displayedIdsToEntries.Remove(toRemove);
      }

      var toAddThese = inventoryIdsToEntities.Where(e => !_displayedIdsToEntries.ContainsKey(e.Key)).ToList();
      foreach (KeyValuePair<string, Entity> held in toAddThese) {
        var newEntry = _inventoryPrefab.Instance() as InventoryEntry;

        var description = held.Value.GetComponent<DisplayComponent>().Description;
        newEntry.PopulateData(held.Key, held.Value.EntityName, description);
        newEntry.Connect(nameof(InventoryEntry.UseItemSelected), this, nameof(OnUseButtonPressed), new Godot.Collections.Array { held.Key });

        _displayedIdsToEntries[held.Key] = newEntry;

        // TODO: A less fixed layout
        // TODO: When you use an item, it should collapse the column instead of leaving a hole - maybe we should just redraw it all?
        if (_displayedIdsToEntries.Count < 10) {
          GetNode<VBoxContainer>("Columns/LeftColumn").AddChild(newEntry);
        } else if (_displayedIdsToEntries.Count < 20) {
          GetNode<VBoxContainer>("Columns/MiddleColumn").AddChild(newEntry);
        } else {
          GetNode<VBoxContainer>("Columns/RightColumn").AddChild(newEntry);
        }
      }
    }

    public override void _UnhandledKeyInput(InputEventKey @event) {
      if (@event.IsActionPressed("ui_cancel", true)) {
        OnCloseButtonPressed();
        return;
      }
    }

    private void OnUseButtonPressed(string entityId) {
      var sceneManager = (SceneManager)GetNode("/root/SceneManager");
      sceneManager.HandleItemToUseSelected(entityId);
    }

    private void OnCloseButtonPressed() {
      var sceneManager = (SceneManager)GetNode("/root/SceneManager");
      sceneManager.ReturnToPreviousScene();
    }
  }
}