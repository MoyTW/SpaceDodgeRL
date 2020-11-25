using Godot;
using SpaceDodgeRL.scenes;
using System;

public class EscapeMenu : Control {

  private Button _continueButton;

  public override void _Ready() {
    this._continueButton = this.GetNode<Button>("CenterContainer/ContinueButton");
    this._continueButton.GrabFocus();
    this._continueButton.Connect("pressed", this, nameof(OnContinueButtonPressed));
  }

  public void PrepMenu() {
    this._continueButton.GrabFocus();
  }

  private void OnContinueButtonPressed() {
    var sceneManager = (SceneManager)GetNode("/root/SceneManager");
    sceneManager.ReturnToPreviousScene();
  }
}
