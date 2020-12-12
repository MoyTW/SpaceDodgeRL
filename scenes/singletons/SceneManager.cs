using Godot;
using SpaceDodgeRL.scenes.components;
using SpaceDodgeRL.scenes.encounter;
using SpaceDodgeRL.scenes.encounter.state;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SpaceDodgeRL.scenes.singletons {

  public class SceneManager : Node {

    private Viewport root;
    private List<Node> sceneStack;

    private AutopilotMenu _autopilotMenu;
    private CharacterMenu _characterMenu;
    private DefeatMenu _defeatMenu;
    private EscapeMenu _escapeMenu;
    private InventoryMenu _inventoryMenu;
    private SettingsMenu _settingsMenu;
    private VictoryMenu _victoryMenu;
    private ReadOnlyCollection<EncounterZone> _autopilotMenuZones;

    public override void _Ready() {
      root = GetTree().Root;
      sceneStack = new List<Node>();

      _autopilotMenu = GD.Load<PackedScene>("res://scenes/encounter/AutopilotMenu.tscn").Instance() as AutopilotMenu;
      _characterMenu = GD.Load<PackedScene>("res://scenes/encounter/CharacterMenu.tscn").Instance() as CharacterMenu;
      _defeatMenu = GD.Load<PackedScene>("res://scenes/encounter/DefeatMenu.tscn").Instance() as DefeatMenu;
      _escapeMenu = GD.Load<PackedScene>("res://scenes/encounter/EscapeMenu.tscn").Instance() as EscapeMenu;
      _inventoryMenu = GD.Load<PackedScene>("res://scenes/encounter/InventoryMenu.tscn").Instance() as InventoryMenu;
      _settingsMenu = GD.Load<PackedScene>("res://scenes/SettingsMenu.tscn").Instance() as SettingsMenu;
      _victoryMenu = GD.Load<PackedScene>("res://scenes/encounter/VictoryMenu.tscn").Instance() as VictoryMenu;
    }

    #region Autopilot Menu

    public void ShowAutopilotMenu(EncounterState state) {
      _autopilotMenu.PrepMenu(state);
      CallDeferred(nameof(DeferredSwitchScene), _autopilotMenu);
    }

    public void CloseAutopilotMenu(string selectedZoneId) {
      CallDeferred(nameof(DeferredCloseAutopilotMenu), selectedZoneId);
    }

    private void DeferredCloseAutopilotMenu(string selectedZoneId) {
      var previousScene = sceneStack[sceneStack.Count - 1] as EncounterScene;
      DeferredReturnToPreviousScene();
      previousScene.HandleAutopilotMenuClosed(selectedZoneId);
    }

    #endregion

    #region Character Menu

    public void ShowCharacterMenu(EncounterState state) {
      CallDeferred(nameof(DeferredShowCharacterMenu), state);
    }

    private void DeferredShowCharacterMenu(EncounterState state) {
      DeferredSwitchScene(_characterMenu);
      _characterMenu.PrepMenu(state);
    }

    // Definitely an awkward call structuring here that could be nicer with signals
    public void HandleLevelUpSelected(string levelUpSelection) {
      var previousScene = sceneStack[sceneStack.Count - 1] as EncounterScene;
      previousScene.HandleLevelUpSelected(previousScene.EncounterState.Player, levelUpSelection);
      _characterMenu.PrepMenu(previousScene.EncounterState);
    }

    #endregion

    # region Defeat Menu

    public void ShowDefeatMenu(EncounterState state) {
      CallDeferred(nameof(DeferredShowDefeatMenu), state);
    }

    private void DeferredShowDefeatMenu(EncounterState state) {
      DeferredSwitchScene(_defeatMenu);
      _defeatMenu.PrepMenu(state);
    }

    #endregion

    #region Escape Menu

    public void ShowEscapeMenu(EncounterState state) {
      CallDeferred(nameof(DeferredShowEscapeMenu), state);
    }

    private void DeferredShowEscapeMenu(EncounterState state) {
      DeferredSwitchScene(_escapeMenu);
      _escapeMenu.PrepMenu(state);
    }

    #endregion

    #region EncounterScene

    public void ShowEncounterScene(EncounterScene scene) {
      CallDeferred(nameof(DeferredShowEncounterScene), scene);
    }

    private void DeferredShowEncounterScene(EncounterScene scene) {
      DeferredSwitchScene(scene);
    }

    #endregion

    #region Inventory Menu

    public void ShowInventoryMenu(EncounterState state) {
      CallDeferred(nameof(DeferredShowInventoryMenu), state);
    }

    private void DeferredShowInventoryMenu(EncounterState state) {
      DeferredSwitchScene(_inventoryMenu);
      _inventoryMenu.PrepMenu(state.Player.GetComponent<InventoryComponent>());
    }

    public void HandleItemToUseSelected(string itemIdToUse) {
      CallDeferred(nameof(DeferredHandleItemToUseSelected), itemIdToUse);
    }

    private void DeferredHandleItemToUseSelected(string itemIdToUse) {
      var previousScene = sceneStack[sceneStack.Count - 1] as EncounterScene;
      DeferredReturnToPreviousScene();
      previousScene.HandleItemToUseSelected(itemIdToUse);
    }

    #endregion

    #region Settings Menu

    public void ShowSettingsMenu() {
      CallDeferred(nameof(DeferredShowSettingsMenu));
    }

    private void DeferredShowSettingsMenu() {
      DeferredSwitchScene(_settingsMenu);
      this._settingsMenu.SetFocus();
    }

    #endregion

    #region Victory Menu

    public void ShowVictoryMenu(EncounterState state) {
      CallDeferred(nameof(DeferredShowVictoryMenu), state);
    }

    private void DeferredShowVictoryMenu(EncounterState state) {
      DeferredSwitchScene(_victoryMenu);
      _victoryMenu.PrepMenu(state);
    }

    #endregion

    // Plumbing
    public void ReturnToPreviousScene() {
      CallDeferred(nameof(DeferredReturnToPreviousScene));
    }

    private void DeferredReturnToPreviousScene() {
      var previousScene = sceneStack[sceneStack.Count - 1];
      sceneStack.RemoveAt(sceneStack.Count - 1);
      DeferredSwitchScene(previousScene, true);
    }

    public void ExitToMainMenu() {
      CallDeferred(nameof(DeferredExitToMainMenu));
    }

    /**
     * Drops us back to the main menu and burns the scene stack.
     */
    private void DeferredExitToMainMenu() {
      var introMenuScene = sceneStack.Find(s => s is IntroMenuScene);
      DeferredSwitchScene(introMenuScene);
      sceneStack.Clear();
      (introMenuScene as IntroMenuScene).SetFocus();
    }

    private void DeferredSwitchScene(Node scene) {
      DeferredSwitchScene(scene, false);
    }

    /**
     * Swaps scenes, operating only on the last child node of root. Saves previous scenes in SceneStack.
     */
    private void DeferredSwitchScene(Node scene, bool previous) {
      var lastScene = root.GetChild(root.GetChildCount() - 1);
      if (!previous) {
        sceneStack.Add(lastScene);
      }
      root.RemoveChild(lastScene);
      root.AddChild(scene);
    }
  }
}