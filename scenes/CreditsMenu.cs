using Godot;
using SpaceDodgeRL.scenes.singletons;
using System;

public class CreditsMenu : VBoxContainer {

  private Button _exitButton;

  public override void _Ready() {
    this._exitButton = this.GetNode<Button>("ExitButton");
    this._exitButton.Connect("pressed", this, nameof(OnExitButtonPressed));
    this._exitButton.GrabFocus();
  }

  public void PrepMenu() {
    this._exitButton.GrabFocus();
  }

  private void OnExitButtonPressed() {
    var sceneManager = (SceneManager)GetNode("/root/SceneManager");
    sceneManager.ExitToMainMenu();
  }
}
