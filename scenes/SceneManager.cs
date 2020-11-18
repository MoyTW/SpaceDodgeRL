using Godot;
using SpaceDodgeRL.scenes.components;
using SpaceDodgeRL.scenes.encounter.state;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SpaceDodgeRL.scenes {

  public class SceneManager : Node {

    private Viewport root;
    private List<Node> sceneStack;

    private AutopilotMenu _autopilotMenu;
    private CharacterMenu _characterMenu;
    private InventoryMenu _inventoryMenu;
    private ReadOnlyCollection<EncounterZone> _autopilotMenuZones;

    public override void _Ready() {
      root = this.GetTree().Root;
      sceneStack = new List<Node>();

      this._autopilotMenu = GD.Load<PackedScene>("res://scenes/encounter/AutopilotMenu.tscn").Instance() as AutopilotMenu;
      this._characterMenu = GD.Load<PackedScene>("res://scenes/encounter/CharacterMenu.tscn").Instance() as CharacterMenu;
      this._inventoryMenu = GD.Load<PackedScene>("res://scenes/encounter/InventoryMenu.tscn").Instance() as InventoryMenu;
    }

    // Autopilot Menu
    public void ShowAutopilotMenu(EncounterState state) {
      this._autopilotMenu.PrepMenu(state);
      CallDeferred(nameof(DeferredSwitchScene), this._autopilotMenu);
    }

    public void CloseAutopilotMenu(EncounterZone selectedZone) {
      CallDeferred(nameof(DeferredCloseAutopilotMenu), selectedZone);
    }

    private void DeferredCloseAutopilotMenu(EncounterZone selectedZone) {
      var previousScene = sceneStack[sceneStack.Count - 1] as EncounterScene;
      sceneStack.RemoveAt(sceneStack.Count - 1);

      previousScene.HandleAutopilotMenuClosed(selectedZone);
      DeferredSwitchScene(previousScene);
    }

    // Character Menu
    public void ShowCharacterMenu(EncounterState state) {
      CallDeferred(nameof(DeferredShowCharacterMenu), state);
    }

    private void DeferredShowCharacterMenu(EncounterState state) {
      DeferredSwitchScene(this._characterMenu);
      this._characterMenu.PrepMenu(state);
    }

    // Definitely an awkward call structuring here that could be nicer with signals
    public void HandleLevelUpSelected(string levelUpSelection) {
      var previousScene = sceneStack[sceneStack.Count - 1] as EncounterScene;
      previousScene.HandleLevelUpSelected(previousScene.EncounterState.Player, levelUpSelection);
      this._characterMenu.PrepMenu(previousScene.EncounterState);
    }

    public void CloseCharacterMenu() {
      CallDeferred(nameof(DeferredCloseCharacterMenu));
    }

    private void DeferredCloseCharacterMenu() {
      var previousScene = sceneStack[sceneStack.Count - 1] as EncounterScene;
      sceneStack.RemoveAt(sceneStack.Count - 1);
      DeferredSwitchScene(previousScene);
    }

    #region Inventory Menu

    public void ShowInventoryMenu(EncounterState state) {
      CallDeferred(nameof(DeferredShowInventoryMenu), state);
    }

    private void DeferredShowInventoryMenu(EncounterState state) {
      DeferredSwitchScene(this._inventoryMenu);
      this._inventoryMenu.PrepMenu(state.Player.GetComponent<InventoryComponent>());
    }

    // TODO: Merge with CloseCharacterMenu into generic "no other effects go back to encounter"
    public void CloseInventoryMenu() {
      CallDeferred(nameof(DeferredCloseInventoryMenu));
    }

    private void DeferredCloseInventoryMenu() {
      var previousScene = sceneStack[sceneStack.Count - 1] as EncounterScene;
      sceneStack.RemoveAt(sceneStack.Count - 1);
      DeferredSwitchScene(previousScene);
    }

    public void CloseInventoryMenu(string itemIdToUse) {
      CallDeferred(nameof(DeferredCloseInventoryMenu), itemIdToUse);
    }

    private void DeferredCloseInventoryMenu(string itemIdToUse) {
      var previousScene = sceneStack[sceneStack.Count - 1] as EncounterScene;
      sceneStack.RemoveAt(sceneStack.Count - 1);

      DeferredSwitchScene(previousScene);
      previousScene.HandleInventoryMenuClosed(itemIdToUse);
    }

    #endregion

    // Plumbing
    private void DeferredSwitchScene(Node scene) {
      var lastScene = root.GetChild(root.GetChildCount() - 1);
      this.sceneStack.Add(lastScene);
      root.RemoveChild(lastScene);
      root.AddChild(scene);
    }
  }
}