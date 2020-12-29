using Godot;
using SpaceDodgeRL.scenes.components;
using SpaceDodgeRL.scenes.entities;
using SpaceDodgeRL.scenes.singletons;
using System.Collections.Generic;
using System.Linq;

namespace SpaceDodgeRL.scenes.encounter {
  public class InventoryMenu : TextureRect {
    private static PackedScene _inventoryPrefab = GD.Load<PackedScene>("res://scenes/encounter/InventoryEntry.tscn");

    private Button _closeButton;
    public List<InventoryEntry> _inventoryEntries;
    private bool _currentlyHovered;
    private Button _lastHovered;

    public override void _Ready() {
      _closeButton = GetNode<Button>("Columns/CloseButton");
      _closeButton.Connect("pressed", this, nameof(OnCloseButtonPressed));
      _closeButton.Connect("mouse_entered", this, nameof(OnMouseEntered), new Godot.Collections.Array() { _closeButton });
      _closeButton.Connect("mouse_exited", this, nameof(OnMouseExited), new Godot.Collections.Array() { _closeButton });

      _inventoryEntries = new List<InventoryEntry>();
    }

    private void DisplaySpace(InventoryComponent inventory) {
      var spaceLabel = GetNode<Label>("Header/HeaderHBox/SpaceHeader");
      spaceLabel.Text = string.Format("({0}/{1})", inventory.InventoryUsed, inventory.InventorySize);
    }

    public override void _Input(InputEvent @event) {
      if (@event is InputEventMouseMotion && Input.GetMouseMode() != Input.MouseMode.Visible) {
        Input.SetMouseMode(Input.MouseMode.Visible);
        this.GetFocusOwner().ReleaseFocus();
        if (this._currentlyHovered) {
          this._lastHovered.GrabFocus();
        }
      } else if (@event is InputEventKey && Input.GetMouseMode() == Input.MouseMode.Visible) {
        Input.SetMouseMode(Input.MouseMode.Hidden);
        if (this.GetFocusOwner() == null && this._lastHovered != null) {
          this._lastHovered.GrabFocus();
        } else if (this.GetFocusOwner() == null && this._lastHovered == null) {
          this._closeButton.GrabFocus();
        }
      }
    }

    private void ResizeEntriesToInventorySize(int inventorySize) {
      if (_inventoryEntries.Count != inventorySize) {
        foreach (var entry in _inventoryEntries) {
          entry.QueueFree();
        }
        _inventoryEntries.Clear();

        for (int i = 0; i < inventorySize; i++) {
          var newEntry = _inventoryPrefab.Instance() as InventoryEntry;
          newEntry.Connect("mouse_entered", this, nameof(OnMouseEntered), new Godot.Collections.Array() { newEntry });
          newEntry.Connect(nameof(InventoryEntry.UseItemSelected), this, nameof(OnUseButtonPressed), new Godot.Collections.Array { newEntry });
          _inventoryEntries.Add(newEntry);

          if (i < 9) {
            GetNode<VBoxContainer>("Columns/LeftColumn").AddChild(newEntry);
          } else if (i < 18) {
            GetNode<VBoxContainer>("Columns/MiddleColumn").AddChild(newEntry);
          } else {
            GetNode<VBoxContainer>("Columns/RightColumn").AddChild(newEntry);
          }
        }
      }
    }

    public void PrepMenu(InventoryComponent inventory) {
      _closeButton.GrabFocus();

      DisplaySpace(inventory);
      ResizeEntriesToInventorySize(inventory.InventorySize);

      var storedItems = inventory.StoredItems.ToList();
      for (int i = 0; i < _inventoryEntries.Count; i++) {
        if (i < storedItems.Count) {
          _inventoryEntries[i].Show();
          var item = storedItems[i];
          _inventoryEntries[i].SetData(item.EntityId, item.EntityName, item.GetComponent<DisplayComponent>().Description);
        } else {
          _inventoryEntries[i].Hide();
        }
      }
    }

    public override void _UnhandledKeyInput(InputEventKey @event) {
      if (@event.IsActionPressed("ui_cancel", true)) {
        OnCloseButtonPressed();
        return;
      }
    }

    private void OnMouseEntered(Button entered) {
      this._currentlyHovered = true;
      this._lastHovered = entered;
    }

    private void OnMouseExited(Button exited) {
      this._currentlyHovered = false;
    }

    private void OnUseButtonPressed(InventoryEntry entryPressed) {
      var sceneManager = (SceneManager)GetNode("/root/SceneManager");
      sceneManager.HandleItemToUseSelected(entryPressed.EntityId);
    }

    private void OnCloseButtonPressed() {
      var sceneManager = (SceneManager)GetNode("/root/SceneManager");
      sceneManager.ReturnToPreviousScene();
    }
  }
}