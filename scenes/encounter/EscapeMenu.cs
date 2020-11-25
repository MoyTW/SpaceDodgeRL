using Godot;
using SpaceDodgeRL.scenes;
using SpaceDodgeRL.scenes.encounter.state;
using System;

public class EscapeMenu : Control {

  private Button _continueButton;
  private EncounterState _state;

  public override void _Ready() {
    this._continueButton = this.GetNode<Button>("CenterContainer/ContinueButton");
    this._continueButton.GrabFocus();
    this._continueButton.Connect("pressed", this, nameof(OnContinueButtonPressed));

    this.GetNode<Button>("CenterContainer/MainMenuButton").Connect("pressed", this, nameof(OnMainMenuBttonPressed));
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
    Godot.File write = new Godot.File();
    write.Open(this._state.SaveFilePath, File.ModeFlags.Write);
    write.StoreString(this._state.ToSaveData());
    write.Close();

    var sceneManager = (SceneManager)GetNode("/root/SceneManager");
    sceneManager.ExitToMainMenu();
  }
}
