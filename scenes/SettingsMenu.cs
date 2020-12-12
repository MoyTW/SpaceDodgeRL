using Godot;
using SpaceDodgeRL.scenes.singletons;
using System;

namespace SpaceDodgeRL.scenes {

  public class SettingsMenu : VBoxContainer {

    public override void _Ready() {
      this.GetNode<Button>("SaveAndExitButton").Connect("pressed", this, nameof(OnSaveAndExitButtonPressed));
    }

    // TODO: Actually save & load
    private void OnSaveAndExitButtonPressed() {
      var sceneManager = (SceneManager)GetNode("/root/SceneManager");
      sceneManager.ReturnToPreviousScene();
    }
  }
}