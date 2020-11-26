using Godot;
using SpaceDodgeRL.scenes;
using SpaceDodgeRL.scenes.encounter.state;

public class DefeatMenu : Control {

  private Button _mainMenuBotton;
  private EncounterState _state;

  public override void _Ready() {
    this._mainMenuBotton = this.GetNode<Button>("CenterContainer/MainMenuButton");
    this._mainMenuBotton.Connect("pressed", this, nameof(OnMainMenuBttonPressed));
    this.GetNode<Button>("CenterContainer/SaveAndQuitButton").Connect("pressed", this, nameof(OnSaveAndQuitButtonPressed));
  }

  public void PrepMenu(EncounterState state) {
    this._mainMenuBotton.GrabFocus();
    this._state = state;
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
