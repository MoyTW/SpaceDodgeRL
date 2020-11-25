using Godot;

public class IntroMenuScene : Control {
  public override void _Ready() {
    this.GetNode<SaveSlotScene>("CenterContainer/SaveSlot1").FocusLoadButton();
  }

  // TODO: There's a weird bug when you have some empty save files and you navigate left from a clear button
  // TODO: Focus on the file that you just came from
  public void SetFocus() {
    this.GetNode<SaveSlotScene>("CenterContainer/SaveSlot1").FocusLoadButton();
  }
}
