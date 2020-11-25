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
    private EscapeMenu _escapeMenu;
    private InventoryMenu _inventoryMenu;
    private ReadOnlyCollection<EncounterZone> _autopilotMenuZones;

    public override void _Ready() {
      root = this.GetTree().Root;
      sceneStack = new List<Node>();

      this._autopilotMenu = GD.Load<PackedScene>("res://scenes/encounter/AutopilotMenu.tscn").Instance() as AutopilotMenu;
      this._characterMenu = GD.Load<PackedScene>("res://scenes/encounter/CharacterMenu.tscn").Instance() as CharacterMenu;
      this._escapeMenu = GD.Load<PackedScene>("res://scenes/encounter/EscapeMenu.tscn").Instance() as EscapeMenu;
      this._inventoryMenu = GD.Load<PackedScene>("res://scenes/encounter/InventoryMenu.tscn").Instance() as InventoryMenu;
    }

    #region Autopilot Menu

    public void ShowAutopilotMenu(EncounterState state) {
      this._autopilotMenu.PrepMenu(state);
      CallDeferred(nameof(DeferredSwitchScene), this._autopilotMenu);
    }

    public void CloseAutopilotMenu(string selectedZoneId) {
      CallDeferred(nameof(DeferredCloseAutopilotMenu), selectedZoneId);
    }

    private void DeferredCloseAutopilotMenu(string selectedZoneId) {
      var previousScene = sceneStack[sceneStack.Count - 1] as EncounterScene;
      sceneStack.RemoveAt(sceneStack.Count - 1);

      previousScene.HandleAutopilotMenuClosed(selectedZoneId);
      DeferredSwitchScene(previousScene);
    }

    #endregion

    #region Character Menu

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

    #endregion

    #region Escape Menu

    public void ShowEscapeMenu() {
      CallDeferred(nameof(DeferredShowEscapeMenu));
    }

    private void DeferredShowEscapeMenu() {
      DeferredSwitchScene(this._escapeMenu);
      this._escapeMenu.PrepMenu();
    }

    #endregion

    #region Inventory Menu

    public void ShowInventoryMenu(EncounterState state) {
      CallDeferred(nameof(DeferredShowInventoryMenu), state);
    }

    private void DeferredShowInventoryMenu(EncounterState state) {
      DeferredSwitchScene(this._inventoryMenu);
      this._inventoryMenu.PrepMenu(state.Player.GetComponent<InventoryComponent>());
    }

    public void HandleItemToUseSelected(string itemIdToUse) {
      CallDeferred(nameof(DeferredHandleItemToUseSelected), itemIdToUse);
    }

    private void DeferredHandleItemToUseSelected(string itemIdToUse) {
      var previousScene = sceneStack[sceneStack.Count - 1] as EncounterScene;
      sceneStack.RemoveAt(sceneStack.Count - 1);

      DeferredSwitchScene(previousScene);
      previousScene.HandleItemToUseSelected(itemIdToUse);
    }

    #endregion

    // Plumbing
    public void ReturnToPreviousScene() {
      CallDeferred(nameof(DeferredReturnToPreviousScene));
    }

    private void DeferredReturnToPreviousScene() {
      var previousScene = sceneStack[sceneStack.Count - 1];
      sceneStack.RemoveAt(sceneStack.Count - 1);
      DeferredSwitchScene(previousScene);
    }

    private void DeferredSwitchScene(Node scene) {
      var lastScene = root.GetChild(root.GetChildCount() - 1);
      this.sceneStack.Add(lastScene);
      root.RemoveChild(lastScene);
      root.AddChild(scene);
    }
  }
}