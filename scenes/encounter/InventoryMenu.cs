using Godot;
using SpaceDodgeRL.scenes;
using SpaceDodgeRL.scenes.components;
using System;

public class InventoryMenu : VBoxContainer {

  private Button _closeButton;

  public override void _Ready() {
    _closeButton = this.GetNode<Button>("Columns/CloseButton");
    _closeButton.Connect("pressed", this, nameof(OnCloseButtonPressed));
  }

  public void PrepMenu(InventoryComponent inventory) {
    _closeButton.GrabFocus();
  }

  private void OnCloseButtonPressed() {
    var sceneManager = (SceneManager)GetNode("/root/SceneManager");
    sceneManager.CloseInventoryMenu();
  }
}
