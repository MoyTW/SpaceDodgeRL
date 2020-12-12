using Godot;
using SpaceDodgeRL.scenes.singletons;

public class IntroMenuScene : Control {
  public override void _Ready() {
    this.GetNode<SaveSlotScene>("CenterContainer/SaveSlot1").FocusLoadButton();
    this.GetNode<Button>("CenterContainer/SettingsButton").Connect("pressed", this, nameof(OnSettingsButtonPressed));
    this.GetNode<Button>("CenterContainer/ExitButton").Connect("pressed", this, nameof(OnExitButtonPressed));
  }

  // TODO: There's a weird bug when you have some empty save files and you navigate left from a clear button
  // TODO: Focus on the file that you just came from
  public void SetFocus() {
    this.GetNode<SaveSlotScene>("CenterContainer/SaveSlot1").FocusLoadButton();
  }

  private void OnSettingsButtonPressed() {
    var sceneManager = (SceneManager)GetNode("/root/SceneManager");
    sceneManager.ShowSettingsMenu();
  }

  private void OnExitButtonPressed() {
    GetTree().Quit();
  }
}
