using Godot;
using SpaceDodgeRL.scenes.singletons;
using System;

namespace SpaceDodgeRL.scenes.encounter {
  public class HelpMenu : BaseMenu {

    private Button _closeButton;

    public override void _Ready() {
      this._closeButton = GetNode<Button>("VBoxContainer/CloseButton");
      this._closeButton.Connect("pressed", this, nameof(OnCloseButtonPressed));
    }

    public void PrepMenu() {
      this._closeButton.GrabFocus();
    }

    private void OnCloseButtonPressed() {
      var sceneManager = (SceneManager)GetNode("/root/SceneManager");
      sceneManager.ReturnToPreviousScene();
    }

    public override void HandleNeedsFocusButNoFocusSet() {
      this._closeButton.GrabFocus();
    }
  }
}