using Godot;
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
    private ReadOnlyCollection<EncounterZone> _autopilotMenuZones;

    public override void _Ready() {
      root = this.GetTree().Root;
      sceneStack = new List<Node>();

      this._autopilotMenu = GD.Load<PackedScene>("res://scenes/encounter/AutopilotMenu.tscn").Instance() as AutopilotMenu;
      this._characterMenu = GD.Load<PackedScene>("res://scenes/encounter/CharacterMenu.tscn").Instance() as CharacterMenu;
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
      this._characterMenu.PrepMenu(state);
      CallDeferred(nameof(DeferredSwitchScene), this._characterMenu);
    }

    public void CloseCharacterMenu() {
      CallDeferred(nameof(DeferredCloseCharacterMenu));
    }

    private void DeferredCloseCharacterMenu() {
      var previousScene = sceneStack[sceneStack.Count - 1] as EncounterScene;
      sceneStack.RemoveAt(sceneStack.Count - 1);
      DeferredSwitchScene(previousScene);
    }

    // Plumbing
    private void DeferredSwitchScene(Node scene) {
      var lastScene = root.GetChild(root.GetChildCount() - 1);
      this.sceneStack.Add(lastScene);
      root.RemoveChild(lastScene);
      root.AddChild(scene);
    }
  }
}