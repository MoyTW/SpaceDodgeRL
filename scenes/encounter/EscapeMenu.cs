using Godot;
using SpaceDodgeRL.scenes.encounter.state;
using SpaceDodgeRL.scenes.singletons;
using System;

public class EscapeMenu : Control {

  private Button _continueButton;
  private EncounterState _state;

  public override void _Ready() {
    this._continueButton = this.GetNode<Button>("CenterContainer/ContinueButton");
    this._continueButton.GrabFocus();
    this._continueButton.Connect("pressed", this, nameof(OnContinueButtonPressed));

    this.GetNode<Button>("CenterContainer/MainMenuButton").Connect("pressed", this, nameof(OnMainMenuBttonPressed));
    this.GetNode<Button>("CenterContainer/SaveAndQuitButton").Connect("pressed", this, nameof(OnSaveAndQuitButtonPressed));
  }

  public void PrepMenu(EncounterState state) {
    this._continueButton.GrabFocus();
    this._state = state;
  }

  private void OnContinueButtonPressed() {
    var sceneManager = (SceneManager)GetNode("/root/SceneManager");
    sceneManager.ReturnToPreviousScene();
  }

  private void OnMainMenuBttonPressed() {
    this._state.WriteToFile();
    ((SceneManager)GetNode("/root/SceneManager")).ExitToMainMenu();
  }

  private void OnSaveAndQuitButtonPressed() {
    this._state.WriteToFile();
    GetTree().Quit();
  }
}
