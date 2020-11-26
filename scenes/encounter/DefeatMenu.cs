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

  // TODO: Figure out what you want to happen to failed/successful runs!
  private void OnMainMenuBttonPressed() {
    /* TODO: Write a morgue file and clear the slot?
    Godot.File write = new Godot.File();
    write.Open(this._state.SaveFilePath, File.ModeFlags.Write);
    write.StoreString(this._state.ToSaveData());
    write.Close();
    */

    var sceneManager = (SceneManager)GetNode("/root/SceneManager");
    sceneManager.ExitToMainMenu();
  }

  private void OnSaveAndQuitButtonPressed() {
    /* TODO: Write a morgue file and clear the slot?
    Godot.File write = new Godot.File();
    write.Open(this._state.SaveFilePath, File.ModeFlags.Write);
    write.StoreString(this._state.ToSaveData());
    write.Close();
    */

    GetTree().Quit();
  }
}
